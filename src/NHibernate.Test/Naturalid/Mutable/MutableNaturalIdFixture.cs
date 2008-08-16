using System;
using System.Collections;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Environment=NHibernate.Cfg.Environment;

namespace NHibernate.Test.Naturalid.Mutable
{
	[TestFixture]
	public class MutableNaturalIdFixture : TestCase
	{
		// TODO : Complete the test as H3.2.6
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get { return new string[] {"Naturalid.Mutable.User.hbm.xml"}; }
		}

		protected override void Configure(Configuration configuration)
		{
			cfg.SetProperty(Environment.UseSecondLevelCache, "true");
			cfg.SetProperty(Environment.UseQueryCache, "true");
			cfg.SetProperty(Environment.GenerateStatistics, "true");
		}

		[Test]
		public void ReattachmentNaturalIdCheck()
		{
			ISession s = OpenSession();
			s.BeginTransaction();
			User u = new User("gavin", "hb", "secret");
			s.Persist(u);
			s.Transaction.Commit();
			s.Close();

			FieldInfo name = u.GetType().GetField("name",
			                                      BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			name.SetValue(u, "Gavin");
			s = OpenSession();
			s.BeginTransaction();
			try
			{
				s.Update(u);
				s.Transaction.Commit();
			}
			catch (HibernateException)
			{
				s.Transaction.Rollback();
			}
			catch (Exception)
			{
				try
				{
					s.Transaction.Rollback();
				}
				catch (Exception) {}
				throw;
			}
			finally
			{
				s.Close();
			}

			s = OpenSession();
			s.BeginTransaction();
			s.Delete(u);
			s.Transaction.Commit();
			s.Close();
		}

		[Test]
		public void NonexistentNaturalIdCache()
		{
			sessions.Statistics.Clear();

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();

			object nullUser =
				s.CreateCriteria(typeof (User))
				.Add(Restrictions.NaturalId().Set("name", "gavin").Set("org", "hb"))
				.SetCacheable(true)
				.UniqueResult();

			Assert.That(nullUser, Is.Null);

			t.Commit();
			s.Close();

			Assert.AreEqual(1, sessions.Statistics.QueryExecutionCount);
			Assert.AreEqual(0, sessions.Statistics.QueryCacheHitCount);
			Assert.AreEqual(0, sessions.Statistics.QueryCachePutCount);

			s = OpenSession();
			t = s.BeginTransaction();

			User u = new User("gavin", "hb", "secret");
			s.Persist(u);

			t.Commit();
			s.Close();

			sessions.Statistics.Clear();

			s = OpenSession();
			t = s.BeginTransaction();

			u =(User) s.CreateCriteria(typeof (User))
				.Add(Restrictions.NaturalId().Set("name", "gavin").Set("org", "hb"))
				.SetCacheable(true).UniqueResult();

			Assert.That(u, Is.Not.Null);

			t.Commit();
			s.Close();

			Assert.AreEqual(1, sessions.Statistics.QueryExecutionCount);
			Assert.AreEqual(0, sessions.Statistics.QueryCacheHitCount);
			Assert.AreEqual(1, sessions.Statistics.QueryCachePutCount);

			sessions.Statistics.Clear();

			s = OpenSession();
			t = s.BeginTransaction();

			u =(User) s.CreateCriteria(typeof (User))
				.Add(Restrictions.NaturalId().Set("name", "gavin").Set("org", "hb"))
				.SetCacheable(true).UniqueResult();

			s.Delete(u);

			t.Commit();
			s.Close();

			Assert.AreEqual(0, sessions.Statistics.QueryExecutionCount);
			Assert.AreEqual(1, sessions.Statistics.QueryCacheHitCount);

			sessions.Statistics.Clear();

			s = OpenSession();
			t = s.BeginTransaction();

			nullUser = s.CreateCriteria(typeof (User))
				.Add(Restrictions.NaturalId().Set("name", "gavin").Set("org", "hb"))
				.SetCacheable(true).UniqueResult();

			Assert.That(nullUser, Is.Null);

			t.Commit();
			s.Close();

			Assert.AreEqual(1, sessions.Statistics.QueryExecutionCount);
			Assert.AreEqual(0, sessions.Statistics.QueryCacheHitCount);
			Assert.AreEqual(0, sessions.Statistics.QueryCachePutCount);
		}

		[Test]
		public void NaturalIdCache()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();

			User u = new User("gavin", "hb", "secret");
			s.Persist(u);

			t.Commit();
			s.Close();

			sessions.Statistics.Clear();

			s = OpenSession();
			t = s.BeginTransaction();

			u = (User) s.CreateCriteria(typeof (User))
				.Add(Restrictions.NaturalId().Set("name", "gavin").Set("org", "hb"))
				.SetCacheable(true).UniqueResult();

			Assert.That(u, Is.Not.Null);

			t.Commit();
			s.Close();

			Assert.AreEqual(1, sessions.Statistics.QueryExecutionCount);
			Assert.AreEqual(0, sessions.Statistics.QueryCacheHitCount);
			Assert.AreEqual(1, sessions.Statistics.QueryCachePutCount);

			s = OpenSession();
			t = s.BeginTransaction();

			User v = new User("xam", "hb", "foobar");
			s.Persist(v);

			t.Commit();
			s.Close();

			sessions.Statistics.Clear();

			s = OpenSession();
			t = s.BeginTransaction();

			u = (User) s.CreateCriteria(typeof (User))
				.Add(Restrictions.NaturalId().Set("name", "gavin").Set("org", "hb"))
				.SetCacheable(true).UniqueResult();

			Assert.That(u, Is.Not.Null);

			t.Commit();
			s.Close();

			Assert.AreEqual(0, sessions.Statistics.QueryExecutionCount);
			Assert.AreEqual(1, sessions.Statistics.QueryCacheHitCount);

			s = OpenSession();
			t = s.BeginTransaction();
			s.Delete("from User");
			t.Commit();
			s.Close();
		}

		[Test]
		public void Querying()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();

			User u = new User("emmanuel", "hb", "bh");
			s.Persist(u);

			t.Commit();
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();

			u = (User) s.CreateQuery("from User u where u.name = :name").SetParameter("name", "emmanuel").UniqueResult();
			Assert.AreEqual("emmanuel", u.Name);
			s.Delete(u);

			t.Commit();
			s.Close();
		}
	}
}