﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NUnit.Framework;
using FirebirdSql.Data.FirebirdClient;

namespace NHibernate.TestDatabaseSetup
{
    [TestFixture]
    public class DatabaseSetup
    {
		private static IDictionary<string, Action<Cfg.Configuration>> SetupMethods;

		static DatabaseSetup()
		{
			SetupMethods = new Dictionary<string, Action<Cfg.Configuration>>();
			SetupMethods.Add("NHibernate.Driver.SqlClientDriver", SetupSqlServer);
			SetupMethods.Add("NHibernate.Driver.FirebirdClientDriver", SetupFirebird);
		}

		[Test]
		public void SetupDatabase()
		{
            var cfg = new Cfg.Configuration();
			var driver = cfg.Properties[Cfg.Environment.ConnectionDriver];

			Assert.That(SetupMethods.ContainsKey(driver), "No setup method found for " + driver);

			var setupMethod = SetupMethods[driver];
			setupMethod(cfg);
		}

        private static void SetupSqlServer(Cfg.Configuration cfg)
        {
            var connStr = cfg.Properties[Cfg.Environment.ConnectionString];
			
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (var cmd = new System.Data.SqlClient.SqlCommand("use master", conn))
                {
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "drop database nhibernate";

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch(Exception)
                    {
                    }

                    cmd.CommandText = "create database nhibernate";
                    cmd.ExecuteNonQuery();
                }
            }
        }

		private static void SetupFirebird(Cfg.Configuration cfg)
		{
			FbConnection.CreateDatabase("Database=NHibernate.fdb;ServerType=1");
		}
    }
}


