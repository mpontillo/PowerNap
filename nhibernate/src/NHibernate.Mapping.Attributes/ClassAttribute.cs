// 
// NHibernate.Mapping.Attributes
// This product is under the terms of the GNU Lesser General Public License.
//
//
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
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
	
	
	/// <summary>Root of an entity class hierarchy. Entities have their own tables</summary>
	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface, AllowMultiple=false)]
	[System.Serializable()]
	public class ClassAttribute : BaseAttribute
	{
		
		private int _batchsize = -1;
		
		private string _persister = null;
		
		private OptimisticLockMode _optimisticlock = OptimisticLockMode.Unspecified;
		
		private bool _selectbeforeupdate = false;
		
		private string _name = null;
		
		private bool _selectbeforeupdatespecified;
		
		private string _table = null;
		
		private bool _dynamicinsert = false;
		
		private bool _dynamicinsertspecified;
		
		private bool _mutable = true;
		
		private bool _mutablespecified;
		
		private string _discriminatorvalue = null;
		
		private string _where = null;
		
		private string _schema = null;
		
		private bool _lazy = false;
		
		private string _discriminatorvalueenumformat = "g";
		
		private string _check = null;
		
		private bool _lazyspecified;
		
		private bool _dynamicupdatespecified;
		
		private bool _dynamicupdate = false;
		
		private string _proxy = null;
		
		private PolymorphismType _polymorphism = PolymorphismType.Unspecified;
		
		/// <summary> Default constructor (position=0) </summary>
		public ClassAttribute() : 
				base(0)
		{
		}
		
		/// <summary> Constructor taking the position of the attribute. </summary>
		public ClassAttribute(int position) : 
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
		public virtual System.Type NameType
		{
			get
			{
				return System.Type.GetType( this.Name );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Name = value.FullName.Substring(7);
				else
					this.Name = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
		
		/// <summary>default: no proxy interface</summary>
		public virtual string Proxy
		{
			get
			{
				return this._proxy;
			}
			set
			{
				this._proxy = value;
			}
		}
		
		/// <summary>default: no proxy interface</summary>
		public virtual System.Type ProxyType
		{
			get
			{
				return System.Type.GetType( this.Proxy );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Proxy = value.FullName.Substring(7);
				else
					this.Proxy = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
		
		/// <summary>Enable the lazy loading of this class in associations</summary>
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
		public virtual bool DynamicUpdate
		{
			get
			{
				return this._dynamicupdate;
			}
			set
			{
				this._dynamicupdate = value;
				_dynamicupdatespecified = true;
			}
		}
		
		/// <summary> Tells if DynamicUpdate has been specified. </summary>
		public virtual bool DynamicUpdateSpecified
		{
			get
			{
				return this._dynamicupdatespecified;
			}
		}
		
		/// <summary> </summary>
		public virtual bool DynamicInsert
		{
			get
			{
				return this._dynamicinsert;
			}
			set
			{
				this._dynamicinsert = value;
				_dynamicinsertspecified = true;
			}
		}
		
		/// <summary> Tells if DynamicInsert has been specified. </summary>
		public virtual bool DynamicInsertSpecified
		{
			get
			{
				return this._dynamicinsertspecified;
			}
		}
		
		/// <summary> </summary>
		public virtual bool SelectBeforeUpdate
		{
			get
			{
				return this._selectbeforeupdate;
			}
			set
			{
				this._selectbeforeupdate = value;
				_selectbeforeupdatespecified = true;
			}
		}
		
		/// <summary> Tells if SelectBeforeUpdate has been specified. </summary>
		public virtual bool SelectBeforeUpdateSpecified
		{
			get
			{
				return this._selectbeforeupdatespecified;
			}
		}
		
		/// <summary>default: unqualified classname</summary>
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
		
		/// <summary>default: no value</summary>
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
		
		/// <summary>default: unqualified class name | none</summary>
		public virtual string DiscriminatorValue
		{
			get
			{
				return this._discriminatorvalue;
			}
			set
			{
				this._discriminatorvalue = value;
			}
		}
		
		/// <summary>default: unqualified class name | none</summary>
		public virtual object DiscriminatorValueObject
		{
			get
			{
				return this.DiscriminatorValue;
			}
			set
			{
				if(value is System.Enum)
					this.DiscriminatorValue = System.Enum.Format(value.GetType(), value, this.DiscriminatorValueEnumFormat);
				else
					this.DiscriminatorValue = value==null ? "null" : value.ToString();
			}
		}
		
		/// <summary>'format' used by System.Enum.Format() in DiscriminatorValueObject</summary>
		public virtual string DiscriminatorValueEnumFormat
		{
			get
			{
				return this._discriminatorvalueenumformat;
			}
			set
			{
				this._discriminatorvalueenumformat = value;
			}
		}
		
		/// <summary> </summary>
		public virtual bool Mutable
		{
			get
			{
				return this._mutable;
			}
			set
			{
				this._mutable = value;
				_mutablespecified = true;
			}
		}
		
		/// <summary> Tells if Mutable has been specified. </summary>
		public virtual bool MutableSpecified
		{
			get
			{
				return this._mutablespecified;
			}
		}
		
		/// <summary> </summary>
		public virtual PolymorphismType Polymorphism
		{
			get
			{
				return this._polymorphism;
			}
			set
			{
				this._polymorphism = value;
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
		
		/// <summary>default: none</summary>
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
		public virtual OptimisticLockMode OptimisticLock
		{
			get
			{
				return this._optimisticlock;
			}
			set
			{
				this._optimisticlock = value;
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
	}
}
