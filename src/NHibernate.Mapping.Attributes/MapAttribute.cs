// 
// NHibernate.Mapping.Attributes
// This product is under the terms of the GNU Lesser General Public License.
//
//
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 2.0.50727.1378
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------
//
//
// This source code was auto-generated by Refly, Version=2.21.1.0 (modified).
//
namespace NHibernate.Mapping.Attributes
{
	
	
	/// <summary> </summary>
	[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple=true)]
	[System.Serializable()]
	public class MapAttribute : BaseAttribute
	{
		
		private string _sort = null;
		
		private bool _genericspecified;
		
		private string _persister = null;
		
		private string _access = null;
		
		private string _where = null;
		
		private bool _inversespecified;
		
		private CascadeStyle _cascade = CascadeStyle.Unspecified;
		
		private string _schema = null;
		
		private bool _lazyspecified;
		
		private string _collectiontype = null;
		
		private bool _inverse = false;
		
		private bool _optimisticlock = false;
		
		private int _batchsize = -1;
		
		private bool _generic = false;
		
		private string _name = null;
		
		private string _orderby = null;
		
		private string _table = null;
		
		private bool _lazy = false;
		
		private bool _optimisticlockspecified;
		
		private OuterJoinStrategy _outerjoin = OuterJoinStrategy.Unspecified;
		
		private string _check = null;
		
		private CollectionFetchMode _fetch = CollectionFetchMode.Unspecified;
		
		/// <summary> Default constructor (position=0) </summary>
		public MapAttribute() : 
				base(0)
		{
		}
		
		/// <summary> Constructor taking the position of the attribute. </summary>
		public MapAttribute(int position) : 
				base(position)
		{
		}
		
		/// <summary> </summary>
		public virtual string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string Access
		{
			get
			{
				return this._access;
			}
			set
			{
				this._access = value;
			}
		}
		
		/// <summary> </summary>
		public virtual System.Type AccessType
		{
			get
			{
				return System.Type.GetType( this.Access );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Access = value.FullName.Substring(7);
				else
					this.Access = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
		
		/// <summary> </summary>
		public virtual string Table
		{
			get
			{
				return this._table;
			}
			set
			{
				this._table = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string Schema
		{
			get
			{
				return this._schema;
			}
			set
			{
				this._schema = value;
			}
		}
		
		/// <summary>Enable the lazy loading of this collection</summary>
		public virtual bool Lazy
		{
			get
			{
				return this._lazy;
			}
			set
			{
				this._lazy = value;
				_lazyspecified = true;
			}
		}
		
		/// <summary> Tells if Lazy has been specified. </summary>
		public virtual bool LazySpecified
		{
			get
			{
				return this._lazyspecified;
			}
		}
		
		/// <summary> </summary>
		public virtual OuterJoinStrategy OuterJoin
		{
			get
			{
				return this._outerjoin;
			}
			set
			{
				this._outerjoin = value;
			}
		}
		
		/// <summary> </summary>
		public virtual CollectionFetchMode Fetch
		{
			get
			{
				return this._fetch;
			}
			set
			{
				this._fetch = value;
			}
		}
		
		/// <summary> </summary>
		public virtual CascadeStyle Cascade
		{
			get
			{
				return this._cascade;
			}
			set
			{
				this._cascade = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string Where
		{
			get
			{
				return this._where;
			}
			set
			{
				this._where = value;
			}
		}
		
		/// <summary> </summary>
		public virtual bool Inverse
		{
			get
			{
				return this._inverse;
			}
			set
			{
				this._inverse = value;
				_inversespecified = true;
			}
		}
		
		/// <summary> Tells if Inverse has been specified. </summary>
		public virtual bool InverseSpecified
		{
			get
			{
				return this._inversespecified;
			}
		}
		
		/// <summary> </summary>
		public virtual string Persister
		{
			get
			{
				return this._persister;
			}
			set
			{
				this._persister = value;
			}
		}
		
		/// <summary> </summary>
		public virtual System.Type PersisterType
		{
			get
			{
				return System.Type.GetType( this.Persister );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Persister = value.FullName.Substring(7);
				else
					this.Persister = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
		
		/// <summary> </summary>
		public virtual int BatchSize
		{
			get
			{
				return this._batchsize;
			}
			set
			{
				this._batchsize = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string Check
		{
			get
			{
				return this._check;
			}
			set
			{
				this._check = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string CollectionType
		{
			get
			{
				return this._collectiontype;
			}
			set
			{
				this._collectiontype = value;
			}
		}
		
		/// <summary> </summary>
		public virtual System.Type CollectionTypeType
		{
			get
			{
				return System.Type.GetType( this.CollectionType );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.CollectionType = value.FullName.Substring(7);
				else
					this.CollectionType = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
		
		/// <summary> </summary>
		public virtual bool OptimisticLock
		{
			get
			{
				return this._optimisticlock;
			}
			set
			{
				this._optimisticlock = value;
				_optimisticlockspecified = true;
			}
		}
		
		/// <summary> Tells if OptimisticLock has been specified. </summary>
		public virtual bool OptimisticLockSpecified
		{
			get
			{
				return this._optimisticlockspecified;
			}
		}
		
		/// <summary>The concrete collection should use a generic version or an object-based version.</summary>
		public virtual bool Generic
		{
			get
			{
				return this._generic;
			}
			set
			{
				this._generic = value;
				_genericspecified = true;
			}
		}
		
		/// <summary> Tells if Generic has been specified. </summary>
		public virtual bool GenericSpecified
		{
			get
			{
				return this._genericspecified;
			}
		}
		
		/// <summary> </summary>
		public virtual string OrderBy
		{
			get
			{
				return this._orderby;
			}
			set
			{
				this._orderby = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string Sort
		{
			get
			{
				return this._sort;
			}
			set
			{
				this._sort = value;
			}
		}
		
		/// <summary> </summary>
		public virtual System.Type SortType
		{
			get
			{
				return System.Type.GetType( this.Sort );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Sort = value.FullName.Substring(7);
				else
					this.Sort = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
	}
}
