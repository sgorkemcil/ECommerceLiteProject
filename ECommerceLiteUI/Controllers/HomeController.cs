﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repository;
using ECommerceLiteUI.Models;

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
            foreach (var item in productList)
            {
                //mapster :S
                //model.Add(item.Adap<ProductViewModel>());
                var product = new ProductViewModel()
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    ProductName = item.ProductName,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Discount = item.Discount,
                    RegisterDate = item.RegisterDate,
                    Price = item.Price,
                    ProductCode = item.ProductCode
                    //isDeleted alanını viewmodelin içine eklemeyi unuttuk.
                    //çünkü: isDeleted alanını daha dün ekledik. Viewmodeli geçen hafta oluşturdu.
                };

                product.GetCategory();
                product.GetProductPictures();
                model.Add(product);
            }

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
    }
}