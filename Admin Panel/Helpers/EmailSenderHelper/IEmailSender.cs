namespace AdminPanel.Helpers.EmailSenderHelper
{
    public interface IEmailSender
    {
        void SendEmail(EmailMessage message);
    }
}
