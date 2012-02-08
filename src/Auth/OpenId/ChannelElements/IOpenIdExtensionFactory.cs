//-----------------------------------------------------------------------
// <copyright file="IOpenIdExtensionFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth.OpenId.ChannelElements {
	using System.Collections.Generic;
	using Kiss.Web.Auth.Messaging;
	using Kiss.Web.Auth.OpenId.Messages;
	using Kiss.Web.Auth.OpenId.Provider;
	using Kiss.Web.Auth.OpenId.RelyingParty;

	/// <summary>
	/// OpenID extension factory class for creating extensions based on received Type URIs.
	/// </summary>
	/// <remarks>
	/// OpenID extension factories must be registered with the library.  This can be
	/// done by adding a factory to <see cref="OpenIdRelyingParty.ExtensionFactories"/>
	/// or <see cref="OpenIdProvider.ExtensionFactories"/>, or by adding a snippet
	/// such as the following to your web.config file:
	/// <example>
	///   &lt;Kiss.Web.Auth&gt;
	///     &lt;openid&gt;
	///       &lt;extensionFactories&gt;
	///         &lt;add type="Kiss.Web.Auth.ApplicationBlock.CustomExtensions.Acme, Kiss.Web.Auth.ApplicationBlock" /&gt;
	///       &lt;/extensionFactories&gt;
	///     &lt;/openid&gt;
	///   &lt;/Kiss.Web.Auth&gt;
	/// </example>
	/// </remarks>
	public interface IOpenIdExtensionFactory {
		/// <summary>
		/// Creates a new instance of some extension based on the received extension parameters.
		/// </summary>
		/// <param name="typeUri">The type URI of the extension.</param>
		/// <param name="data">The parameters associated specifically with this extension.</param>
		/// <param name="baseMessage">The OpenID message carrying this extension.</param>
		/// <param name="isProviderRole">A value indicating whether this extension is being received at the OpenID Provider.</param>
		/// <returns>
		/// An instance of <see cref="IOpenIdMessageExtension"/> if the factory recognizes
		/// the extension described in the input parameters; <c>null</c> otherwise.
		/// </returns>
		/// <remarks>
		/// This factory method need only initialize properties in the instantiated extension object
		/// that are not bound using <see cref="MessagePartAttribute"/>.
		/// </remarks>
		IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage, bool isProviderRole);
	}
}
