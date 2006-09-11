using System;
using System.Data;
using System.Text;
using NHibernate.SqlCommand;
using NHibernate.Util;
using Environment = NHibernate.Cfg.Environment;
using NHibernate.SqlTypes;

namespace NHibernate.Driver
{
	/// <summary>
	/// Base class for the implementation of IDriver
	/// </summary>
	public abstract class DriverBase : IDriver
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DriverBase));

		public abstract IDbConnection CreateConnection();
		public abstract IDbCommand CreateCommand();

		/// <summary>
		/// Does this Driver require the use of a Named Prefix in the SQL statement.  
		/// </summary>
		/// <remarks>
		/// For example, SqlClient requires <c>select * from simple where simple_id = @simple_id</c>
		/// If this is false, like with the OleDb provider, then it is assumed that  
		/// the <c>?</c> can be a placeholder for the parameter in the SQL statement.
		/// </remarks>
		public abstract bool UseNamedPrefixInSql { get; }

		/// <summary>
		/// Does this Driver require the use of the Named Prefix when trying
		/// to reference the Parameter in the Command's Parameter collection.  
		/// </summary>
		/// <remarks>
		/// This is really only useful when the UseNamedPrefixInSql == true.  When this is true the
		/// code will look like:
		/// <code>IDbParameter param = cmd.Parameters["@paramName"]</code>
		/// if this is false the code will be 
		/// <code>IDbParameter param = cmd.Parameters["paramName"]</code>.
		/// </remarks>
		public abstract bool UseNamedPrefixInParameter { get; }

		/// <summary>
		/// The Named Prefix for parameters.  
		/// </summary>
		/// <remarks>
		/// Sql Server uses <c>"@"</c> and Oracle uses <c>":"</c>.
		/// </remarks>
		public abstract string NamedPrefix { get; }

		/// <summary>
		/// Change the parameterName into the correct format IDbCommand.CommandText
		/// for the ConnectionProvider
		/// </summary>
		/// <param name="parameterName">The unformatted name of the parameter</param>
		/// <returns>A parameter formatted for an IDbCommand.CommandText</returns>
		public string FormatNameForSql(string parameterName)
		{
			return UseNamedPrefixInSql ? (NamedPrefix + parameterName) : StringHelper.SqlParameter;
		}

		/// <summary>
		/// Changes the parameterName into the correct format for an IDbParameter
		/// for the Driver.
		/// </summary>
		/// <remarks>
		/// For SqlServerConnectionProvider it will change <c>id</c> to <c>@id</c>
		/// </remarks>
		/// <param name="parameterName">The unformatted name of the parameter</param>
		/// <returns>A parameter formatted for an IDbParameter.</returns>
		public string FormatNameForParameter(string parameterName)
		{
			return UseNamedPrefixInParameter ? (NamedPrefix + parameterName) : parameterName;
		}

		public virtual bool SupportsMultipleOpenReaders
		{
			get { return true; }
		}

		public virtual bool SupportsPreparingCommands
		{
			get { return true; }
		}

		public virtual IDbCommand GenerateCommand(CommandType type, SqlString sqlString)
		{
			IDbCommand cmd = CreateCommand();
			cmd.CommandType = type;

			SetCommandTimeout(cmd, Environment.Properties[Environment.CommandTimeout]);
			SetCommandText(cmd, sqlString);
			SetCommandParameters(cmd, sqlString.ParameterTypes);

			return cmd;
		}
		
		private static void SetCommandTimeout(IDbCommand cmd, object envTimeout)
		{
			if (envTimeout != null)
			{
				int timeout = Convert.ToInt32(envTimeout);
				if (timeout >= 0)
				{
					if (log.IsDebugEnabled)
					{
						log.Debug(string.Format("setting ADO Command timeout to '{0}' seconds", timeout));
					}
					try
					{
						cmd.CommandTimeout = timeout;
					}
					catch (Exception e)
					{
						if (log.IsWarnEnabled)
						{
							log.Warn(e.ToString());
						}
					}
				}
				else
				{
					log.Error("Invalid timeout of '" + envTimeout + "' specified, ignoring");
				}
			}
		}
		
		private static string ToParameterName(int index)
		{
			return "p" + index;
		}
		
		private void SetCommandText(IDbCommand cmd, SqlString sqlString)
		{
			int paramIndex = 0;
			StringBuilder builder = new StringBuilder(sqlString.Count * 15);
			foreach (object part in sqlString.SqlParts)
			{
				Parameter parameter = part as Parameter;

				if (parameter != null)
				{
					string paramName = ToParameterName(paramIndex);
					builder.Append(FormatNameForSql(paramName));
					paramIndex++;
				}
				else
				{
					builder.Append((string) part);
				}
			}

			cmd.CommandText = builder.ToString();
		}
		
		private void SetCommandParameters(IDbCommand cmd, SqlType[] sqlTypes)
		{
			for (int i = 0; i < sqlTypes.Length; i++)
			{
				string paramName = ToParameterName(i);
				IDbDataParameter dbParam = GenerateParameter(cmd, paramName, sqlTypes[i]);
				cmd.Parameters.Add(dbParam);
			}
		}

		protected virtual void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType)
		{
			if (sqlType == null)
			{
				throw new QueryException(String.Format("No type assigned to parameter '{0}'", name));
			}

			dbParam.ParameterName = FormatNameForParameter(name);
			dbParam.DbType = sqlType.DbType;
		}

		/// <summary>
		/// Generates an IDbDataParameter for the IDbCommand.  It does not add the IDbDataParameter to the IDbCommand's
		/// Parameter collection.
		/// </summary>
		/// <param name="command">The IDbCommand to use to create the IDbDataParameter.</param>
		/// <param name="name">The name to set for IDbDataParameter.Name</param>
		/// <param name="sqlType">The SqlType to set for IDbDataParameter.</param>
		/// <returns>An IDbDataParameter ready to be added to an IDbCommand.</returns>
		protected IDbDataParameter GenerateParameter(IDbCommand command, string name, SqlType sqlType)
		{
			IDbDataParameter dbParam = command.CreateParameter();
			InitializeParameter(dbParam, name, sqlType);

			return dbParam;
		}

		/// <summary>
		/// Prepare the <paramref name="command" />. Will only be called if <see cref="SupportsPreparingCommands" />
		/// is <c>true</c>.
		/// </summary>
		/// <remarks>
		/// Drivers that require Size or Precision/Scale to be set before the IDbCommand is prepared should 
		/// override this method and use the info contained in <paramref name="parameterTypes" /> to set those
		/// values.  By default those values are not set, only the DbType and ParameterName are set.
		/// </remarks>
		public virtual void PrepareCommand(IDbCommand command, SqlType[] parameterTypes)
		{
			command.Prepare();
		}
	}
}