//-----------------------------------------------------------------------
// <copyright file="IRelyingPartyApplicationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Kiss.Web.Auth.Messaging.Bindings;
	using Kiss.Web.Auth.OpenId.ChannelElements;

	/// <summary>
	/// A hybrid of all the store interfaces that a Relying Party requires in order
	/// to operate in "smart" mode.
	/// </summary>
	public interface IRelyingPartyApplicationStore : IAssociationStore<Uri>, INonceStore {
	}
}
