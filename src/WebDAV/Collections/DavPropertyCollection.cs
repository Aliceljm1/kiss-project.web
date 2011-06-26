using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Kiss.Web.WebDAV.Classes;

namespace Kiss.Web.WebDAV.Collections
{
	/// <summary>
	/// WebDav Property Collection.
	/// </summary>
	[Serializable]
	public class DavPropertyCollection : Collection<DavProperty>, ICloneable
	{
		/// <summary>
		/// WebDav Property Collection.
		/// </summary>
		public DavPropertyCollection() { }

		/// <summary>
		/// Retrieve a property by name
		/// </summary>
		public DavProperty this[string propertyName]
		{
			get
			{
				return base.Items
							.Where(e => e.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
							.FirstOrDefault();
			}
		}

		/// <summary>
		/// Removes a property from the collection by name
		/// </summary>
		/// <param name="propertyName"></param>
		public void Remove(string propertyName)
		{
			DavProperty _property = this[propertyName];
			if (_property != null)
				base.Remove(_property);
		}

		/// <summary>
		/// Retrieve all the available collection property names
		/// </summary>
		public List<string> GetPropertyNames()
		{
			return base.Items
						.Select(e => e.Name)
						.ToList();
		}

		/// <summary>
		/// Copy an existing property collection
		/// </summary>
		/// <param name="propertyCollection"></param>
		public void Copy(DavPropertyCollection propertyCollection)
		{
			if (propertyCollection == null)
				throw new ArgumentNullException("propertyCollection", InternalFunctions.GetResourceString("ArgumentNullException", "PropertyCollection"));

			base.Clear();
			foreach (DavProperty _davProperty in propertyCollection)
			{
				if (_davProperty.Name != null)
					this.Add(_davProperty.Clone());
			}
		}

		#region ICloneable Members
		// Explicit interface method impl
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// DavPropertyAttribute Clone
		/// </summary>
		/// <remarks>Deep copy</remarks>
		/// <returns></returns>
		public DavPropertyCollection Clone()
		{
			// Start with a flat, memberwise copy
			DavPropertyCollection _davPropertyCollection = new DavPropertyCollection();

			// Then deep-copy everything that needs the 
			foreach (DavProperty _davProperty in this)
				_davPropertyCollection.Add(_davProperty.Clone());

			return _davPropertyCollection;
		}
		#endregion
	}
}


