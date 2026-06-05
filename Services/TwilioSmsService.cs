using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ReserveBag.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string to, string message);
    }

    public class TwilioSmsService : ISmsService
    {
        private readonly IConfiguration _config;

        public TwilioSmsService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendSmsAsync(string to, string message)
        {
            var accountSid = _config ["Twilio:AccountSid"];
            var authToken = _config ["Twilio:AuthToken"];
            var fromNumber = _config ["Twilio:FromNumber"];

            TwilioClient.Init (accountSid, authToken);

            await MessageResource.CreateAsync (
                to: new PhoneNumber (to),
                from: new PhoneNumber (fromNumber),
                body: message
            );
        }
    }
}