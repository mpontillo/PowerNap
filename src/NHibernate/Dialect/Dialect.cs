using System;
using System.Collections;
using System.Data;
using System.Text;

using NHibernate.Engine;
using NHibernate.Hql;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Dialect 
{
	/// <summary>
	/// Represents a dialect of SQL implemented by a particular RDBMS. Sublcasses
	/// implement Hibernate compatibility with differen systems
	/// </summary>
	/// <remarks>
	/// Subclasses should provide a public default constructor that <c>Register()</c>
	/// a set of type mappings and default Hibernate properties.
	/// </remarks>
	public abstract class Dialect 
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Dialect));

		private TypeNames typeNames = new TypeNames("$1");
		private IDictionary properties = new Hashtable();
		
		private static readonly IDictionary aggregateFunctions = new Hashtable();

		protected const string DefaultBatchSize = "15";
		protected const string NoBatch = "0";

		static Dialect() 
		{
			aggregateFunctions["count"] = new Dialect.CountQueryFunctionInfo();
			aggregateFunctions["avg"] = new Dialect.AvgQueryFunctionInfo();
			aggregateFunctions["max"] = new QueryFunctionStandard();
			aggregateFunctions["min"] = new QueryFunctionStandard();
			aggregateFunctions["sum"] = new QueryFunctionStandard();
			
		}

		/// <summary>
		/// The base constructor for Dialect.
		/// </summary>
		/// <remarks>
		/// Every subclass should override this and call Register() with every <see cref="DbType"/> except
		/// <see cref="DbType.Object"/>, <see cref="DbType.SByte"/>, <see cref="DbType.UInt16"/>, <see cref="DbType.UInt32"/>, 
		/// <see cref="DbType.UInt64"/>, <see cref="DbType.VarNumeric"/>.
		/// 
		/// <para>
		/// The Default properties for this Dialect should also be set - such as whether or not to use outer-joins
		/// and what the batch size should be.
		/// </para>
		/// </remarks>
		protected Dialect() 
		{
			log.Info( "Using dialect: " + this );
		}

		
		/// <summary>
		/// Characters used for quoting sql identifiers
		/// </summary>
		public const string PossibleQuoteChars = "`'\"[";
		public const string PossibleClosedQuoteChars = "`'\"]";

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// <see cref="NHibernate.SqlTypes.SqlType"/>,
		/// </summary>
		/// <param name="code">The SqlType</param>
		/// <returns>The database type name used by ddl.</returns>
		public virtual string GetTypeName(SqlType sqlType) 
		{
			string result = typeNames.Get(sqlType.DbType);
			if(result==null) 
			{
				throw new HibernateException( "No default type mapping for SqlType " + sqlType.ToString() );
			}

			return result;
		}

		/// <summary>
		/// Get the name of the database type associated with the given
		/// <see cref="SqlType"/>.
		/// </summary>
		/// <param name="sqlType">The SqlType </param>
		/// <param name="length">The length of the SqlType</param>
		/// <returns>The database type name used by ddl.</returns>
		public virtual string GetTypeName(SqlType sqlType, int length) 
		{
			string result = typeNames.Get(sqlType.DbType, length);
			if(result==null) 
			{
				throw new HibernateException( "No type mapping for SqlType " + sqlType.ToString() + " of length " + length );
			}
			return result;
		}

		/// <summary>
		/// Subclasses register a typename for the given type code and maximum
		/// column length. <c>$1</c> in the type name will be replaced by the column
		/// length (if appropriate)
		/// </summary>
		/// <param name="code">The typecode</param>
		/// <param name="capacity">Maximum length of database type</param>
		/// <param name="name">The database type name</param>
		protected void Register(DbType code, int capacity, string name) 
		{
			typeNames.Put(code, capacity, name);
		}

		/// <summary>
		/// Suclasses register a typename for the given type code. <c>$1</c> in the 
		/// typename will be replaced by the column length (if appropriate).
		/// </summary>
		/// <param name="code">The typecode</param>
		/// <param name="name">The database type name</param>
		protected void Register(DbType code, string name) 
		{
			typeNames.Put(code, name);
		}



		/// <summary>
		/// Does this dialect support the <c>ALTER TABLE</c> syntax?
		/// </summary>
		public virtual bool HasAlterTable 
		{
			get { return true; }
		}

		/// <summary>
		/// Do we need to drop constraints before dropping tables in the dialect?
		/// </summary>
		public virtual bool DropConstraints 
		{
			get { return true; }
		}

		/// <summary>
		/// Do we need to qualify index names with the schema name?
		/// </summary>
		public virtual bool QualifyIndexName 
		{
			get { return true; }
		}

		/// <summary>
		/// Does this dialect support the <c>FOR UDPATE</c> syntax?
		/// </summary>
		public virtual bool SupportsForUpdate 
		{
			get { return true; }
		}

		/// <summary>
		/// Does this dialect support the <c>FOR UDPATE OF</c> syntax?
		/// </summary>
		public virtual bool SupportsForUpdateOf
		{
			get { return false; }
		}

		/// <summary>
		/// Does this dialect support the Oracle-style <c>FOR UPDATE NOWAIT</c> syntax?
		/// </summary>
		public virtual bool SupportsForUpdateNoWait 
		{
			get { return false; }
		}

		/// <summary>
		/// Does this dialect support the <c>UNIQUE</c> column syntax?
		/// </summary>
		public virtual bool SupportsUnique 
		{
			get { return true; }
		}

		/// <summary>
		/// The syntax used to add a column to a table. Note this is deprecated
		/// </summary>
		public virtual string AddColumnString 
		{ 
			get { throw new NotSupportedException("No add column syntax supported by Dialect"); }
		}

		public virtual string GetAddForeignKeyConstraintString(string constraintName, string[] foreignKey, string referencedTable, string[] primaryKey) 
		{
			return new StringBuilder(30)
				.Append(" add constraint ")
				.Append(constraintName)
				.Append(" foreign key (")
				.Append( string.Join(StringHelper.CommaSpace, foreignKey) )
				.Append(") references ")
				.Append(referencedTable)
				.ToString();
		}

		/// <summary>
		/// The syntax used to add a primary key constraint to a table
		/// </summary>
		/// <param name="constraintName"></param>
		public virtual string GetAddPrimaryKeyConstraintString(string constraintName) 
		{
			return " add constraint " + constraintName + " primary key ";
		}

		/// <summary>
		/// The keyword used to specify a nullable column
		/// </summary>
		public virtual string NullColumnString 
		{
			get { return String.Empty; }
		}

		/// <summary>
		/// Does this Dialect allow adding a Sql String at the end of the 
		/// INSERT statement to retrieve the new Identity value.
		/// </summary>
		/// <value>defaults to false</value>
		/// <remarks>
		/// <para>
		/// If the Dialect supports this then only one Command will need to be executed
		/// against the Database to do the Insert and get the Id.
		/// </para>
		/// <para>
		/// If this is overridden and returns <c>true</c> then the Dialect
		/// is expected to override the method <see cref="AddIdentitySelectToInsert(SqlString)"/>
		/// </para>
		/// </remarks>
		public virtual bool SupportsIdentitySelectInInsert 
		{
			get { return false; }
		}


		/// <summary>
		/// Does this dialect support identity column key generation?
		/// </summary>
		public virtual bool SupportsIdentityColumns 
		{
			get { return false; }
		}

		/// <summary>
		/// Does this dialect support sequences?
		/// </summary>
		public virtual bool SupportsSequences 
		{
			get { return false; }
		}

		public virtual SqlString AddIdentitySelectToInsert(SqlString insertSql) 
		{
			throw new NotSupportedException("This Dialect does not implement AddIdentitySelectToInsert");
		}

		/// <summary>
		/// The syntax that returns the identity value of the last insert, if native
		/// key generation is supported
		/// </summary>
		public virtual string IdentitySelectString 
		{
			get { throw new MappingException("Dialect does not support native key generation"); }
		}

		/// <summary>
		/// The keyword used to specify an identity column, if native key generation is supported
		/// </summary>
		public virtual string IdentityColumnString 
		{
			get { throw new MappingException("Dialect does not support native key generation"); }
		}

		/// <summary>
		/// The keyword used to insert a generated value into an identity column (or null)
		/// </summary>
		public virtual string IdentityInsertString 
		{
			get { return null; }
		}

		/// <summary>
		/// The keyword used to insert a row without specifying any column values
		/// </summary>
		public virtual string NoColumnsInsertString 
		{
			get { return "values ( )"; }
		}

		/// <summary>
		/// The syntax that fetches the next value of a sequence, if sequences are supported.
		/// </summary>
		/// <param name="sequenceName">The name of the sequence</param>
		/// <returns></returns>
		public virtual string GetSequenceNextValString(string sequenceName) 
		{
			throw new MappingException("Dialect does not support sequences");
		}

		/// <summary>
		/// The syntax used to create a sequence, if sequences are supported
		/// </summary>
		/// <param name="sequenceName"></param>
		/// <returns></returns>
		public virtual string GetCreateSequenceString(string sequenceName) 
		{
			throw new MappingException("Dialect does not support sequences");
		}

		/// <summary>
		/// The syntax used to drop a sequence, if sequences are supported
		/// </summary>
		/// <param name="sequenceName"></param>
		/// <returns></returns>
		public virtual string GetDropSequenceString(string sequenceName) 
		{
			throw new MappingException("Dialect does not support sequences");
		}

		public virtual string QuerySequenceString 
		{
			get { return null; }
		}

		public static Dialect GetDialect() 
		{
			string dialectName = Cfg.Environment.Properties[Cfg.Environment.Dialect] as string;
			if (dialectName==null) throw new HibernateException("The dialect was not set. Set the property hibernate.dialect.");
			try 
			{
				return (Dialect) Activator.CreateInstance(ReflectHelper.ClassForName(dialectName));
			} 
			catch (Exception e) 
			{
				throw new HibernateException("Could not instanciate dialect class", e);
			}
		}

		public static Dialect GetDialect(IDictionary props) 
		{
			if (props==null) return GetDialect();
			string dialectName = (string) props[Cfg.Environment.Dialect];
			if (dialectName==null) return GetDialect();
			try 
			{
				return (Dialect) Activator.CreateInstance(ReflectHelper.ClassForName(dialectName));
			} 
			catch (Exception e) 
			{
				throw new HibernateException("could not instantiate dialect class", e);
			}
		}

		/// <summary>
		/// Retrieve a set of default Hibernate properties for this database.
		/// </summary>
		public IDictionary DefaultProperties 
		{
			get { return properties; }
		}

		/// <summary>
		/// Completely optional cascading drop clause
		/// </summary>
		public virtual string CascadeConstraintsString 
		{
			get { return String.Empty; }
		}

		/// <summary>
		/// Create an <c>JoinFragment</c> for this dialect
		/// </summary>
		/// <returns></returns>
		public virtual JoinFragment CreateOuterJoinFragment() 
		{
			return new ANSIJoinFragment();
		}

		/// <summary>
		/// Create an <c>CaseFragment</c> for this dialect
		/// </summary>
		/// <returns></returns>
		public virtual CaseFragment CreateCaseFragment() 
		{
			return new ANSICaseFragment(this);
		}

		/// <summary>
		/// The name of the SQL function that transforms a string to lowercase
		/// </summary>
		public virtual string LowercaseFunction 
		{
			get {return "lower";}
		}

		/// <summary>
		/// Does this Diealect have some kind of <c>LIMIT</c> syntax?
		/// </summary>
		/// <value>False, unless overridden.</value>
		public virtual bool SupportsLimit 
		{
			get { return false;}
		}

		/// <summary>
		/// Add a <c>LIMIT</c> clause to the given SQL <c>SELECT</c> 
		/// </summary>
		/// <param name="querySelect"></param>
		/// <returns>The modified SQL</returns>
		[Obsolete("Use the GetLimitString(SqlString) overload instead.")]
		public virtual string GetLimitString(String querySelect) 
		{
			//TODO: H2.0.3 - this might need to the a SqlString or an IDbCommand depending
			// on when it comes in.  Need to look at Loader to know for sure...
			throw new NotSupportedException("Paged Queries not supported");
		}

		/// <summary>
		/// Add a <c>LIMIT</c> clause to the given SQL <c>SELECT</c>
		/// </summary>
		/// <param name="querySqlString">A Query in the form of a SqlString.</param>
		/// <returns>A new SqlString that contains the <c>LIMIT</c> clause.</returns>
		public virtual SqlString GetLimitString(SqlString querySqlString) 
		{
			throw new NotSupportedException("Paged Queries not supported");
		}


		/// <summary>
		/// Does the <c>LIMIT</c> clause specify arguments in the "reverse" order
		/// limit, offset instead of offset, limit?
		/// </summary>
		/// <value>False, unless overridden.</value>
		/// <remarks>Inheritors should return true if the correct order is limit, offset</remarks>
		public virtual bool BindLimitParametersInReverseOrder 
		{
			get { return false;}
		}

		/// <summary>
		/// Does the <c>LIMIT</c> clause come at the start of the 
		/// <c>SELECT</c> statement rather than at the end?
		/// </summary>
		/// <value>false, unless overridden</value>
		public virtual bool BindLimitParametersFirst 
		{
			get { return false;}
		}
		
		/// <summary>
		/// Does the <c>LIMIT</c> clause take a "maximum" row number
		/// instead of a total number of returned rows?
		/// </summary>
		/// <returns>false, unless overridden</returns>
		public virtual bool UseMaxForLimit
		{
			get { return false;}
		}

		/// <summary>
		/// Should we use a <c>LIMIT</c> clause when there is no first result
		/// specified?
		/// </summary>
		public virtual bool PreferLimit
		{
			get { return false; }
		}

		/// <summary>
		/// The opening quote for a quoted identifier.
		/// </summary>
		protected virtual char OpenQuote 
		{
			get { return '"'; }
		}

		/// <summary>
		/// The closing quote for a quoted identifier.
		/// </summary>
		protected virtual char CloseQuote 
		{
			get { return '"'; }
		}

		/// <summary>
		/// Aggregate SQL functions as defined in general.
		/// </summary>
		/// <remarks>
		/// The results of this method should be integrated with the 
		/// specialization's data.
		/// </remarks>
		public virtual IDictionary AggregateFunctions 
		{
			get { return aggregateFunctions;}
		}
		
		/// <summary>
		/// The largest value that can be set in IDbDataParameter.Size for a parameter
		/// that contains an AnsiString.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Currently the only Driver that needs to worry about setting the Param size
		/// is MsSql
		/// </para>
		/// </remarks>
		public virtual int MaxAnsiStringSize 
		{
			get { throw new NotImplementedException("should be implemented by subclass if needed."); }
		}

		/// <summary>
		/// The largest value that can be set in IDbDataParameter.Size for a parameter
		/// that contains a DbType.Binary value.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Currently the only Driver that needs to worry about setting the Param size
		/// is MsSql
		/// </para>
		/// </remarks>
		public virtual int MaxBinarySize 
		{
			get { throw new NotImplementedException("should be implemented by subclass if needed."); }
		}

		/// <summary>
		/// The largest value that can be set in IDbDataParameter.Size for a parameter
		/// that contains a DbType.Binary value that is written to a BLOB column.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Currently the only Driver that needs to worry about setting the Param size
		/// is MsSql
		/// </para>
		/// </remarks>
		public virtual int MaxBinaryBlobSize 
		{
			get { throw new NotImplementedException("should be implemented by subclass if needed."); }
		}

		/// <summary>
		/// The largest value that can be set in IDbDataParameter.Size for a parameter
		/// that contains a Unicode String value that is written to a CLOB column.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Currently the only Driver that needs to worry about setting the Param size
		/// is MsSql.  		
		/// </para>
		/// </remarks>
		public virtual int MaxStringClobSize 
		{
			get { throw new NotImplementedException("should be implemented by subclass if needed."); }
		}

		/// <summary>
		/// The largest value that can be set in IDbDataParameter.Size for a parameter
		/// that contains an Unicode String value.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Setting the value to 0 indicates that there is no Maximum Size or that it 
		/// does not need to be set to Prepare the IDbCommand.
		/// </para>
		/// <para>
		/// Currently the only Driver that needs to worry about setting the Param size
		/// is MsSql
		/// </para>
		/// </remarks>
		public virtual int MaxStringSize 
		{
			get { throw new NotImplementedException("should be implemented by subclass - this will be converted to abstract"); }
		}
		
		/// <summary>
		/// Checks to see if the name has been quoted.
		/// </summary>
		/// <param name="name">The name to check if it is quoted</param>
		/// <returns>true if name is already quoted.</returns>
		/// <remarks>
		/// The default implementation is to compare the first character
		/// to Dialect.OpenQuote and the last char to Dialect.CloseQuote
		/// </remarks>
		public virtual bool IsQuoted(string name) 
		{
			return (name[0]==OpenQuote && name[name.Length-1]==CloseQuote);
		}

		/// <summary>
		/// Unquotes and unescapes an already quoted name
		/// </summary>
		/// <param name="quoted">Quoted string</param>
		/// <returns>Unquoted string</returns>
		/// <remarks>
		/// <p>
		/// This method checks the string <c>quoted</c> to see if it is 
		/// quoted.  If the string <c>quoted</c> is already enclosed in the OpenQuote
		/// and CloseQuote then those chars are removed.
		/// </p>
		/// <p>
		/// After the OpenQuote and CloseQuote have been cleaned from the string <c>quoted</c>
		/// then any chars in the string <c>quoted</c> that have been escaped by doubling them
		/// up are changed back to a single version.
		/// </p>
		/// <p>
		/// The following quoted values return these results
		/// "quoted" = quoted
		/// "quote""d" = quote"d
		/// quote""d = quote"d 
		/// </p>
		/// <p>
		/// If this implementation is not sufficient for your Dialect then it needs to be overridden.
		/// MsSql2000Dialect is an example of where UnQuoting rules are different.
		/// </p>
		/// </remarks>
		public virtual string UnQuote(string quoted)
		{
			string unquoted;

			if (IsQuoted(quoted)) 
				unquoted = quoted.Substring(1, quoted.Length - 2);
			else
				unquoted = quoted;

			unquoted = unquoted.Replace( new string(OpenQuote, 2), OpenQuote.ToString() );

			if( OpenQuote!= CloseQuote)
				unquoted = unquoted.Replace( new string(CloseQuote, 2), CloseQuote.ToString() );

			return unquoted;
		}

		/// <summary>
		/// Unquotes an array of Quoted Names.
		/// </summary>
		/// <param name="quoted">strings to Unquote</param>
		/// <returns>an array of unquoted strings.</returns>
		/// <remarks>
		/// This use UnQuote(string) for each string in the quoted array so
		/// it should not need to be overridden - only UnQuote(string) needs
		/// to be overridden unless this implementation is not sufficient.
		/// </remarks>
		public virtual string[] UnQuote(string[] quoted) 
		{
			string[] unquoted = new string[ quoted.Length ];
			
			for (int i=0; i<quoted.Length; i++)
				unquoted[i] = UnQuote(quoted[i]);

			return unquoted;
		}

		
		/// <summary>
		/// Quotes a name.
		/// </summary>
		/// <param name="name">The string that needs to be Quoted.</param>
		/// <returns>A QuotedName </returns>
		/// <remarks>
		/// <p>
		/// This method assumes that the name is not already Quoted.  So if the name passed
		/// in is <c>"name</c> then it will return <c>"""name"</c>.  It escapes the first char
		/// - the " with "" and encloses the escaped string with OpenQuote and CloseQuote. 
		/// </p>
		/// </remarks>
		protected virtual string Quote(string name) 
		{
			string quotedName = name.Replace( OpenQuote.ToString(), new string(OpenQuote, 2) );
			
			// in some dbs the Open and Close Quote are the same chars - if they are 
			// then we don't have to escape the Close Quote char because we already
			// got it.
			if(OpenQuote!=CloseQuote) 
			{
				quotedName = name.Replace( CloseQuote.ToString(), new string(CloseQuote, 2) );
			}

			return OpenQuote + quotedName + CloseQuote;
		}

		/// <summary>
		/// Quotes a name for being used as a aliasname
		/// </summary>
		/// <remarks>Original implementation calls <see cref="QuoteForTableName"/></remarks>
		/// <param name="columnName">Name of the alias</param>
		/// <returns>A Quoted name in the format of OpenQuote + aliasName + CloseQuote</returns>
		/// <remarks>
		/// <p>
		/// If the aliasName is already enclosed in the OpenQuote and CloseQuote then this 
		/// method will return the aliasName that was passed in without going through any
		/// Quoting process.  So if aliasName is passed in already Quoted make sure that 
		/// you have escaped all of the chars according to your DataBase's specifications.
		/// </p>
		/// </remarks>
		public virtual string QuoteForAliasName(string aliasName)
		{
			return IsQuoted(aliasName) ?
				aliasName :
				Quote(aliasName);

		}

		/// <summary>
		/// Quotes a name for being used as a columnname
		/// </summary>
		/// <remarks>Original implementation calls <see cref="QuoteForTableName"/></remarks>
		/// <param name="columnName">Name of the column</param>
		/// <returns>A Quoted name in the format of OpenQuote + columnName + CloseQuote</returns>
		/// <remarks>
		/// <p>
		/// If the columnName is already enclosed in the OpenQuote and CloseQuote then this 
		/// method will return the columnName that was passed in without going through any
		/// Quoting process.  So if columnName is passed in already Quoted make sure that 
		/// you have escaped all of the chars according to your DataBase's specifications.
		/// </p>
		/// </remarks>
		public virtual string QuoteForColumnName(string columnName)
		{
			return IsQuoted(columnName) ?
				columnName :
				Quote(columnName);

		}

		/// <summary>
		/// Quotes a name for being used as a tablename
		/// </summary>
		/// <param name="tableName">Name of the table</param>
		/// <returns>A Quoted name in the format of OpenQuote + tableName + CloseQuote</returns>
		/// <remarks>
		/// <p>
		/// If the tableName is already enclosed in the OpenQuote and CloseQuote then this 
		/// method will return the tableName that was passed in without going through any
		/// Quoting process.  So if tableName is passed in already Quoted make sure that 
		/// you have escaped all of the chars according to your DataBase's specifications.
		/// </p>
		/// </remarks>
		public virtual string QuoteForTableName(string tableName)
		{
			return IsQuoted(tableName) ?
				tableName :
				Quote(tableName);

		}

		
		public class CountQueryFunctionInfo : IQueryFunctionInfo 
		{
			#region IQueryFunctionInfo Members

			public IType QueryFunctionType(IType columnType, IMapping mapping)
			{
				return NHibernate.Int32;
			}

			public bool IsFunctionArgs
			{
				get { return true;}
			}

			public bool IsFunctionNoArgsUseParanthesis
			{
				get { return true;}
			}

			#endregion
		}

		public class AvgQueryFunctionInfo : IQueryFunctionInfo
		{
			#region IQueryFunctionInfo Members

			public IType QueryFunctionType(IType columnType, IMapping mapping)
			{
				SqlType[] sqlTypes;
				try 
				{
					sqlTypes = columnType.SqlTypes(mapping);
				}
				catch (MappingException me) 
				{
					throw new QueryException(me);
				}

				if(sqlTypes.Length!=1) throw new QueryException("multi-column type can not be in avg()");
				
				SqlType sqlType = sqlTypes[0];

				if(sqlType.DbType==DbType.Int16 || sqlType.DbType==DbType.Int32 || sqlType.DbType==DbType.Int64)
				{
					return NHibernate.Single;
				}
				else 
				{
					return columnType;
				}
			}

			public bool IsFunctionArgs
			{
				get { return true; }
			}

			public bool IsFunctionNoArgsUseParanthesis
			{
				get { return true; }
			}

			#endregion
		}


	}
}
