using System;
using System.Configuration;
using System.Diagnostics;
using Twilio;

namespace Foundation.Server.Infrastructure
{
    public class TwilioMessage
    {
        public string SentToPhone { get; set; }
        public string Message { get; set; }
    }

    public class TwilioHelper
    {
        static readonly string AccountSid;
        static readonly string AuthToken;
        static readonly string twilioPhoneNumber;

        static TwilioHelper()
        {
            AccountSid = ConfigurationManager.AppSettings["twilioAccountSid"];
            AuthToken = ConfigurationManager.AppSettings["twilioAuthToken"];
            twilioPhoneNumber = ConfigurationManager.AppSettings["twilioPhoneNumber"];
        }

        public static void SendMessage(TwilioMessage msg)
        {
            try
            {
                var twilio = new TwilioRestClient(AccountSid, AuthToken);

                twilio.SendMessage(twilioPhoneNumber, msg.SentToPhone, msg.Message, message =>
                {
                    Trace.TraceInformation("Trilio message sent to " + message.To);
                });
            }
            catch (Exception ex)
            {

                Trace.TraceError("Failed to send Twilio Message " + ex.Message, ex);
            }

        }
    }
}
