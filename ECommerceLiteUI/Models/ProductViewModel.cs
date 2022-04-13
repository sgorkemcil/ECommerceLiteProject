using ECommerceLiteEntity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using ECommerceLiteBLL.Repository;

namespace ECommerceLiteUI.Models
{
    public class ProductViewModel
    {
        //product tablosunun ilişkili olduğu repolardaki bilgileri çekmek amacıyla ihtiyac olan 2 repo kullanılıyor.
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductPictureRepo myproductPictureRepo = new ProductPictureRepo();
        public int Id { get; set; }
        public DateTime RegisterDate { get; set; }

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "Ürün adı 2 ile 100 karakter aralığında olmalıdır.")]
        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; }

        [Required]
        [StringLength(maximumLength: 500, ErrorMessage = "Ürün açıklaması en fazla 500 karakter olmalıdır.")]
        [Display(Name = "Ürün Açıklaması")]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Ürün Açıklaması")]
        [StringLength(maximumLength: 8, MinimumLength = 8, ErrorMessage = "Ürün kodu en fazla 8 karakter olmalıdır.")]
        [Index(IsUnique = true)]//Benzersiz tekrarsız olmasını sağlar

        public string ProductCode { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }

        
        public int CategoryId { get; set; }
        
        public Category Category { get; set; }

        public List<ProductPicture> ProductPictureList { get; set; }
        = new List<ProductPicture>();

        //Ürün eklenirken ürüne ait resimler seçilebilir.

        //Seçilen resimleri hafıza tutacak property

        public List<HttpPostedFileBase> Files { get; set; } = new List<HttpPostedFileBase>();

        public void GetProductPictures()
        {
            if (Id>0)
            {
                ProductPictureList = myproductPictureRepo.AsQueryable().Where(x => x.ProductId == Id).ToList();
            }
            
        }
        public void GetCategory()
        {
            if (CategoryId>0)
            {
                //ÖRN:Elektronik kat.-->Akıllı Telefon kat.-->ürün(iphone 13 pro max)

                Category = myCategoryRepo.GetById(CategoryId);
                //Akıllı telefon kat artık elimde
                //Akıllı telefon kat.bir üst kategori var mı?
                //ÖRN :Elek-->Akıllı tel.-->applegiller-->
                if (Category.BaseCategoryId !=null && Category.BaseCategoryId>0)
                {
                    Category.CategoryList = new List<Category>();
                    Category.BaseCategory = myCategoryRepo.GetById(Category.BaseCategoryId.Value);
                    Category.CategoryList.Add(Category.BaseCategory);

                    bool isOver = false;
                    Category baseCategory = Category.BaseCategory;
                    while(!isOver)
                    {
                        if(Category.BaseCategory.BaseCategoryId>0)
                        {
                            Category.CategoryList.Add(myCategoryRepo.GetById(baseCategory.BaseCategoryId.Value));
                            baseCategory = myCategoryRepo.GetById(baseCategory.BaseCategoryId.Value);
                        }
                        else 
                        {
                            isOver = true;
                        }
                    }

                    Category.CategoryList = Category.CategoryList.OrderBy(x => x.Id).ToList();

                }

            }
        }

    }
}