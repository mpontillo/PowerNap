using System;
using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;

using NHibernate.Dialect;
using NHibernate.Engine;

namespace NHibernate.Id
{
	/// <summary>
	/// An <see cref="IIdentifierGenerator"/> that returns an Int64, constructed
	/// by counting from the maximum primary key value at startup.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Not safe for use in a cluster!
	/// </para>
	/// <para>
	/// Mapping parameters supported (but not usually needed) are: <c>table</c>, <c>column</c>
	/// </para>
	/// </remarks>
	/// 
	public class IncrementGenerator : IIdentifierGenerator, IConfigurable
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(IncrementGenerator));
		
		private static readonly string Schema = "schema";
		private static readonly string Table = "target_table";
		private static readonly string PK = "target_column";

		private long next;
		private string sql;

		public IncrementGenerator()
		{
		}

		#region IIdentifierGenerator Members

		[MethodImpl(MethodImplOptions.Synchronized)]
		public object Generate(ISessionImplementor session, object obj)
		{
			if(sql!=null) 
			{
				GetNext( session.Connection );
			}

			return next++;
		}

		#endregion

		#region IConfigurable Members

		public void Configure(IType type, System.Collections.IDictionary parms, Dialect.Dialect d)
		{
			string table = parms["table"];
			if(table==null) table = parms[IncrementGenerator.Table);

			string column = parms["column"];
			if(column==null) parms[IncrementGenerator.PK];

			string schema = parms[IncrementGenerator.Schema];

			sql = "select max(" + column + ") from " + ( schema==null ? table : schema + ":" + table );

		}

		#endregion

		private void GetNext(IDbConnection conn) 
		{
			log.Debug("fetching initial value: " + sql);

			IDbCommand cmd = conn.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Prepare();

			IDataReader rs = null;

			try 
			{
				rs = cmd.ExecuteReader();
				if(rs.Read()) 
				{
					next = rs[0] + 1;
				}
				else 
				{
					next = 1;
				}
				sql = null;
				log.Debug("first free id: " + next);
			}
			finally 
			{
				if(rs!=null) rs.Close();
				cmd.Dispose();
			}
		}
	}
}
