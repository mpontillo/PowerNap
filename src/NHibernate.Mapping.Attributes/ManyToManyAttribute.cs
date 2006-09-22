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
	
	
	/// <summary> </summary>
	[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple=true)]
	[System.Serializable()]
	public class ManyToManyAttribute : BaseAttribute
	{
		
		private OuterJoinStrategy _outerjoin = OuterJoinStrategy.Unspecified;
		
		private string _foreignkey = null;
		
		private string _column = null;
		
		private string _class = null;
		
		private NotFoundMode _notfound = NotFoundMode.Unspecified;
		
		private FetchMode _fetch = FetchMode.Unspecified;
		
		/// <summary> Default constructor (position=0) </summary>
		public ManyToManyAttribute() : 
				base(0)
		{
		}
		
		/// <summary> Constructor taking the position of the attribute. </summary>
		public ManyToManyAttribute(int position) : 
				base(position)
		{
		}
		
		/// <summary> </summary>
		public virtual string Class
		{
			get
			{
				return this._class;
			}
			set
			{
				this._class = value;
			}
		}
		
		/// <summary> </summary>
		public virtual System.Type ClassType
		{
			get
			{
				return System.Type.GetType( this.Class );
			}
			set
			{
				if(value.Assembly == typeof(int).Assembly)
					this.Class = value.FullName.Substring(7);
				else
					this.Class = value.FullName + ", " + value.Assembly.GetName().Name;
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
		public virtual string ForeignKey
		{
			get
			{
				return this._foreignkey;
			}
			set
			{
				this._foreignkey = value;
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
		public virtual FetchMode Fetch
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
		public virtual NotFoundMode NotFound
		{
			get
			{
				return this._notfound;
			}
			set
			{
				this._notfound = value;
			}
		}
	}
}
