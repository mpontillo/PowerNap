using System;
using System.Collections;
using System.Data;
using System.Text;

using NHibernate.Cache;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Transaction;
using NHibernate.Util;

namespace NHibernate.Cfg
{
	/// <summary>
	/// Reads configuration properties and configures a <see cref="Settings"/> instance. 
	/// </summary>
	public sealed class SettingsFactory
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof(SettingsFactory) );

		private SettingsFactory()
		{
			//should not be publically creatable
		}

		public static Settings BuildSettings(IDictionary properties) 
		{
			Settings settings = new Settings();

			Dialect.Dialect dialect = null;
			try 
			{
				dialect = Dialect.Dialect.GetDialect( properties );
				IDictionary temp = new Hashtable();
				
				foreach( DictionaryEntry de in dialect.DefaultProperties ) 
				{
					temp[ de.Key ] = de.Value;
				}
				foreach( DictionaryEntry de in properties ) 
				{
					temp[ de.Key ] = de.Value;
				}
				properties = temp;
			} 
			catch( HibernateException he ) 
			{
				log.Warn( "No dialect set - using GenericDialect: " + he.Message );
				dialect = new GenericDialect();
			}

			
			bool useOuterJoin = PropertiesHelper.GetBoolean(Cfg.Environment.OuterJoin, properties, true);
			log.Info( "use outer join fetching: " + useOuterJoin );

			IConnectionProvider connectionProvider = ConnectionProviderFactory.NewConnectionProvider(properties);
			ITransactionFactory transactionFactory = new TransactionFactory(); //Transaction BuildTransactionFactory(properties);

			string isolationString = PropertiesHelper.GetString( Cfg.Environment.Isolation, properties, String.Empty );
			IsolationLevel isolation = IsolationLevel.Unspecified;
			if( isolationString.Length > 0) 
			{
				try 
				{
					isolation = (IsolationLevel)Enum.Parse( typeof(IsolationLevel), isolationString );
					log.Info( "Using Isolation Level: " + isolation.ToString() );
				}
				catch( ArgumentException ae ) 
				{
					log.Error( "error configuring IsolationLevel " + isolationString, ae );
					throw new HibernateException( 
						"The isolation level of " + isolationString + " is not a valid IsolationLevel.  Please " +
						"use one of the Member Names from the IsolationLevel.", ae );
				}
			}

			string defaultSchema = properties[Cfg.Environment.DefaultSchema] as string;
			if ( defaultSchema!=null) log.Info ("Default schema set to: " + defaultSchema);
			
			bool showSql = PropertiesHelper.GetBoolean( Cfg.Environment.ShowSql, properties, false );
			if (showSql) log.Info("echoing all SQL to stdout");

			// queries:

			IDictionary querySubstitutions = PropertiesHelper.ToDictionary( Cfg.Environment.QuerySubstitutions, " ,=;:\n\t\r\f", properties );
			if ( log.IsInfoEnabled ) 
			{
				StringBuilder sb = new StringBuilder("Query language substitutions: ");
				foreach(DictionaryEntry entry in querySubstitutions) 
				{
					sb.AppendFormat("{0}={1};", entry.Key, entry.Value);
				}
				log.Info(sb.ToString());
			}

			string cacheClassName = PropertiesHelper.GetString( Environment.CacheProvider, properties, "NHibernate.Cache.HashtableCacheProvider" );
			ICacheProvider cacheProvider = null;
			log.Info( "cache provider: " + cacheClassName );
			try 
			{
				cacheProvider = (ICacheProvider) Activator.CreateInstance( ReflectHelper.ClassForName( cacheClassName ) );
			}
			catch( Exception e ) 
			{
				throw new HibernateException( "could not instantiate CacheProvider: " + cacheClassName, e );
			}
			
			bool prepareSql = PropertiesHelper.GetBoolean( Environment.PrepareSql, properties, true );

			string sessionFactoryName = (string) properties[ Cfg.Environment.SessionFactoryName ];

			settings.DefaultSchemaName = defaultSchema;
			settings.IsShowSqlEnabled = showSql;
			settings.Dialect = dialect;
			settings.IsolationLevel = isolation;
			settings.ConnectionProvider = connectionProvider;
			settings.QuerySubstitutions = querySubstitutions;
			settings.TransactionFactory = transactionFactory;
			settings.CacheProvider = cacheProvider;
			settings.SessionFactoryName = sessionFactoryName;
			settings.IsOuterJoinFetchEnabled = useOuterJoin;
			settings.PrepareSql = prepareSql;

			return settings;

			
		}

	}
}
