//-----------------------------------------------------------------------
// <copyright file="IAssociationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth.OpenId {
	using System;
	

	/// <summary>
	/// An enumeration that can specify how a given <see cref="Association"/> is used.
	/// </summary>
	public enum AssociationRelyingPartyType {
		/// <summary>
		/// The <see cref="Association"/> manages a shared secret between
		/// Provider and Relying Party sites that allows the RP to verify
		/// the signature on a message from an OP.
		/// </summary>
		Smart,

		/// <summary>
		/// The <see cref="Association"/> manages a secret known alone by
		/// a Provider that allows the Provider to verify its own signatures
		/// for "dumb" (stateless) relying parties.
		/// </summary>
		Dumb
	}

	/// <summary>
	/// Stores <see cref="Association"/>s for lookup by their handle, keeping
	/// associations separated by a given distinguishing factor (like which server the
	/// association is with).
	/// </summary>
	/// <typeparam name="TKey">
	/// <see cref="System.Uri"/> for consumers (to distinguish associations across servers) or
	/// <see cref="AssociationRelyingPartyType"/> for providers (to distinguish dumb and smart client associations).
	/// </typeparam>
	/// <remarks>
	/// Expired associations should be periodically cleared out of an association store.
	/// This should be done frequently enough to avoid a memory leak, but sparingly enough
	/// to not be a performance drain.  Because this balance can vary by host, it is the
	/// responsibility of the host to initiate this cleaning.
	/// </remarks>
	////// [ContractClass(typeof(IAssociationStoreContract<>))]
	public interface IAssociationStore<TKey> {
		/// <summary>
		/// Saves an <see cref="Association"/> for later recall.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for providers).</param>
		/// <param name="association">The association to store.</param>
		/// <remarks>
		/// TODO: what should implementations do on association handle conflict?
		/// </remarks>
		void StoreAssociation(TKey distinguishingFactor, Association association);

		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="securityRequirements">The security requirements that the returned association must meet.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </returns>
		/// <remarks>
		/// In the event that multiple associations exist for the given 
		/// <paramref name="distinguishingFactor"/>, it is important for the 
		/// implementation for this method to use the <paramref name="securityRequirements"/>
		/// to pick the best (highest grade or longest living as the host's policy may dictate)
		/// association that fits the security requirements.
		/// Associations that are returned that do not meet the security requirements will be
		/// ignored and a new association created.
		/// </remarks>
		Association GetAssociation(TKey distinguishingFactor, SecuritySettings securityRequirements);

		/// <summary>
		/// Gets the association for a given key and handle.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="handle">The handle of the specific association that must be recalled.</param>
		/// <returns>The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key and handle.</returns>
		Association GetAssociation(TKey distinguishingFactor, string handle);

		/// <summary>Removes a specified handle that may exist in the store.</summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="handle">The handle of the specific association that must be deleted.</param>
		/// <returns>True if the association existed in this store previous to this call.</returns>
		/// <remarks>
		/// No exception should be thrown if the association does not exist in the store
		/// before this call.
		/// </remarks>
		bool RemoveAssociation(TKey distinguishingFactor, string handle);
	}

	// For some odd reason, having this next class causes our test project to fail to build with this error:
	// Error	42	Method 'StoreAssociation' in type 'Kiss.Web.Auth.OpenId.IAssociationStoreContract_Accessor`1' from assembly 'Kiss.Web.Auth_Accessor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' does not have an implementation.	Kiss.Web.Auth.Test
	/////// <summary>
	/////// Code Contract for the <see cref="IAssociationStore&lt;TKey&gt;"/> class.
	/////// </summary>
	/////// <typeparam name="TKey">The type of the key.</typeparam>
	////// [ContractClassFor(typeof(IAssociationStore<>))]
	////internal abstract class IAssociationStoreContract<TKey> : IAssociationStore<TKey> {
	////    #region IAssociationStore<TKey> Members

	////    /// <summary>
	////    /// Saves an <see cref="Association"/> for later recall.
	////    /// </summary>
	////    /// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for providers).</param>
	////    /// <param name="association">The association to store.</param>
	////    /// <remarks>
	////    /// TODO: what should implementations do on association handle conflict?
	////    /// </remarks>
	////    void IAssociationStore<TKey>.StoreAssociation(TKey distinguishingFactor, Association association) {
	////        // // Contract.Requires<ArgumentNullException>(distinguishingFactor != null);
	////        // // Contract.Requires<ArgumentNullException>(association != null);
	////        throw new NotImplementedException();
	////    }

	////    /// <summary>
	////    /// Gets the best association (the one with the longest remaining life) for a given key.
	////    /// </summary>
	////    /// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
	////    /// <param name="securityRequirements">The security requirements that the returned association must meet.</param>
	////    /// <returns>
	////    /// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
	////    /// </returns>
	////    /// <remarks>
	////    /// In the event that multiple associations exist for the given
	////    /// <paramref name="distinguishingFactor"/>, it is important for the
	////    /// implementation for this method to use the <paramref name="securityRequirements"/>
	////    /// to pick the best (highest grade or longest living as the host's policy may dictate)
	////    /// association that fits the security requirements.
	////    /// Associations that are returned that do not meet the security requirements will be
	////    /// ignored and a new association created.
	////    /// </remarks>
	////    Association IAssociationStore<TKey>.GetAssociation(TKey distinguishingFactor, SecuritySettings securityRequirements) {
	////        // // Contract.Requires<ArgumentNullException>(distinguishingFactor != null);
	////        // // Contract.Requires<ArgumentNullException>(securityRequirements != null);
	////        throw new NotImplementedException();
	////    }

	////    /// <summary>
	////    /// Gets the association for a given key and handle.
	////    /// </summary>
	////    /// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
	////    /// <param name="handle">The handle of the specific association that must be recalled.</param>
	////    /// <returns>
	////    /// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key and handle.
	////    /// </returns>
	////    Association IAssociationStore<TKey>.GetAssociation(TKey distinguishingFactor, string handle) {
	////        // // Contract.Requires<ArgumentNullException>(distinguishingFactor != null);
	////        // Contract.Requires(!String.IsNullOrEmpty(handle));
	////        throw new NotImplementedException();
	////    }

	////    /// <summary>
	////    /// Removes a specified handle that may exist in the store.
	////    /// </summary>
	////    /// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
	////    /// <param name="handle">The handle of the specific association that must be deleted.</param>
	////    /// <returns>
	////    /// True if the association existed in this store previous to this call.
	////    /// </returns>
	////    /// <remarks>
	////    /// No exception should be thrown if the association does not exist in the store
	////    /// before this call.
	////    /// </remarks>
	////    bool IAssociationStore<TKey>.RemoveAssociation(TKey distinguishingFactor, string handle) {
	////        // // Contract.Requires<ArgumentNullException>(distinguishingFactor != null);
	////        // Contract.Requires(!String.IsNullOrEmpty(handle));
	////        throw new NotImplementedException();
	////    }

	////    /// <summary>
	////    /// Clears all expired associations from the store.
	////    /// </summary>
	////    /// <remarks>
	////    /// If another algorithm is in place to periodically clear out expired associations,
	////    /// this method call may be ignored.
	////    /// This should be done frequently enough to avoid a memory leak, but sparingly enough
	////    /// to not be a performance drain.
	////    /// </remarks>
	////    void IAssociationStore<TKey>.ClearExpiredAssociations() {
	////        throw new NotImplementedException();
	////    }

	////    #endregion
	////}
}
