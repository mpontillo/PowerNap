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
	
	
	/// <summary>Property of an entity class or component, component-element, composite-id, etc. Class Properties (get_ and set_ methods) are mapped to table columns</summary>
	[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple=true)]
	[System.Serializable()]
	public class PropertyAttribute : BaseAttribute
	{
		
		private bool _unique = false;
		
		private bool _update = true;
		
		private string _name = null;
		
		private bool _notnullspecified;
		
		private bool _uniquespecified;
		
		private string _access = null;
		
		private int _length = -1;
		
		private string _index = null;
		
		private bool _insert = true;
		
		private bool _insertspecified;
		
		private string _type = null;
		
		private bool _notnull = false;
		
		private string _column = null;
		
		private string _formula = null;
		
		private bool _updatespecified;
		
		/// <summary> Default constructor (position=0) </summary>
		public PropertyAttribute() : 
				base(0)
		{
		}
		
		/// <summary> Constructor taking the position of the attribute. </summary>
		public PropertyAttribute(int position) : 
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
		public virtual string Type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
			}
		}
		
		/// <summary> </summary>
		public virtual System.Type TypeType
		{
			get
			{
				return System.Type.GetType( this.Type );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Type = value.FullName.Substring(7);
				else
					this.Type = value.FullName + ", " + value.Assembly.GetName().Name;
			}
		}
		
		/// <summary> </summary>
		public virtual string Column
		{
			get
			{
				return this._column;
			}
			set
			{
				this._column = value;
			}
		}
		
		/// <summary> </summary>
		public virtual int Length
		{
			get
			{
				return this._length;
			}
			set
			{
				this._length = value;
			}
		}
		
		/// <summary> </summary>
		public virtual bool NotNull
		{
			get
			{
				return this._notnull;
			}
			set
			{
				this._notnull = value;
				_notnullspecified = true;
			}
		}
		
		/// <summary> Tells if NotNull has been specified. </summary>
		public virtual bool NotNullSpecified
		{
			get
			{
				return this._notnullspecified;
			}
		}
		
		/// <summary> </summary>
		public virtual bool Unique
		{
			get
			{
				return this._unique;
			}
			set
			{
				this._unique = value;
				_uniquespecified = true;
			}
		}
		
		/// <summary> Tells if Unique has been specified. </summary>
		public virtual bool UniqueSpecified
		{
			get
			{
				return this._uniquespecified;
			}
		}
		
		/// <summary>only supported for properties of a class (not component)</summary>
		public virtual bool Update
		{
			get
			{
				return this._update;
			}
			set
			{
				this._update = value;
				_updatespecified = true;
			}
		}
		
		/// <summary> Tells if Update has been specified. </summary>
		public virtual bool UpdateSpecified
		{
			get
			{
				return this._updatespecified;
			}
		}
		
		/// <summary>only supported for properties of a class (not component)</summary>
		public virtual bool Insert
		{
			get
			{
				return this._insert;
			}
			set
			{
				this._insert = value;
				_insertspecified = true;
			}
		}
		
		/// <summary> Tells if Insert has been specified. </summary>
		public virtual bool InsertSpecified
		{
			get
			{
				return this._insertspecified;
			}
		}
		
		/// <summary>only supported for properties of a class (not component)</summary>
		public virtual string Formula
		{
			get
			{
				return this._formula;
			}
			set
			{
				this._formula = value;
			}
		}
		
		/// <summary> </summary>
		public virtual string Index
		{
			get
			{
				return this._index;
			}
			set
			{
				this._index = value;
			}
		}
	}
}
