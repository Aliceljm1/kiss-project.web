using System.Configuration;

namespace Kiss.Web.Auth.Configuration
{
    /// <summary>
    /// Represents the section in the host's .config file that configures
    /// this library's settings.
    /// </summary>
    // [ContractVerification(true)]
    public class AuthSection : ConfigurationSection
    {
        /// <summary>
        /// The name of the section under which this library's settings must be found.
        /// </summary>
        private const string SectionName = "auth";

        /// <summary>
        /// The name of the &lt;messaging&gt; sub-element.
        /// </summary>
        private const string MessagingElementName = "messaging";

        /// <summary>
        /// The name of the &lt;openid&gt; sub-element.
        /// </summary>
        private const string OpenIdElementName = "openid";

        /// <summary>
        /// Initializes a new instance of the <see cref="Kiss.Web.AuthSection"/> class.
        /// </summary>
        internal AuthSection()
        {
            this.SectionInformation.AllowLocation = false;
        }

        /// <summary>
        /// Gets the configuration section from the .config file.
        /// </summary>
        public static AuthSection Configuration
        {
            get
            {
                return (AuthSection)ConfigurationManager.GetSection(SectionName) ?? new AuthSection();
            }
        }

        /// <summary>
        /// Gets or sets the configuration for the messaging framework.
        /// </summary>
        [ConfigurationProperty(MessagingElementName)]
        public MessagingElement Messaging
        {
            get
            {
                return (MessagingElement)this[MessagingElementName] ?? new MessagingElement();
            }

            set
            {
                this[MessagingElementName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the configuration for OpenID.
        /// </summary>
        [ConfigurationProperty(OpenIdElementName)]
        internal OpenIdElement OpenId
        {
            get
            {
                return (OpenIdElement)this[OpenIdElementName] ?? new OpenIdElement();
            }

            set
            {
                this[OpenIdElementName] = value;
            }
        }
    }
}
