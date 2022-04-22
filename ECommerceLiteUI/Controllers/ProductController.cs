using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repository;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Mapster;

namespace ECommerceLiteUI.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ProductController : Controller
    {                                                                                                                                                                                                                                                
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        ProductPictureRepo myProductPictureRepo = new ProductPictureRepo();
        private const int pageSize = 5;



       //Bu controllera Admin gibi yetkili kişiler erişecektir.
       //Buarada ürünlerin listelenmesi ,ekleme,silme,güncelleme işlemleri yapılacaktır.
        public ActionResult ProductList(int? page = 1, string search = "")
        {
            //Alt Kategorileri repo aracılığıyla dbden çektik
            ViewBag.SubCategories = GetSubCategories();
            //Sayfaya bazı bilgiler göndereceğiz
            var totalProduct = myProductRepo.GetAll().Count;//toplam ürün sayısı
            ViewBag.TotalProduct = totalProduct;//toplam ürün sayısını sayfaya göndereceğiz
            ViewBag.TotalPages = (int)Math.Ceiling(totalProduct / (double)pageSize);

            //toplamürün/sayfada gösterilecek üründen kaç sayfa olduğu bilgisi
            ViewBag.PageSize = pageSize;//Her sayfada kaç ürün gözükecek bilgisini html sayfasına gönderelim

            //frenleme
            //1.yöntem
            if(page<1)
            {
                page = 1;
            }
            if (page>ViewBag.TotalPages)
            {
                page = ViewBag.TotalPages;
            }
            //2.yöntem
            page = page < 1 ? //eğer page 1'den küçükse 
                       1 :    //page'in değerini 1 yap
                       page > ViewBag.TotalPages ?  //değilse bak bakalım page toplam sayfadan büyük mü
                                              ViewBag.TotalPages  // page değeri=toplam sayfa
                                              :
                                              page;  //page'e dokunma page aynı değerinden devamkee...



            ViewBag.CurrentPage = page;//View'de kaçıncı sayfada olduğum bilgiisni tutsun

            List<Product>allProducts = new List<Product>();
           
            //var allProducts = myProductRepo.GetAll();
            if (string.IsNullOrEmpty(search))
            {
             allProducts = myProductRepo.GetAll();
            }
            else
            {
                allProducts = myProductRepo.GetAll()
                    .Where(x => x.ProductName.Contains(search) || x.Description.Contains(search)).ToList();
            }

            //Paging-->1.yöntem bu yontem en klasık yontemdir.
            allProducts = allProducts.Skip(
                (page.Value < 1 ? 1 : page.Value - 1)
                * pageSize
                )
                .Take(pageSize)//10 take al neden 10?Çünkü yukarıdaki pagesize 10'a eşitmiş
                .ToList();
          
            
            return View(allProducts);

        }                                                                                                                                                                                                                                                                                                                        
        [HttpGet]
        public ActionResult Create()
        {
            //Sayfayı Çağırırken ürünün kategorisinin ne olduğunu seçmek lazım .
            //Bu nedenle sayfaya kategoriler gitmeli
            //Linq
            //select*from Categories where BaseCategoryId is not null
            List<SelectListItem> subCategories = new List<SelectListItem>();
            
            GetSubCategories().ForEach(x => subCategories.Add(new SelectListItem()
            {
                Text = x.CategoryName,
                Value = x.Id.ToString()
            }));

            ViewBag.SubCategories = subCategories;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel model)
        {
            try
            {
                List<SelectListItem> subCategories = new List<SelectListItem>();

                GetSubCategories().ForEach(x => subCategories.Add(new SelectListItem()
                {
                    Text = x.CategoryName,
                    Value = x.Id.ToString()
                }));

                ViewBag.SubCategories = subCategories;
                              

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Veri girişleri düzgün olmalıdır.");
                    return View(model);
                }
                
                if (model.CategoryId<=0 )
                {
                    ModelState.AddModelError("", "Ürüne ait kategori seçilmelidir.!");
                    return View(model);

                }

                //Burada kontrol lazım
                //Acaba girdiği ürün kodu bizim db de zaten var mı ?
                //metotlu 
                if (myProductRepo.IsSameProductCode(model.ProductCode))
                {
                    ModelState.AddModelError("", "Dikkat! Girdiğiniz ürün kodu sistemdeki bir başka ürüne aittir.Ürün kodları benzersiz olmalıdır.");
                    return View(model);
                }



                //Ürün tabloya kayıt olacak .
                //TODO:Mapleme yapılacak

                Product product = new Product()
                {
                    ProductName = model.ProductName,
                    Description = model.Description,
                    ProductCode = model.ProductCode,
                    CategoryId = model.CategoryId,
                    Discount = model.Discount,
                    Quantity = model.Quantity,
                    RegisterDate = DateTime.Now,
                    Price = model.Price

                };
                //mapleme yapıldı
                //Mapster paketi indirildi.Mapster bir objedeki verileri diğer bir objeye zahmetsizce aktarır.
                //Aktarım yapabilmesi için A objesiyl B objesinin içindeki propertylerin isimleri ve tipleri birebir aynı olmalıdır.
                //bu projede mapster kullandık
                //Core projesınde daha profesyenel olan AutoMapper'ı kullanacagız
                //bir dto objesinin içindeki verileri alır asıl objenin içine aktarır.Asıl objenın verılerını dto objesının ıcındekı propertylere aktarır.
                //Product product = model.Adapt<Product>();
                //Product product2=model.Adapt<ProductViewModel,Product>();


                int insertResult = myProductRepo.Insert(product);
                if(insertResult>0)
                {
                    //Sıfırdan büyükse product tabloya eklendi.
                    //Acaba bu producta resim seçmiş mi?resim seçtiyse o resimlerin yollarını kaydet 
                    if (model.Files.Any() && model.Files[0] !=null)
                    {
                        int pictureinsertResult = 0;
                        foreach (var item in model.Files)
                        {
                            if (item!=null && item.ContentType.Contains("image") && item.ContentLength>0)
                            {
                                string productName = SiteSettings.StringCharacterConverter(model.ProductName).ToLower().Replace("-", "");
                                string extensionName = Path.GetExtension(item.FileName);
                                string directoryPath=Server.MapPath
                                    ($"~/ProductPictures/{productName}/{model.ProductCode}");
                                string guid = Guid.NewGuid().ToString().Replace("-", "");
                                //ProductPictures/iphone13/20202020/iphone13-guid.jpg
                                string filePath=Server.MapPath
                                    ($"~/ProductPictures/{productName}/{model.ProductCode}"+
                                    $"{productName}-{guid}{extensionName}");
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                //resmi o klasöre kayıt edelim
                                item.SaveAs(filePath);
                                //işlem bitti DB'ye kayıt olacak
                                ProductPicture picture = new ProductPicture()
                                {
                                    ProductId = product.Id,
                                    RegisterDate = DateTime.Now,
                                    Picture = $"/ProductPictures/{productName}/{model.ProductCode}" +
                                    $"{productName}-{guid}{extensionName}",
                                    IsDeleted = false
                                };
                                pictureinsertResult += myProductPictureRepo.Insert(picture);
                                
                            }

                        }
                        //pictureinsertResult kontrol edilecektir.
                        if (pictureinsertResult>0 && model.Files.Count==pictureinsertResult)
                        {
                            //Bütün resimler eklenmiş
                            TempData["ProductInsertSuccess"] = "Yeni ürün eklenmiştir.";
                            return RedirectToAction("ProductList", "Product");
                        }
                        else if(pictureinsertResult>0 && model.Files.Count!=pictureinsertResult)
                        {
                            //eksik eklemiş
                            TempData["ProductInsertWarning"] = "Yeni ürün eklendi ama resimlerden bazıları beklenmedik bir sorun yüzünden eklenemedi!Eklenilenemeyen resimleri daha sonra tekrar ekleyiniz.";
                            return RedirectToAction("ProductList", "Product");
                        }
                        else
                        {
                            //ürünü ekledi ama resimleri eklemedi
                            TempData["ProductInsertWarning"] = "Yeni ürün eklendi ama ürüne ait resimler eklenemedi.Resimleri daha sonra tekrar eklemeyi deneyiniz.";
                            return RedirectToAction("ProductList", "Product");
                        }

                    }
                    else
                    {
                        TempData["ProductInsertSuccess"] = "Yeni ürün eklenmiştir.";
                        return RedirectToAction("ProductList", "Product");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "HATA:Ürün Ekleme işleminde bir hata oluştu!Tekrar deneyiniz");
                    return View(model);

                }


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu");
                //ex loglanacak
                return View(model);
                
            }
        }

        public JsonResult GetProductDetails(int id)
        {
            try
            {
                var product = myProductRepo.GetById(id);
                if (product!=null)
                {
                    //var data = product.Adapt<ProductViewModel>();
                    var data = new ProductViewModel()
                    {
                        Id=product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        ProductCode = product.ProductCode,
                        CategoryId = product.CategoryId,
                        Discount = product.Discount,
                        Quantity = product.Quantity,
                        RegisterDate = product.RegisterDate,
                        Price = product.Price

                    };
                    return Json(new { iSSuccess =true,data},JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { iSSuccess = false });
                }
            }
            catch (Exception)
            {
                //ex loglansın
                return Json(new { iSSuccess = false });
            }
        }

        public ActionResult Edit(ProductViewModel model)
        {
            try
            {
                var product = myProductRepo.GetById(model.Id);
                if (product != null)
                {
                    product.ProductName = model.ProductName;
                    product.Description = model.Description;
                    product.Discount = model.Discount;
                    product.Quantity = model.Quantity;
                    product.ProductCode = model.ProductCode;
                    product.Price = model.Price;
                    product.CategoryId = model.CategoryId;

                    int updateResult = myProductRepo.Update();
                    if (updateResult > 0)
                    {
                        TempData["EditSuccess"] = "Bilgiler başarıyla güncellendi.";
                        return RedirectToAction("ProductList", "Product");
                    }
                    else
                    {
                        TempData["EditFailed"] = "Beklenmedik bir hata olduğu için ürün bilgileri sisteme aktarılamadı";
                        return RedirectToAction("ProductList", "Product");
                    }

                }
                else
                {
                    TempData["EditFailed"] = "Ürün bulunamadığı için bilgileri güncellenemedi!";
                    return RedirectToAction("ProductList", "Product");
                }

            }
            catch (Exception)
            {

                //ex loglanacak
                TempData["EditFailed"] = "Beklenmedik bir hata nedeniyle ürün güncellenemedi!";
                return RedirectToAction("ProductList", "Product");
            }
        }

            public List<Category> GetSubCategories()
            {
            //Alt kategorisi olmasına rağmen, product ekleme sayfasındaki comboya gelen kategoriler var.Bu bugı bir metot ekleyerek çözümledik.
            //Mateoda kategorinin id si kategori tablosunda basacategoryid alanında geçiyorsa continue ile o kategoriyi atladık ve listemize
            //almadık.Böylece sadece çocuk kategoriler gelecektir.Ebeveyn kategoriler sayfaya gelemeyecektir.
                //eğer caterpo.asq().where(x => x.bas == id)
                //continue;
            List<Category> returnList = new List<Category>();
                var categoryList = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList();
                foreach (var item in categoryList)
                {
                    if (myCategoryRepo.AsQueryable().Count(x => x.BaseCategoryId == item.Id) > 0)
                    {
                        continue;
                    }
                    else
                    {
                        returnList.Add(item);
                    }
                }


                return returnList;
            }
        }
        

    }
