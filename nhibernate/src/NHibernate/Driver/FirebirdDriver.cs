using System;
using System.Data;
using System.Reflection;

namespace NHibernate.Driver
{
	/// <summary>
	/// Summary description for FirebirdDriver.
	/// </summary>
	public class FirebirdDriver : DriverBase
	{
		private System.Type connectionType;
		private System.Type commandType;

		public FirebirdDriver()
		{
			connectionType = System.Type.GetType("FirebirdSql.Data.Firebird.FbConnection, FirebirdSql.Data.Firebird");
			commandType = System.Type.GetType("FirebirdSql.Data.Firebird.FbCommand, FirebirdSql.Data.Firebird");
		}

		public override System.Type CommandType
		{
			get	{ return commandType; }
		}

		public override System.Type ConnectionType
		{
			get	{ return connectionType; }
		}

		public override bool UseNamedPrefixInSql 
		{
			get {return true;}
		}

		public override bool UseNamedPrefixInParameter 
		{
			get {return true;}
		}

		public override string NamedPrefix 	
		{
			get {return "@";}
		}
	}
}
