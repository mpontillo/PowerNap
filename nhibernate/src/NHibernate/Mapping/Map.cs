using NHibernate.Type;

namespace NHibernate.Mapping
{
	/// <summary>
	/// A map has a primary key consisting of the key columns 
	/// + index columns.
	/// </summary>
	public class Map : IndexedCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class.
		/// </summary>
		/// <param name="owner">The <see cref="PersistentClass"/> that contains this map mapping.</param>
		public Map(PersistentClass owner)
			: base(owner)
		{
		}

		/// <summary>
		/// Gets the appropriate <see cref="PersistentCollectionType"/> that is 
		/// specialized for this list mapping.
		/// </summary>
		public override PersistentCollectionType CollectionType
		{
			get
			{
#if NET_2_0
				if (this.IsGeneric)
				{
					//TODO: this is causing problems because it is being called during BindProperty for the class containing
					// the collection but the Index property is not populated until the 2nd pass in BindMapSecondPass.  Need 
					// to look at why the index is part of the 2nd pass and can't be done during the initial pass.
					return TypeFactory.GenericMap(Role, this.Index.Type.ReturnedClass, this.Element.Type.ReturnedClass);
					// TODO: deal with sorting
				}
				else
				{
					return IsSorted ? 
							TypeFactory.SortedMap( Role, Comparer ) : 
							TypeFactory.Map(Role);
				}
#else

				return IsSorted ?
					TypeFactory.SortedMap( Role, Comparer ) :
					TypeFactory.Map( Role );
#endif
			}
		}

		/// <summary></summary>
		public override void CreateAllKeys( )
		{
			base.CreateAllKeys();
			if ( !IsInverse )
			{
				Index.CreateForeignKey();
			}
		}
	}
}