using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;

using log4net;

namespace NHibernate.Cfg
{
	/// <summary>
	/// Provides access to configuration info
	/// </summary>
	/// <remarks>
	/// Hibernate has two property scopes:
	/// <list>
	///		<item>
	///		 Factory-Level properties may be passed to the <c>ISessionFactory</c> when it is instantiated.
	///		 Each instance might have different property values. If no properties are specified, the
	///		 factory gets them from Environment
	///		</item>
	///		<item>
	///		 System-Level properties are shared by all factory instances and are always determined
	///		 by the <c>Environment</c> properties
	///		</item>
	/// </list>
	/// </remarks>
	public sealed class Environment
	{
		/// <summary></summary>
		public const string Version = "0.9.0.0";

		/// <summary></summary>
		public const string ConnectionProvider = "hibernate.connection.provider";
		/// <summary></summary>
		public const string ConnectionDriver = "hibernate.connection.driver_class";
		/// <summary></summary>
		public const string ConnectionString = "hibernate.connection.connection_string";
		/// <summary></summary>
		public const string Isolation = "hibernate.connection.isolation";
		/// <summary></summary>
		public const string SessionFactoryName = "hibernate.session_factory_name";
		/// <summary></summary>
		public const string Dialect = "hibernate.dialect";
		/// <summary></summary>
		public const string DefaultSchema = "hibernate.default_schema";
		/// <summary></summary>
		public const string ShowSql = "hibernate.show_sql";
		/// <summary></summary>
		public const string UseOuterJoin = "hibernate.use_outer_join";
		/// <summary></summary>
		public const string MaxFetchDepth = "hibernate.max_fetch_depth";
		/// <summary></summary>
		public const string UseGetGeneratedKeys = "hibernate.jdbc.use_get_generated_keys";
		/// <summary></summary>
		public const string StatementFetchSize = "hibernate.jdbc.fetch_size";
		/// <summary></summary>
		public const string StatementBatchSize = "hibernate.jdbc.batch_size";
		/// <summary></summary>
		public const string BatchVersionedData = "hibernate.jdbc.batch_versioned_data";
		/// <summary></summary>
		public const string OutputStylesheet = "hibernate.xml.output_stylesheet";
		/// <summary></summary>
		public const string TransactionStrategy = "hibernate.transaction.factory_class";
		/// <summary></summary>
		public const string TransactionManagerStrategy = "hibernate.transaction.manager_lookup_class";
		/// <summary></summary>
		public const string CacheProvider = "hibernate.cache.provider_class";
		public const string UseQueryCache = "hibernate.cache.use_query_cache";
		public const string QueryCacheFactory = "hibernate.cache.query_cache_factory";
		public const string CacheRegionPrefix = "hibernate.cache.region_prefix";
		public const string UseMinimalPuts = "hibernate.cache.use_minimal_puts";
		/// <summary></summary>
		public const string QuerySubstitutions = "hibernate.query.substitutions";
		/// <summary></summary>
		public const string QueryImports = "hibernate.query.imports";
		public const string Hbm2ddlAuto = "hibernate.hbm2ddl.auto";
		public const string SqlExceptionConverter = "hibernate.sql_exception_converter";
		public const string WrapResultSets = "hibernate.wrap_result_sets";

		// NHibernate-specific properties

		/// <summary></summary>
		public const string PrepareSql = "hibernate.prepare_sql";
		/// <summary></summary>
		public const string CommandTimeout = "hibernate.command_timeout";

		private static readonly ILog log = LogManager.GetLogger( typeof( Environment ) );

		private static IDictionary properties = new Hashtable();

		private static XmlNode configNode = null;

		private static bool isConfigured = false;

		internal static void Configure()
		{
			if( !isConfigured )
			{
				bool configurationFound = false;

				configurationFound = ConfigureFromNameValueCollection();
				if( !configurationFound )
				{
					configurationFound = ConfigureFromXmlNode();
				}
				if( !configurationFound )
				{
					log.Debug( "no hibernate settings in app.config/web.config were found" );
				}
				isConfigured = true;
			}
		}

		private static bool ConfigureFromXmlNode()
		{
			configNode = ConfigurationSettings.GetConfig( "hibernate-configuration" ) as XmlNode;
			return configNode != null;
		}

		private static bool ConfigureFromNameValueCollection()
		{
			NameValueCollection props = ConfigurationSettings.GetConfig( "nhibernate" ) as NameValueCollection;
			if( props != null )
			{
				foreach( string key in props.Keys )
				{
					properties[ key ] = props[ key ];
				}
				return true;
			}
			return false;
		}

		internal static XmlNode ConfigurationNode
		{
			get { return configNode; }
		}

		private Environment()
		{
			// should not be created.	
		}

		/// <summary>
		/// Gets a copy of the configuration found in app.config/web.config
		/// </summary>
		/// <remarks>
		/// This is the replacement for hibernate.properties
		/// </remarks>
		public static IDictionary Properties
		{
			get
			{
				IDictionary copy = new Hashtable( properties.Count );
				foreach( DictionaryEntry de in properties )
				{
					copy[ de.Key ] = de.Value;
				}
				return copy;
			}
		}

		internal static void SetProperties( IDictionary props )
		{
			properties = props;
		}

		/// <summary></summary>
		public static bool UseStreamsForBinary
		{
			get { return true; }
		}

		/// <summary>
		/// Issue warnings to user when any obsolete property names are used.
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static void VerifyProperties( IDictionary props )
		{
		}
	}
}