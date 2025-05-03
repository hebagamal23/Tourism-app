namespace Tourism_project.Services
{
    public interface IEmailVerificationService
    {

        Task VerifyEmailAsync(string email);

    }
}
