﻿//-----------------------------------------------------------------------
// <copyright file="IRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	
	using System.Text;
	using Kiss.Web.Auth.Messaging;
	using Kiss.Web.Auth.OpenId.Messages;

	/// <summary>
	/// Represents an incoming OpenId authentication request.
	/// </summary>
	/// <remarks>
	/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
	/// be authentication requests where the Provider site has to make decisions based
	/// on its own user database and policies.
	/// </remarks>
	// [ContractClass(typeof(IRequestContract))]
	public interface IRequest {
		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		bool IsResponseReady { get; }

		/// <summary>
		/// Gets or sets the security settings that apply to this request.
		/// </summary>
		/// <value>Defaults to the <see cref="OpenIdProvider.SecuritySettings"/> on the <see cref="OpenIdProvider"/>.</value>
		ProviderSecuritySettings SecuritySettings { get; set; }

		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		/// <param name="extension">The extension to add to the response message.</param>
		void AddResponseExtension(IOpenIdMessageExtension extension);

		/// <summary>
		/// Removes any response extensions previously added using <see cref="IRequest.AddResponseExtension"/>.
		/// </summary>
		/// <remarks>
		/// This should be called before sending a negative response back to the relying party
		/// if extensions were already added, since negative responses cannot carry extensions.
		/// </remarks>
		void ClearResponseExtensions();

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		/// <returns>An instance of the extension initialized with values passed in with the request.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter to make of type T.")]
		T GetExtension<T>() where T : IOpenIdMessageExtension, new();

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>An instance of the extension initialized with values passed in with the request.</returns>
		IOpenIdMessageExtension GetExtension(Type extensionType);
	}

	/// <summary>
	/// Code contract for the <see cref="IRequest"/> interface.
	/// </summary>
	// [ContractClassFor(typeof(IRequest))]
	internal abstract class IRequestContract : IRequest {
		/// <summary>
		/// Prevents a default instance of the <see cref="IRequestContract"/> class from being created.
		/// </summary>
		private IRequestContract() {
		}

		#region IRequest Members

		/// <summary>
		/// Gets or sets the security settings that apply to this request.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="OpenIdProvider.SecuritySettings"/> on the <see cref="OpenIdProvider"/>.
		/// </value>
		ProviderSecuritySettings IRequest.SecuritySettings {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		bool IRequest.IsResponseReady {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		/// <param name="extension">The extension to add to the response message.</param>
		void IRequest.AddResponseExtension(IOpenIdMessageExtension extension) {
			// // Contract.Requires<ArgumentNullException>(extension != null);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes any response extensions previously added using <see cref="IRequest.AddResponseExtension"/>.
		/// </summary>
		/// <remarks>
		/// This should be called before sending a negative response back to the relying party
		/// if extensions were already added, since negative responses cannot carry extensions.
		/// </remarks>
		void IRequest.ClearResponseExtensions() {
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		T IRequest.GetExtension<T>() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		IOpenIdMessageExtension IRequest.GetExtension(Type extensionType) {
			// // Contract.Requires<ArgumentNullException>(extensionType != null);
			throw new NotImplementedException();
		}

		#endregion
	}
}
