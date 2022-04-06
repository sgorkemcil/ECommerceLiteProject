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
                // aktivasyon kodu üretelim
                var activationCode = Guid.NewGuid().ToString().Replace("-", "");

                //Artık sisteme kayıt olabilir...
                var newUser = new ApplicationUser()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.TCNumber,
                    ActivationCode=activationCode
                };
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
                //To Do:Loglama yapılacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu!Tekrar deneyiniz!");
                return View(model);
                
            }
        }

        [HttpGet]
        public async Task<ActionResult>Activation(string code)
        {
            try
            {
                //select*from AspNetUsers where Activationcode='sdkfjsdlfsdfsdfklsdfk'
                var user =
                    myUserStore.Context.Set<ApplicationUser>()
                    .FirstOrDefault(x => x.ActivationCode == code);
                if (user==null)
                {
                    ViewBag.ActivationResult = "Aktivasyon işlemi başarısız!sistem yöneticisinden yeniden email isteyiniz..";
                    return View();
                }
                //user bulundu !
                if (user.EmailConfirmed)//zaten aktifleşmiş mi?
                {
                    ViewBag.ActivationResult = "Aktivasyon işlemimiz zaten gerçekleşmiştir!Giriş yaparak sistemi kullanabilirsiniz.";
                    return View();
                }
                user.EmailConfirmed = true;
                await myUserStore.UpdateAsync(user);
                await myUserStore.Context.SaveChangesAsync();
                //Bu kişi artık aktif.
                PassiveUser passiveUser = myPassiveUserRepo.AsQueryable().FirstOrDefault(x => x.UserId == user.Id);
                if (passiveUser!=null)
                {
                    //TODO:PassiveUser tablosuna TargetRole ekleme işlemini daha sonra yapalım.Kafalarındaki soru işareti gittikten sonra ...
                    passiveUser.IsDeleted = true;
                    myPassiveUserRepo.Update();

                    Customer customer = new Customer()
                    {
                        UserId = user.Id,
                        TCNumber = passiveUser.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };

                    await myCustomerRepo.InsertAsync(customer);
                    //Aspnetuserrole tablosuna bu kişinin artık customer mertebesine ulaştığını bildirelim 
                    myUserManager.RemoveFromRole(user.Id, Roles.Passive.ToString());
                    myUserManager.AddToRole(user.Id, Roles.Customer.ToString());
                    //işlem bitti başarı old.dair mesajı gönderelim .

                    ViewBag.ActivationResukt = $"Merhaba Sayın {user.Name}{user.Surname},aktifleştirme işleminiz başarılıdır!Giriş yapıp sistemi kullanabilirsiniz";
                    return View();
                    


                }
                //NOT:Müsait olduğunuzda bireysel beyin fırtınası yapabilirsiniz.
                //Kendinize şu soruyu sorun!PassiveUser null gelirse nasıl bir yol izlenebilir??
                //PassiveUser null gelmesi çok büyük bir problem mi ?
                //Customerda bu kişi kayıtlı mı ?Custumerda bir problem yok...Customerda kayıtlı değilse PROBLEM VAR !

                return View();
            }
            catch (Exception ex)
            {

                //TODO:loglama yapılacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu!");
                return View();
            }
        }
    }
}