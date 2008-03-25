using System;
using System.Collections;
using System.Data;
using log4net;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Mapping;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.Util;
using System.Collections.Generic;
using System.Data.Common;

namespace NHibernate.Id
{
	/// <summary>
	/// An <see cref="IIdentifierGenerator" /> that generates <c>Int64</c> values using an 
	/// oracle-style sequence. A higher performance algorithm is 
	/// <see cref="SequenceHiLoGenerator"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	///	This id generation strategy is specified in the mapping file as 
	///	<code>
	///	&lt;generator class="sequence"&gt;
	///		&lt;param name="sequence"&gt;uid_sequence&lt;/param&gt;
	///		&lt;param name="schema"&gt;db_schema&lt;/param&gt;
	///	&lt;/generator&gt;
	///	</code>
	/// </p>
	/// <p>
	/// The <c>sequence</c> parameter is required while the <c>schema</c> is optional.
	/// </p>
	/// </remarks>
	public class SequenceGenerator : IPersistentIdentifierGenerator, IConfigurable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SequenceGenerator));

		/// <summary>
		/// The name of the sequence parameter.
		/// </summary>
		public const string Sequence = "sequence";

		/// <summary> 
		/// The parameters parameter, appended to the create sequence DDL.
		/// For example (Oracle): <tt>INCREMENT BY 1 START WITH 1 MAXVALUE 100 NOCACHE</tt>.
		/// </summary>
		public const string Parameters = "parameters";

		private string sequenceName;
		private IType identifierType;
		private SqlString sql;
		private string parameters;

		#region IConfigurable Members

		/// <summary>
		/// Configures the SequenceGenerator by reading the value of <c>sequence</c> and
		/// <c>schema</c> from the <c>parms</c> parameter.
		/// </summary>
		/// <param name="type">The <see cref="IType"/> the identifier should be.</param>
		/// <param name="parms">An <see cref="IDictionary"/> of Param values that are keyed by parameter name.</param>
		/// <param name="dialect">The <see cref="Dialect.Dialect"/> to help with Configuration.</param>
		public virtual void Configure(IType type, IDictionary<string, string> parms, Dialect.Dialect dialect)
		{
			sequenceName = PropertiesHelper.GetString(Sequence, parms, "hibernate_sequence");
			parameters = parms[Parameters];
			string schemaName = parms[PersistentIdGeneratorParmsNames.Schema];
			string catalogName = parms[PersistentIdGeneratorParmsNames.Catalog];

			if (sequenceName.IndexOf('.') < 0)
			{
				sequenceName = Table.Qualify(catalogName, schemaName, sequenceName);
			}

			identifierType = type;
			sql = new SqlString(dialect.GetSequenceNextValString(sequenceName));
		}

		#endregion

		#region IIdentifierGenerator Members

		/// <summary>
		/// Generate an <see cref="Int16"/>, <see cref="Int32"/>, or <see cref="Int64"/> 
		/// for the identifier by using a database sequence.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity for which the id is being generated.</param>
		/// <returns>The new identifier as a <see cref="Int16"/>, <see cref="Int32"/>, or <see cref="Int64"/>.</returns>
		public virtual object Generate(ISessionImplementor session, object obj)
		{
			try
			{
				IDbCommand cmd = session.Batcher.PrepareCommand(CommandType.Text, sql, SqlTypeFactory.NoTypes);
				IDataReader reader = null;
				try
				{
					reader = session.Batcher.ExecuteReader(cmd);
					try
					{
						reader.Read();
						object result = IdentifierGeneratorFactory.Get(reader, identifierType, session);
						if (log.IsDebugEnabled)
						{
							log.Debug("Sequence identifier generated: " + result);
						}
						return result;
					}
					finally
					{
						reader.Close();
					}
				}
				finally
				{
					session.Batcher.CloseCommand(cmd, reader);
				}
			}
			catch (DbException sqle)
			{
				log.Error("error generating sequence", sqle);
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle, "could not get next sequence value");
			}
		}

		#endregion

		#region IPersistentIdentifierGenerator Members

		/// <summary>
		/// The SQL required to create the database objects for a SequenceGenerator.
		/// </summary>
		/// <param name="dialect">The <see cref="Dialect.Dialect"/> to help with creating the sql.</param>
		/// <returns>
		/// An array of <see cref="String"/> objects that contain the Dialect specific sql to 
		/// create the necessary database objects for the SequenceGenerator.
		/// </returns>
		public string[] SqlCreateStrings(Dialect.Dialect dialect)
		{
			string baseDDL = dialect.GetCreateSequenceString(sequenceName);
			string paramsDDL = null;
			if (parameters != null)
			{
				paramsDDL = ' ' + parameters;
			}
			return new string[] { string.Concat(baseDDL,paramsDDL) };
		}

		/// <summary>
		/// The SQL required to remove the underlying database objects for a SequenceGenerator.
		/// </summary>
		/// <param name="dialect">The <see cref="Dialect.Dialect"/> to help with creating the sql.</param>
		/// <returns>
		/// A <see cref="String"/> that will drop the database objects for the SequenceGenerator.
		/// </returns>
		public string SqlDropString(Dialect.Dialect dialect)
		{
			return dialect.GetDropSequenceString(sequenceName);
		}

		/// <summary>
		/// Return a key unique to the underlying database objects for a SequenceGenerator.
		/// </summary>
		/// <returns>
		/// The configured sequence name.
		/// </returns>
		public string GeneratorKey()
		{
			return sequenceName;
		}

		#endregion
	}
}