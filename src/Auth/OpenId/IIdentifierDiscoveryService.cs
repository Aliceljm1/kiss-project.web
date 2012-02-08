﻿//-----------------------------------------------------------------------
// <copyright file="IIdentifierDiscoveryService.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	
	using System.Linq;
	using System.Text;
	using Kiss.Web.Auth.Messaging;
	using Kiss.Web.Auth.OpenId.RelyingParty;

	/// <summary>
	/// A module that provides discovery services for OpenID identifiers.
	/// </summary>
	// [ContractClass(typeof(IIdentifierDiscoveryServiceContract))]
	public interface IIdentifierDiscoveryService {
		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <param name="requestHandler">The means to place outgoing HTTP requests.</param>
		/// <param name="abortDiscoveryChain">if set to <c>true</c>, no further discovery services will be called for this identifier.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By design")]
		// [Pure]
		IEnumerable<IdentifierDiscoveryResult> Discover(Identifier identifier, IDirectWebRequestHandler requestHandler, out bool abortDiscoveryChain);
	}

	/// <summary>
	/// Code contract for the <see cref="IIdentifierDiscoveryService"/> interface.
	/// </summary>
	// [ContractClassFor(typeof(IIdentifierDiscoveryService))]
	internal abstract class IIdentifierDiscoveryServiceContract : IIdentifierDiscoveryService {
		/// <summary>
		/// Prevents a default instance of the <see cref="IIdentifierDiscoveryServiceContract"/> class from being created.
		/// </summary>
		private IIdentifierDiscoveryServiceContract() {
		}

		#region IDiscoveryService Members

		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <param name="requestHandler">The means to place outgoing HTTP requests.</param>
		/// <param name="abortDiscoveryChain">if set to <c>true</c>, no further discovery services will be called for this identifier.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		IEnumerable<IdentifierDiscoveryResult> IIdentifierDiscoveryService.Discover(Identifier identifier, IDirectWebRequestHandler requestHandler, out bool abortDiscoveryChain) {
			// // Contract.Requires<ArgumentNullException>(identifier != null);
			// // Contract.Requires<ArgumentNullException>(requestHandler != null);
			// Contract.Ensures(Contract.Result<IEnumerable<IdentifierDiscoveryResult>>() != null);
			throw new NotImplementedException();
		}

		#endregion
	}
}
