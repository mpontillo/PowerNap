using System;
using System.Collections.Generic;
using System.Data;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace NHibernate.Driver
{
	public class BasicResultSetsCommand: IResultSetsCommand
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof(BasicResultSetsCommand));

		private readonly ISessionImplementor session;
		private readonly Dialect.Dialect dialect;
		private readonly IBatcher batcher;
		private int resultSetsCount = 0;
		private readonly List<SqlType> types = new List<SqlType>();
		private SqlString sqlString = new SqlString();

		public BasicResultSetsCommand(ISessionImplementor session)
		{
			this.session = session;
			dialect = session.Factory.Dialect;
			batcher = session.Batcher;
		}

		public virtual void Append(SqlCommandInfo commandInfo)
		{
			resultSetsCount++;
			sqlString = sqlString.Append(commandInfo.Text).Append(";").Append(Environment.NewLine);
			types.AddRange(commandInfo.ParameterTypes);
		}

		public int ParametersCount
		{
			get { return types.Count; }
		}

		public bool HasQueries
		{
			get { return resultSetsCount > 0; }
		}

		public SqlString Sql
		{
			get { return sqlString; }
		}

		public virtual IDataReader GetReader(QueryParameters[] queryParameters, int? commandTimeout)
		{
			SqlType[] sqlTypes = types.ToArray();
			var command= batcher.PrepareQueryCommand(CommandType.Text, sqlString, sqlTypes);
			if(commandTimeout.HasValue)
			{
				command.CommandTimeout = commandTimeout.Value;				
			}
			log.Info(command.CommandText);

			BindParameters(command, queryParameters);
			return new BatcherDataReaderWrapper(batcher, command);
		}

		protected virtual void BindParameters(IDbCommand command, QueryParameters[] queryParameters)
		{
			int colIndex = 0;

			for (int queryIndex = 0; queryIndex < resultSetsCount; queryIndex++)
			{
				int limitParameterSpan = BindLimitParametersFirstIfNeccesary(command, queryParameters[queryIndex], colIndex);
				colIndex = BindQueryParameters(command, queryParameters[queryIndex], colIndex + limitParameterSpan);
				colIndex += BindLimitParametersLastIfNeccesary(command, queryParameters[queryIndex], colIndex);
			}
		}

		protected virtual int BindLimitParametersLastIfNeccesary(IDbCommand command, QueryParameters parameter, int colIndex)
		{
			RowSelection selection = parameter.RowSelection;
			if (Loader.Loader.UseLimit(selection, dialect) && !dialect.BindLimitParametersFirst)
			{
				return Loader.Loader.BindLimitParameters(command, colIndex, selection, session);
			}
			return 0;
		}

		protected virtual int BindQueryParameters(IDbCommand command, QueryParameters parameter, int colIndex)
		{
			colIndex += parameter.BindParameters(command, colIndex, session);
			return colIndex;
		}

		protected virtual int BindLimitParametersFirstIfNeccesary(IDbCommand command, QueryParameters parameter, int colIndex)
		{
			int limitParameterSpan = 0;
			RowSelection selection = parameter.RowSelection;
			if (Loader.Loader.UseLimit(selection, dialect) && dialect.BindLimitParametersFirst)
			{
				limitParameterSpan += Loader.Loader.BindLimitParameters(command, colIndex, selection, session);
			}
			return limitParameterSpan;
		}
	}

	/// <summary>
	/// Datareader wrapper with the same life cycle of its command (through the batcher)
	/// </summary>
	public class BatcherDataReaderWrapper: IDataReader
	{
		private readonly IBatcher batcher;
		private readonly IDbCommand command;
		private readonly IDataReader reader;

		public BatcherDataReaderWrapper(IBatcher batcher, IDbCommand command)
		{
			if (batcher == null)
			{
				throw new ArgumentNullException("batcher");
			}
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}
			this.batcher = batcher;
			this.command = command;
			reader = batcher.ExecuteReader(command);
		}

		public void Dispose()
		{
			batcher.CloseCommand(command, reader);
		}

		#region IDataRecord Members

		public string GetName(int i)
		{
			return reader.GetName(i);
		}

		public string GetDataTypeName(int i)
		{
			return reader.GetDataTypeName(i);
		}

		public System.Type GetFieldType(int i)
		{
			return reader.GetFieldType(i);
		}

		public object GetValue(int i)
		{
			return reader.GetDecimal(i);
		}

		public int GetValues(object[] values)
		{
			return reader.GetValues(values);
		}

		public int GetOrdinal(string name)
		{
			return reader.GetOrdinal(name);
		}

		public bool GetBoolean(int i)
		{
			return reader.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return reader.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return reader.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public Guid GetGuid(int i)
		{
			return reader.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			return reader.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			return reader.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return reader.GetInt64(i);
		}

		public float GetFloat(int i)
		{
			return reader.GetFloat(i);
		}

		public double GetDouble(int i)
		{
			return reader.GetDouble(i);
		}

		public string GetString(int i)
		{
			return reader.GetString(i);
		}

		public decimal GetDecimal(int i)
		{
			return reader.GetDecimal(i);
		}

		public DateTime GetDateTime(int i)
		{
			return reader.GetDateTime(i);
		}

		public IDataReader GetData(int i)
		{
			return reader.GetData(i);
		}

		public bool IsDBNull(int i)
		{
			return reader.IsDBNull(i);
		}

		public int FieldCount
		{
			get { return reader.FieldCount; }
		}

		public object this[int i]
		{
			get { return reader[i]; }
		}

		public object this[string name]
		{
			get { return reader[name]; }
		}

		#endregion

		public override bool Equals(object obj)
		{
			return reader.Equals(obj);
		}

		public override int GetHashCode()
		{
			return reader.GetHashCode();
		}

		public void Close()
		{
			batcher.CloseCommand(command, reader);
		}

		public DataTable GetSchemaTable()
		{
			return reader.GetSchemaTable();
		}

		public bool NextResult()
		{
			return reader.NextResult();
		}

		public bool Read()
		{
			return reader.Read();
		}

		public int Depth
		{
			get { return reader.Depth; }
		}

		public bool IsClosed
		{
			get { return reader.IsClosed; }
		}

		public int RecordsAffected
		{
			get { return reader.RecordsAffected; }
		}
	}
}