using System;
using System.Data;
using System.Text;

using NHibernate.Engine;
using NExpression = NHibernate.Expression;
using NHibernate.SqlCommand;
using NHibernate.Type;

using NHibernate.DomainModel;
using NUnit.Framework;

namespace NHibernate.Test.ExpressionTest
{
	/// <summary>
	/// Summary description for NotExpressionFixture.
	/// </summary>
	[TestFixture]
	public class NotExpressionFixture : BaseExpressionFixture
	{
		[Test]
		public void NotSqlStringTest() 
		{
			ISession session = factory.OpenSession();
			
			NExpression.ICriterion notExpression = NExpression.Expression.Not(NExpression.Expression.Eq("Address", "12 Adress"));

			CreateObjects( typeof( Simple ), session );
			SqlString sqlString = notExpression.ToSqlString( criteria, criteriaQuery );

			string expectedSql = dialect is Dialect.MySQLDialect ?
				"not (sql_alias.address = :sql_alias.address)" :
				"not sql_alias.address = :sql_alias.address";
			
			Parameter firstParam = new Parameter( "address", "sql_alias", new SqlTypes.StringSqlType() );
			CompareSqlStrings(sqlString, expectedSql, new Parameter[] {firstParam});
			
			session.Close();
		}
		
	}
}
