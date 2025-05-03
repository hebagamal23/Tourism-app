namespace Tourism_project.Services
{
    public interface IEmailServices
    {
         Task SendVerificationEmail( string toEmail,  string subject, string textBody, string htmlBody);
    }
}
