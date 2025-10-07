using Amazon.SimpleEmailV2.Model;
using Amazon.SimpleEmailV2;

namespace Fitnutri.Application.Email
{
    public class AwsSesEmailSender : IEmailSender
    {
        private readonly IAmazonSimpleEmailServiceV2 _ses;
        private readonly string _from;

        public AwsSesEmailSender(IAmazonSimpleEmailServiceV2 ses, IConfiguration cfg)
        {
            _ses = ses;
            _from = cfg["Email:From"] ?? throw new InvalidOperationException("Email:From não configurado.");
        }

        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        {
            var request = new SendEmailRequest
            {
                FromEmailAddress = _from,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { to }
                },
                Content = new EmailContent
                {
                    Simple = new Message
                    {
                        Subject = new Content { Data = subject },
                        Body = new Body
                        {
                            Html = new Content { Data = htmlBody }
                        }
                    }
                }
            };

            await _ses.SendEmailAsync(request, ct);
        }
    }
}
