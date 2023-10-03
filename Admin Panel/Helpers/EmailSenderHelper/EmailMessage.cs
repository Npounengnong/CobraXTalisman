using MailKit.Net.Smtp;
using MimeKit;

namespace AdminPanel.Helpers.EmailSenderHelper
{
    public class EmailMessage
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }

        public EmailMessage(IEnumerable<EmailAddressEntity> to, string subject, string? content, string? title)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(x.DisplayName, x.Address)));
            Subject = subject;
            Content = content;
            Title = title;
        }
    }
}
