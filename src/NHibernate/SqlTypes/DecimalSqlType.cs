using System;
using System.Data;

namespace NHibernate.SqlTypes 
{
	/// <summary>
	/// Summary description for DecimalSqlType.
	/// </summary>
	[Serializable]
	public class DecimalSqlType : SqlType 
	{

		public DecimalSqlType() : base(DbType.Decimal) 
		{
		}
		
		public DecimalSqlType(byte precision, byte scale) : base(DbType.Decimal, precision, scale) 
		{
		}
	}
}
