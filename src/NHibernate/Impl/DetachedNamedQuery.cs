using System;

namespace NHibernate.Impl
{
	/// <summary>
	/// Named query in "detached mode" where the NHibernate session is not available.
	/// </summary>
	/// <seealso cref="AbstractDetachedQuery"/>
	/// <seealso cref="IDetachedQuery"/>
	/// <seealso cref="IQuery"/>
	/// <seealso cref="ISession.GetNamedQuery(string)"/>
	[Serializable]
	public class DetachedNamedQuery : AbstractDetachedQuery
	{
		private readonly string queryName;
		/// <summary>
		/// Create a new instance of <see cref="DetachedNamedQuery"/> for a named query string defined in the mapping file.
		/// </summary>
		/// <param name="queryName">The name of a query defined externally.</param>
		/// <remarks>
		/// The query can be either in HQL or SQL format.
		/// </remarks>
		public DetachedNamedQuery(string queryName)
		{
			this.queryName = queryName;
		}

		/// <summary>
		/// Get the query name.
		/// </summary>
		public string QueryName
		{
			get { return queryName; }
		}

		public override IQuery GetExecutableQuery(ISession session)
		{
			IQuery result = session.GetNamedQuery(queryName);
			SetQueryProperties(result);
			return result;
		}
	}
}