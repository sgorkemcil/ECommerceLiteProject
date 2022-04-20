using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repository;
using ECommerceLiteUI.Models;
using Mapster;

namespace ECommerceLiteUI.Controllers
{
    public class HomeController : Controller
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        public ActionResult Index()
        {
            //Ana kategorilerilerden 4 tanesini viewbag ile sayfaya gönderelim
            var categoryList = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId == null).Take(4).ToList();

            ViewBag.CategoryList = categoryList.OrderByDescending(x => x.Id).ToList();

            //ürünler
            var productList = myProductRepo.AsQueryable().Where(x => x.IsDeleted == false && x.Quantity >= 1).Take(10).ToList();
            //x.IsDeleted == false ile !x.IsDeleted bu aynıdır
            List<ProductViewModel> model = new List<ProductViewModel>();

            //Mapster ile mapledik 
            //Foreach Linq sorgusuu
            productList.ForEach(x =>
            {
                var item = x.Adapt<ProductViewModel>();
                item.GetCategory();
                item.GetProductPictures();
                model.Add(item);
            });
            
            

            
            //foreach (var item in productList)
            //{
                
            //    //model.Add(item.Adap<ProductViewModel>());
            //    var product = new ProductViewModel()
            //    {
            //        Id = item.Id,
            //        CategoryId = item.CategoryId,
            //        ProductName = item.ProductName,
            //        Description = item.Description,
            //        Quantity = item.Quantity,
            //        Discount = item.Discount,
            //        RegisterDate = item.RegisterDate,
            //        Price = item.Price,
            //        ProductCode = item.ProductCode
            //        //isDeleted alanını viewmodelin içine eklemeyi unuttuk.
            //        //çünkü: isDeleted alanını daha dün ekledik. Viewmodeli geçen hafta oluşturdu.
            //    };

            //    product.GetCategory();
            //    product.GetProductPictures();
            //    model.Add(product);
            //}

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult AddToCart(int id)
        {
            try
            {
                //Session'a eklenecek
                //Session oturum demektir.
                var shoppingCart = Session["ShoppingCart"] as List<ProductViewModel>;
                if (shoppingCart==null)
                {
                    shoppingCart = new List<ProductViewModel>();

                }
                if (id>0)
                {
                    var product = myProductRepo.GetById(id);

                    if (product==null)
                    {
                        TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır.Lütfen tekrar deneyiniz";
                        //product null geldi logla
                        return RedirectToAction("Index", "Home");

                    }
                    //tamam ekleme yapılacak
                    //var productAddToCart = product.Adapt<ProductViewModel>();
                    var productAddToCart = new ProductViewModel()
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        CategoryId = product.CategoryId,
                        Discount = product.Discount,
                        Price = product.Price,
                        Quantity = product.Quantity,
                        RegisterDate = product.RegisterDate,
                        ProductCode = product.ProductCode,
                    };
                    if (shoppingCart.Count(x=>x.Id==product.Id)>0)
                    {
                        shoppingCart.FirstOrDefault(x => x.Id == productAddToCart.Id).Quantity++;
                        
                    }
                    else
                    {
                        productAddToCart.Quantity = 1;
                        shoppingCart.Add(productAddToCart);

                    }
                    //ÖNEMLİ-->Session'a bu listeyi atamamız gerekli..
                    Session["shoppingCart"] = shoppingCart;
                    TempData["AddToCartSuccess"] = "Ürün sepete eklendi";
                    return RedirectToAction("Index", "Home");


                }
                else
                {
                    TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır.Lütfen tekrar deneyiniz";
                    //Loglama yap id düzgün gelmedi
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                //ex loglanacak
                TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır.Lütfen tekrar deneyiniz";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}