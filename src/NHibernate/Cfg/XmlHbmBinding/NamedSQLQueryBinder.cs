using System;
using System.Collections;
using System.Xml;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Engine;
using NHibernate.Util;

namespace NHibernate.Cfg.XmlHbmBinding
{
	public class NamedSQLQueryBinder : QueryBinder
	{
		public NamedSQLQueryBinder(Mappings mappings, XmlNamespaceManager namespaceManager)
			: base(mappings, namespaceManager)
		{
		}

		public NamedSQLQueryBinder(XmlBinder parent)
			: base(parent)
		{
		}

		public void AddNamedSqlQuery(HbmSqlQuery querySchema)
		{
			mappings.AddSecondPass(delegate
				{
					string queryName = querySchema.name;
					string queryText = querySchema.GetText();
					bool cacheable = false;
					string region = null;
					int timeout = -1;
					int fetchSize = -1;
					bool readOnly = true;
					string comment = null;
					bool callable = false;
					string resultSetRef = querySchema.resultsetref;

					FlushMode flushMode = GetFlushMode(querySchema.flushmodeSpecified,
						querySchema.flushmode);

					IDictionary parameterTypes = new SequencedHashMap();
					IList synchronizedTables = GetSynchronizedTables(querySchema);

					NamedSQLQueryDefinition namedQuery;

					if (string.IsNullOrEmpty(resultSetRef))
					{
						ResultSetMappingDefinition definition =
							new ResultSetMappingBinder(this).Create(querySchema);

						namedQuery = new NamedSQLQueryDefinition(queryText,
							definition.GetQueryReturns(), synchronizedTables, cacheable, region, timeout,
							fetchSize, flushMode, readOnly, comment, parameterTypes, callable);
					}
					else
						// TODO: check there is no actual definition elemnents when a ref is defined
						namedQuery = new NamedSQLQueryDefinition(queryText,
							resultSetRef, synchronizedTables, cacheable, region, timeout, fetchSize,
							flushMode, readOnly, comment, parameterTypes, callable);

					log.DebugFormat("Named SQL query: {0} -> {1}", queryName, namedQuery.QueryString);
					mappings.AddSQLQuery(queryName, namedQuery);
				});
		}

		private static FlushMode GetFlushMode(bool flushModeSpecified, HbmFlushMode flushMode)
		{
			if (!flushModeSpecified)
				return FlushMode.Unspecified;

			switch (flushMode)
			{
				case HbmFlushMode.Auto:
					return FlushMode.Auto;

				case HbmFlushMode.Never:
					return FlushMode.Never;

				default:
					throw new ArgumentOutOfRangeException("flushMode");
			}
		}

		private static IList GetSynchronizedTables(HbmSqlQuery querySchema)
		{
			IList synchronizedTables = new ArrayList();

			foreach (object item in querySchema.Items ?? new object[0])
			{
				HbmSynchronize synchronizeSchema = item as HbmSynchronize;

				if (synchronizeSchema != null)
					synchronizedTables.Add(synchronizeSchema.table);
			}

			return synchronizedTables;
		}
	}
}