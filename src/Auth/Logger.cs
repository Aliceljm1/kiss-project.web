//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth
{
    using System;

    using System.Globalization;
    using Kiss;

    /// <summary>
    /// A general logger for the entire Kiss.Web.Auth library.
    /// </summary>
    /// <remarks>
    /// Because this logger is intended for use with non-localized strings, the
    /// overloads that take <see cref="CultureInfo"/> have been removed, and 
    /// <see cref="CultureInfo.InvariantCulture"/> is used implicitly.
    /// </remarks>
    internal static partial class Logger
    {
        #region Category-specific loggers

        /// <summary>
        /// Backing field for the <see cref="Yadis"/> property.
        /// </summary>
        private static readonly ILogger yadis = Create("Kiss.Web.Auth.Yadis");

        /// <summary>
        /// Backing field for the <see cref="Messaging"/> property.
        /// </summary>
        private static readonly ILogger messaging = Create("Kiss.Web.Auth.Messaging");

        /// <summary>
        /// Backing field for the <see cref="Channel"/> property.
        /// </summary>
        private static readonly ILogger channel = Create("Kiss.Web.Auth.Messaging.Channel");

        /// <summary>
        /// Backing field for the <see cref="Bindings"/> property.
        /// </summary>
        private static readonly ILogger bindings = Create("Kiss.Web.Auth.Messaging.Bindings");

        /// <summary>
        /// Backing field for the <see cref="Signatures"/> property.
        /// </summary>
        private static readonly ILogger signatures = Create("Kiss.Web.Auth.Messaging.Bindings.Signatures");

        /// <summary>
        /// Backing field for the <see cref="Http"/> property.
        /// </summary>
        private static readonly ILogger http = Create("Kiss.Web.Auth.Http");

        /// <summary>
        /// Backing field for the <see cref="Controls"/> property.
        /// </summary>
        private static readonly ILogger controls = Create("Kiss.Web.Auth.Controls");

        /// <summary>
        /// Backing field for the <see cref="OpenId"/> property.
        /// </summary>
        private static readonly ILogger openId = Create("Kiss.Web.Auth.OpenId");

        /// <summary>
        /// Backing field for the <see cref="OAuth"/> property.
        /// </summary>
        private static readonly ILogger oauth = Create("Kiss.Web.Auth.OAuth");

        /// <summary>
        /// Backing field for the <see cref="InfoCard"/> property.
        /// </summary>
        private static readonly ILogger infocard = Create("Kiss.Web.Auth.InfoCard");

        /// <summary>
        /// Gets the logger for service discovery and selection events.
        /// </summary>
        internal static ILogger Yadis { get { return yadis; } }

        /// <summary>
        /// Gets the logger for Messaging events.
        /// </summary>
        internal static ILogger Messaging { get { return messaging; } }

        /// <summary>
        /// Gets the logger for Channel events.
        /// </summary>
        internal static ILogger Channel { get { return channel; } }

        /// <summary>
        /// Gets the logger for binding elements and binding-element related events on the channel.
        /// </summary>
        internal static ILogger Bindings { get { return bindings; } }

        /// <summary>
        /// Gets the logger specifically used for logging verbose text on everything about the signing process.
        /// </summary>
        internal static ILogger Signatures { get { return signatures; } }

        /// <summary>
        /// Gets the logger for HTTP-level events.
        /// </summary>
        internal static ILogger Http { get { return http; } }

        /// <summary>
        /// Gets the logger for events logged by ASP.NET controls.
        /// </summary>
        internal static ILogger Controls { get { return controls; } }

        /// <summary>
        /// Gets the logger for high-level OpenID events.
        /// </summary>
        internal static ILogger OpenId { get { return openId; } }

        /// <summary>
        /// Gets the logger for high-level OAuth events.
        /// </summary>
        internal static ILogger OAuth { get { return oauth; } }

        /// <summary>
        /// Gets the logger for high-level InfoCard events.
        /// </summary>
        internal static ILogger InfoCard { get { return infocard; } }

        #endregion

        /// <summary>
        /// Creates an additional logger on demand for a subsection of the application.
        /// </summary>
        /// <param name="name">A name that will be included in the log file.</param>
        /// <returns>The <see cref="ILog"/> instance created with the given name.</returns>
        internal static ILogger Create(string name)
        {
            // // // Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(name));
            return LogManager.GetLogger(name);
        }

        /// <summary>
        /// Creates an additional logger on demand for a subsection of the application.
        /// </summary>
        /// <param name="type">A type whose full name that will be included in the log file.</param>
        /// <returns>The <see cref="ILog"/> instance created with the given type name.</returns>
        internal static ILogger Create(Type type)
        {
            // // // Contract.Requires<ArgumentNullException>(type != null);

            return Create(type.FullName);
        }
    }
}
