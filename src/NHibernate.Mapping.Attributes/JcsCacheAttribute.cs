// 
// NHibernate.Mapping.Attributes
// This product is under the terms of the GNU Lesser General Public License.
//
//
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 2.0.50727.42
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
	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface | System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple=true)]
	[System.Serializable()]
	public class JcsCacheAttribute : BaseAttribute
	{
		
		private JcsCacheUsage _usage = JcsCacheUsage.Unspecified;
		
		private string _region = null;
		
		/// <summary> Default constructor (position=0) </summary>
		public JcsCacheAttribute() : 
				base(0)
		{
		}
		
		/// <summary> Constructor taking the position of the attribute. </summary>
		public JcsCacheAttribute(int position) : 
				base(position)
		{
		}
		
		/// <summary> </summary>
		public virtual string Region
		{
			get
			{
				return this._region;
			}
			set
			{
				this._region = value;
			}
		}
		
		/// <summary> </summary>
		public virtual JcsCacheUsage Usage
		{
			get
			{
				return this._usage;
			}
			set
			{
				this._usage = value;
			}
		}
	}
}
