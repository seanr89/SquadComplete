using System.Net.Mail;

public class EmailSMTPService
{
    private readonly string _email;
    private readonly string _password;
    public EmailSMTPService()
    {
        _email = Environment.GetEnvironmentVariable("SMTP_SENDER") ?? "srafferty89@gmail.com";
        _password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
    }

    /// <summary>
    /// Sends an email to the specified recipient with the given subject and body.
    /// </summary>
    /// <param name="recipient">The email address of the recipient.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <returns>Returns true if the email was sent successfully, otherwise returns false.</returns>
    /// <exception cref="Exception">Catches any exception that occurs during the email sending process and logs it to the console.</exception>
    public void SendEmail(string recipient, string subject, string body, bool isHtml = false)
    {
        try
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("srafferty89@gmail.com");
            mail.To.Add(recipient);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isHtml;

            //Enable SMTP configuration - password is empty for testing purposes atm
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(_email, _password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}