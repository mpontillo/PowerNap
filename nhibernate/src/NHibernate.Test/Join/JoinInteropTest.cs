using System;
using log4net;
using NHibernate.Linq;
using NUnit.Framework;
using System.Collections;
using System.Data;

namespace NHibernate.Test.Join
{
	using NHibernate.Test.Subclass;

	/// <summary>
	/// Test case to ensure that NHibernate is interoperable with joined tables created
	/// by third parties whose rows can have all NULL values.
	/// </summary>
	/// <remarks>
	/// Not all ORM frameworks handle a "join table" the same way that NHibernate does.
	/// In particular, some frameworks will leave a row in a "join table" with all NULL
	/// properties. NHibernate cannot deal with this situation, since it assumes that if
	/// all the joined table's properties are null, it should never have existed.
	/// </remarks>
	[TestFixture]
	public class JoinInteropTest : TestCase
	{
		private static ILog log = LogManager.GetLogger(typeof(JoinInteropTest));

		#region TestCase Overrides

		private void ExecuteSql(ISession session, string sql)
		{
			IDbCommand command = session.Connection.CreateCommand();
			session.Transaction.Enlist(command);
			command.CommandText = sql;
			command.ExecuteNonQuery();
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			// This covers all recent versions of SQL server.
			// Restricting this test case since we explicitly do SQL inserts 
			// in the test setup.
			if (dialect is Dialect.MsSql2000Dialect)
			{
				return true;
			}

			return false;
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = this.OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				// ... and need to bypass NHibernate to insert the following rows, in order
				// to simulate rows in a legacy database that were "already there".
				ExecuteSql(session, "insert into Info values (1, 'Data with NULL MoreData')");
				ExecuteSql(session, "insert into ExtendedInfo values (1, NULL)");

				ExecuteSql(session, "insert into Info values (2, 'Data with non-null MoreData')");
				ExecuteSql(session, "insert into ExtendedInfo values (2, 'Some non-null MoreData')");
				
				ExecuteSql(session, "insert into Info values (3, NULL)");
				ExecuteSql(session, "insert into ExtendedInfo values (3, 'non-null MoreData with null Data')");

				// Everything NULL - not sure this will ever work?
				ExecuteSql(session, "insert into Info values (4, NULL)");
				ExecuteSql(session, "insert into ExtendedInfo values (4, NULL)");

				tx.Commit();
			}

		}

		protected override void OnTearDown()
		{
			base.OnTearDown();
			using (ISession session = this.OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				// Not using HQL to delete since this may have other problems
				ExecuteSql(session, "delete from ExtendedInfo");
				ExecuteSql(session, "delete from Info");
				tx.Commit();
			}
		}

		#endregion TestCase Overrides


		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get
			{
				return new string[] { 
					"Join.JoinInteropMappings.hbm.xml",
				};
			}
		}

		[Test]
		public void TestGetTestDataHql()
		{
			using (var session = this.OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var query = session.CreateQuery("from " + typeof(Info).Name);
				var list = query.List<Info>();
				foreach (var element in list)
				{
					Assert.That(element != null);

					Console.WriteLine("{0} ('{1}', '{2}')", element.Id, element.Data, element.MoreData);
				}

				var isDirty = session.IsDirty();
				Assert.That(isDirty == false);

				tx.Commit();
			}
		}

		[Test]
		public void TestGetTestDataLinq()
		{
			using (var session = this.OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var query = session.Query<Info>();

				foreach (var element in query)
				{
					Console.WriteLine("{0} ('{1}', '{2}')", element.Id, element.Data, element.MoreData);
				}

				var isDirty = session.IsDirty();
				Assert.That(isDirty == false);

				tx.Commit();
			}
		}
	}
}