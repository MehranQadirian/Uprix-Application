using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Classes.Core_Classes
{
    public class EmailSender
    {
        public void SendActivationCode(string toEmail, string activationCode)
        {
            try
            {
                string fromEmail = "YOUR-GMAIL";
                string fromPassword = "<YOUR-SECRET-PASS>";

                string htmlBody = $@"
<!DOCTYPE html>
<html lang='fa'>
<head>
<meta charset='UTF-8'>
<style>
    body {{
        margin: 0;
        padding: 0;
        background-color: #f3f4f6;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        direction: rtl;
    }}
    .container {{
        max-width: 520px;
        background-color: #ffffff;
        margin: 40px auto;
        padding: 35px 40px;
        border-radius: 20px;
        box-shadow: 0 10px 28px rgba(0, 0, 0, 0.12);
        border: 1px solid #e5e7eb;
        text-align: center;
    }}
    .brand {{
        font-size: 20px;
        font-weight: 700;
        color: #1e3a8a;
        text-align: left;
        margin-bottom: 10px;
    }}
    .icon {{
        font-size: 42px;
        color: #2563eb;
        margin-bottom: 15px;
    }}
    h2 {{
        color: #111827;
        font-weight: 700;
        font-size: 26px;
        margin-bottom: 25px;
    }}
    .code {{
        display: inline-block;
        background: linear-gradient(135deg, #3b82f6, #1d4ed8);
        color: #fff;
        font-weight: 900;
        font-size: 34px;
        letter-spacing: 6px;
        padding: 18px 42px;
        border-radius: 14px;
        box-shadow: 0 6px 14px rgba(29, 78, 216, 0.5);
        user-select: all;
        margin-bottom: 30px;
        font-family: 'Courier New', Courier, monospace;
    }}
    p {{
        color: #374151;
        font-size: 15px;
        line-height: 1.7;
        margin: 0 0 18px 0;
    }}
    .footer {{
        font-size: 13px;
        color: #9ca3af;
        margin-top: 25px;
        border-top: 1px solid #e5e7eb;
        padding-top: 15px;
    }}
    .footer i {{
        margin-left: 5px;
        color: #6b7280;
    }}
    @media only screen and (max-width: 540px) {{
        .container {{
            margin: 20px 15px;
            padding: 25px 20px;
        }}
        h2 {{
            font-size: 22px;
        }}
        .code {{
            font-size: 26px;
            padding: 15px 30px;
            letter-spacing: 3px;
        }}
        p {{
            font-size: 13px;
        }}
    }}
</style>
</head>
<body>
    <div class='container'>
        <div class='brand'>🔹 Uprix</div>
        <div class='icon'>🔒</div>
        <h2>Your activation code</h2>
        <div class='code'>{activationCode}</div>
        <p>This code is valid for a limited time only. Please enter it in the software..</p>
        <p>If this request was not made by you, please ignore this message..</p>
        <div class='footer'>
            <i>©</i> All rights reserved - Uprix {DateTime.Now.Year} {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}
        </div>
    </div>
</body>
</html>
";



                // تنظیمات SMTP
                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true
                };

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Activation Service"),
                    Subject = "Your activation code",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                smtp.Send(message);

                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
