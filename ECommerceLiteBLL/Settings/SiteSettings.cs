using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ECommerceLiteEntity.ViewModels;

namespace ECommerceLiteBLL.Settings
{
    public class SiteSettings
    {
        //To Do:Mail adresini webconfig dosyasından cekmeyı de ogrenelım 
        public static string SiteMail { get; set; } = "nayazilim303@gmail.com";
        public static string SiteMailPassword { get; set; } = "betul303303";
        public static string SiteMailSmtpHost { get; set; } = "smtp.gmail.com";
        public static int SiteMailSmtpPort { get; set; } = 587;
        public static bool SiteMailEnableSSL = true;

        public async static Task SendMail(MailModel model)
        {
            try
            {
                using (var smtp=new SmtpClient())
                {
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(model.To));
                    message.From=new MailAddress(SiteMail);
                    message.Subject = model.Subject;
                    message.IsBodyHtml = true;
                    message.Body = model.Message;
                    message.BodyEncoding = Encoding.UTF8;
                    if(!string.IsNullOrEmpty(model.Cc))//modeldeki cc boş değilse
                    {
                        message.CC.Add(new MailAddress(model.Cc));
                    }
                    if (!string.IsNullOrEmpty(model.Bcc))//modeldeki cc boş değilse
                    {
                        message.Bcc.Add(new MailAddress(model.Bcc));
                    }
                    var networkCredentials = new NetworkCredential()
                    { 
                        UserName=SiteMail,
                        Password=SiteMailPassword
                    };

                    smtp.Credentials = networkCredentials;
                    smtp.Host = SiteMailSmtpHost;
                    smtp.Port = SiteMailSmtpPort;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);




                }
            }
            catch (Exception ex )
            {

                throw ex;
                //To Do:ex loglanacak
            }
        }

        public static string StringCharacterConverter(string name)
        {
            string resultString=name
            .Replace("'", "")
            .Replace(" ", "-")
            .Replace("<", "")
            .Replace(">", "")
            .Replace("&", "")
            .Replace("[", "")
            .Replace("!", "")
            .Replace("]", "")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ç", "c")
            .Replace("ğ", "g")
            .Replace("İ", "I")
            .Replace("Ö", "O")
            .Replace("Ü", "U")
            .Replace("Ş", "S")
            .Replace("Ç", "C")
            .Replace("Ğ", "G")
            .Replace("|", "")
            .Replace(".", "-")
            .Replace("?", "-")
            .Replace(";", "-");

            return resultString.ToLower();
        }
    }
}
