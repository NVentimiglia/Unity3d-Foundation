using System.Configuration;

namespace Foundation.Server.Infrastructure
{
    /// <summary>
    /// Custom Strongly Typed Web.Config Settings
    /// </summary>
    public class AppConfig : ConfigurationSection
    {
        // ReSharper disable InconsistentNaming
        private static readonly AppConfig _settings = ConfigurationManager.GetSection("AppConfig") as AppConfig;
        // ReSharper restore InconsistentNaming

        public static AppConfig Settings
        {
            get
            {
                return _settings;
            }
        }

        [ConfigurationProperty("AppDomain", DefaultValue = "http://unity3dFoundation.com")]
        public string AppDomain
        {
            get { return (string)this["AppDomain"]; }
        }

        [ConfigurationProperty("AppName", DefaultValue = "Simple Systems")]
        public string AppName
        {
            get { return (string)this["AppName"]; }
        }

        [ConfigurationProperty("NoReplyEmail", DefaultValue = "noreply@unity3dFoundation.com")]
        public string NoReplyEmail
        {
            get { return (string)this["NoReplyEmail"]; }
        }

        [ConfigurationProperty("DisableEmail", DefaultValue = "false")]
        public bool DisableEmail
        {
            get { return (bool)this["DisableEmail"]; }
        }
        
        /// <summary>
        /// AES key for account system.
        /// </summary>
        [ConfigurationProperty("AESKey", DefaultValue = "D6DB280F32C885C3527CA8A114203BBB8E288659DCDD81E022982194CD21801A")]
        public string AesKey
        {
            get { return (string)this["AESKey"]; }
        }

        /// <summary>
        /// FacebookId
        /// </summary>
        [ConfigurationProperty("FacebookId", DefaultValue = "")]
        public string FacebookId
        {
            get { return (string)this["FacebookId"]; }
        }

        /// <summary>
        /// FacebookSecret
        /// </summary>
        [ConfigurationProperty("FacebookSecret", DefaultValue = "")]
        public string FacebookSecret
        {
            get { return (string)this["FacebookSecret"]; }
        }

        public bool FacebookEnabled
        {
            get { return !string.IsNullOrEmpty(FacebookId) && !string.IsNullOrEmpty(FacebookSecret); }
        }


        /// <summary>
        /// FacebookId
        /// </summary>
        [ConfigurationProperty("RealtimeId", DefaultValue = "")]
        public string RealtimeId
        {
            get { return (string)this["RealtimeId"]; }
        }

        /// <summary>
        /// FacebookSecret
        /// </summary>
        [ConfigurationProperty("RealtimeSecret", DefaultValue = "")]
        public string RealtimeSecret
        {
            get { return (string)this["RealtimeSecret"]; }
        }

        public bool RealtimeEnabled
        {
            get { return !string.IsNullOrEmpty(RealtimeSecret) && !string.IsNullOrEmpty(RealtimeId); }
        }

        /// <summary>
        /// Application Password
        /// </summary>
        [ConfigurationProperty("ApplicationId", DefaultValue = "")]
        public string ApplicationId
        {
            get { return (string)this["ApplicationId"]; }
        }

    }
}
