using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Account;
using ECommerceLiteBLL.Repository ;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.IdentityModels;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ECommerceLiteEntity.Enums;
using System.Threading.Tasks;
using ECommerceLiteEntity.ViewModels;

namespace ECommerceLiteUI.Controllers
{
    public class AccountController : BaseController
    {
        //Global alan
        //Not:Bir sonraki projede repoları UI'ın içinde NEW'leyeceğiz!
        //Çünkü bu bağımlılık oluşturur! Bir sonraki projede bağımlılıkları tersine çevirme işlemi olarak 
        //bilinen Dependency Injection işlemleri yapacağız.

        CustomerRepo myCustomerRepo = new CustomerRepo();
        PassiveUserRepo myPassiveUserRepo = new PassiveUserRepo();
        UserManager<ApplicationUser> myUserManager = MembershipTools.NewUserManager();
        UserStore<ApplicationUser> myUserStore = MembershipTools.NewUserStore();
        RoleManager<ApplicationRole> myRoleManager = MembershipTools.NewRoleManager();

        [HttpGet]
        public ActionResult Register ()
        {
            //Kayıt ol sayfası
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]//Güvenliği sağlamak amacıyla bot hesaplardan bu metoda erişilmesin diye
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)//model validasyonları sağladı mı ?
                {
                    return View(model);

                }
                var checkUserTC = myUserStore.Context.Set<Customer>()
                    .FirstOrDefault(x => x.TCNumber == model.TCNumber)?.TCNumber;
                if (checkUserTC!=null)//Buldu!!!
                {
                    ModelState.AddModelError("", "Bu TC numarası ile daha önceden sisteme kayıt yapılmıştır.");
                    return View(model);

                }
                //To DO:soru işareti silinip debuglanakacak
                var checkUserEmail = myUserStore.Context.Set<ApplicationUser>()
                    .FirstOrDefault(x => x.Email == model.Email)?.Email;
                if (checkUserEmail != null)//Buldu!!!
                {
                    ModelState.AddModelError("", "Bu email ile daha önceden sisteme kayıt yapılmıştır.");
                    return View(model);
                }

                //Artık sisteme kayıt olabilir...
                var newUser = new ApplicationUser()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.TCNumber
                };
                // aktivasyon kodu üretelim
                var activationCode = Guid.NewGuid().ToString().Replace("-", "");
                //artık ekleyelim

                var createResult = myUserManager.CreateAsync(newUser, model.Password);

                //To Do: createResult.Isfault ne acaba ? debuglarken bakalım..
                if (createResult.Result.Succeeded)
                {
                    //görev başarıyla tammalandıysa kişi aspnetusers tablosuna eklenmiştir.!
                    //Yeni kayıt olduğu için bu kişiye pasif rol verilecektir.
                    //Kişi emailine gelen aktivasyon koduna tıklarsa pasiflikten çıkıp customer olabilir.

                    await myUserManager.AddToRoleAsync(newUser.Id, Roles.Passive.ToString());
                    PassiveUser myPassiveUser = new PassiveUser()
                    {
                        UserId = newUser.Id,
                        TCNumber = model.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };
                    // myPassiveUserRepo.Insert(myPassiveUser);
                    await myPassiveUserRepo.InsertAsync(myPassiveUser);
                    //email gönderilecek
                    //site adresini alıyoruz.
                    var siteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host +
                        (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    await SiteSettings.SendMail(new MailModel()
                    {
                        To = newUser.Email,
                        Subject = "EcommerceLite Site Aktivasyon Emaili",
                        Message = $"Merhaba{newUser.Name}{newUser.Surname}," +
                        $"<br/>Hesabınızı aktifleştirmek için <b>"+
                        $"<a href='{siteURL}/Account/Activation?"+
                        $"code={activationCode}'>Aktivasyon Linkine</a></b> tıklayınız..."
                    });
                    //işlemler bitti...
                    return RedirectToAction("Login", "Account", new { email = $"{newUser.Email}" });
                }
                else
                {
                    ModelState.AddModelError("", "Kayıt işleminde beklenmedik bir hata oluştu!");
                    return View(model);
                }

            }
            catch (Exception ex)
            {

                
            }
        }


    }
}