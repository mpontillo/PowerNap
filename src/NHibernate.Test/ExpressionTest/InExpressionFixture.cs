using System;

using NHibernate.DomainModel;
using NHibernate.Engine;
using NHibernate.Expression;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Util;
using NUnit.Framework;

using NExpression = NHibernate.Expression;

namespace NHibernate.Test.ExpressionTest
{
	/// <summary>
	/// Summary description for InExpressionFixture.
	/// </summary>
	[TestFixture]
	public class InExpressionFixture : BaseExpressionFixture
	{
		[Test]
		public void InSqlStringTest()
		{
			ISession session = factory.OpenSession();

			ICriterion inExpression = Expression.Expression.In( "Count", new int[ ] {3, 4, 5} );

			CreateObjects( typeof( Simple ), session );
			SqlString sqlString = inExpression.ToSqlString(criteria, criteriaQuery, CollectionHelper.EmptyMap);

			string expectedSql = "sql_alias.count_ in (?, ?, ?)";

			CompareSqlStrings( sqlString, expectedSql, 3 );

			session.Close();
		}

		[Test]
		public void InEmptyList()
		{
			ISession session = factory.OpenSession();
			InExpression expression = new InExpression( "Count", new object[0] );
			CreateObjects( typeof( Simple ), session );
			SqlString sql = expression.ToSqlString(criteria, criteriaQuery, CollectionHelper.EmptyMap);
			Assert.AreEqual( "1=0", sql.ToString() );
			session.Close();
		}
	}
}