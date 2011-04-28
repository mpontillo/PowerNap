using System;
using System.Data;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2667
{
    /// <summary>
    /// Used for testing LINQ queries in NHibernate that compare enumeration values.
    /// </summary>
    [TestFixture]
    public class Fixture : BugTestCase
    {
        #region BugTestCase Overrides

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
            // in the test setup, and also modify the schema so NULL values
            // are allowed in the version column.
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
                // Unsure if there is a cross-platform way to do this.
                // (Need to drop the "not null" constraint.)
                ExecuteSql(session, "alter table Info alter column Version INT");

                // ... and need to bypass NHibernate to insert the following rows, in order
                // to simulate rows in a legacy database that were "already there".
                ExecuteSql(session, "insert into Info values (1, 'Positive version')");
                ExecuteSql(session, "insert into Info values (0, 'Zero version')");
                ExecuteSql(session, "insert into Info values (null, 'NULL version')");
                ExecuteSql(session, "insert into Info values (-1, 'Negative version')");

                tx.Commit();
            }

        }

        protected override void OnTearDown()
        {
            base.OnTearDown();
            using (ISession session = this.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                // Not using HQL to delete since this has other problems,
                // which are tested for in a separate test case: TestDelete().
                ExecuteSql(session, "delete from Info");
                tx.Commit();
            }
        }

        #endregion BugTestCase Overrides

		/// <summary>
		/// In some cases, it appeared as though NHibernate considered NULL 
		/// version properties "dirty" all the time, even though nothing in
		/// the entity had been changed. This test case attempts to check for 
		/// that.
		/// </summary>
        [Test]
        public void TestGetTestDataLinq()
        {
            using (var session = this.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var query = session.Query<Info>();
                
                foreach(var result in query)
                {
                    Console.WriteLine("{0}", result.Data);
                }

                var isDirty = session.IsDirty();
                Assert.That(isDirty == false);

                tx.Commit();
            }
        }

		/// <summary>
		/// In some cases, it appeared as though NHibernate considered NULL 
		/// version properties "dirty" all the time, even though nothing in
		/// the entity had been changed. This test case attempts to check for 
		/// that.
		/// </summary>
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
                }

                var isDirty = session.IsDirty();
                Assert.That(isDirty == false);

                tx.Commit();
            }
        }

		/// <summary>
		/// NHibernate fails to delete a row because of an incorrect WHERE 
		/// clause if a version property is null. This test case checks
		/// to ensure the delete does not throw that exception.
		/// </summary>
        [Test]
        public void TestDelete()
        {
            base.OnTearDown();
            using (ISession session = this.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                const string hql = "from System.Object";
                session.Delete(hql);
                session.Flush();

                tx.Commit();
            }

            // Expect no exception. (Can get NHibernate.PropertyValueException because the version property can be null.)
        }

		/// <summary>
		/// NHibernate fails to update a row because of an incorrect WHERE 
		/// clause if a version property is null. This test case checks
		/// to ensure the update does not throw that exception.
		/// </summary>
        [Test]
        public void TestUpdate()
        {
            base.OnTearDown();
            using (ISession session = this.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                var query = session.CreateQuery("from " + typeof(Info).Name);
                var list = query.List<Info>();
                foreach (var element in list)
                {
                    element.Data = "Updated " + element.Data;
                }

                tx.Commit();
            }

            // Expect no exception. (Can get NHibernate.PropertyValueException because the version property can be null.)
        }
    }
}
