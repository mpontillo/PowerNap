// 
// NHibernate.Mapping.Attributes
// This product is under the terms of the GNU Lesser General Public License.
//
//
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
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
	
	
	/// <summary>Subclass declarations are nested beneath the root class declaration to achieve polymorphic persistence</summary>
	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple=true)]
	[System.Serializable()]
	public class SubclassAttribute : BaseAttribute
	{
		
		private bool _lazy = false;
		
		private bool _dynamicinsert = false;
		
		private string _proxy = null;
		
		private string _name = null;
		
		private bool _lazyspecified;
		
		private bool _selectbeforeupdatespecified;
		
		private bool _selectbeforeupdate = false;
		
		private bool _dynamicupdate = false;
		
		private bool _dynamicinsertspecified;
		
		private string _discriminatorvalue = null;
		
		private string _extends = null;
		
		private bool _dynamicupdatespecified;
		
		private string _discriminatorvalueenumformat = "g";
		
		/// <summary> Default constructor (position=0) </summary>
		public SubclassAttribute() : 
				base(0)
		{
		}
		
		/// <summary> Constructor taking the position of the attribute. </summary>
		public SubclassAttribute(int position) : 
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
		
		/// <summary>Name of the root class. Required if the Subclass is declared outside the declaration of its root class</summary>
		public virtual string Extends
		{
			get
			{
				return this._extends;
			}
			set
			{
				this._extends = value;
			}
		}
		
		/// <summary>Name of the root class. Required if the Subclass is declared outside the declaration of its root class</summary>
		public virtual System.Type ExtendsType
		{
			get
			{
				return System.Type.GetType( this.Extends );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Extends = value.FullName.Substring(7);
				else
					this.Extends = value.FullName + ", " + value.Assembly.GetName().Name;
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
					this.DiscriminatorValue = value.ToString();
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
	}
}
