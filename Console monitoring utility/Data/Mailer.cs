using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Console_monitoring_utility.Data
{
    public class Mailer
    {
        public MailMessage Message { get; set; }
        public SmtpClient SmtpClient { get; private set; }

        [JsonConstructor]
        public Mailer(string fromAddress, string displayName, string password,
            List<string> toAddress, string subject, string host, int port, bool ssl)
        {
            try
            {
                Message = new MailMessage
                {
                    From = new MailAddress(fromAddress, displayName),
                    Subject = subject,
                };
                toAddress.ForEach(address => Message.To.Add(address));

                SmtpClient = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = ssl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress, password),
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
