using CollaborativeSoftware;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CollaborativeSoftware
{
    public static class EmailService
    {
        public static async Task Send2FACodeAsync(string toEmail, string code)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    "quizelyangs@gmail.com",
                    //Password123
                    "oeoh xemh qpeb xoza"
                )
            };

            var mail = new MailMessage
            {
                From = new MailAddress("YOUR_EMAIL@gmail.com", "Quizelyangs"),
                Subject = "Your Quizelyangs Verification Code",
                Body = $"Your verification code is: {code}\n\nThis code expires in 5 minutes. Heavy Gewtz.",
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);

            await smtpClient.SendMailAsync(mail);
        }
    }
}
