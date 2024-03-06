using API.Mastery.Udemy.Configuration;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace API.Mastery.Udemy.Services
{
    public class EmailService : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        public EmailService(IOptions<SmtpSettings> smtpsenttigs)
        {
            _smtpSettings = smtpsenttigs.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();

                //Mailbox Adress  
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                //Aca es a quien va destinado el mail.
                message.To.Add(new MailboxAddress("",email));
                message.Subject = subject;
                //Este es el texto que se lo pasamos por parametro.
                message.Body = new TextPart("html") { Text = htmlMessage};
                

                //Se abre con using para asegurarnos que van a ser eliminadas cuando termina el metodo.
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server);
                    await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                
            }

            catch (Exception)
            {
                throw;
            }
        }
    }
}
