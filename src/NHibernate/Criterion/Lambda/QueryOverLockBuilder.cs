
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using NHibernate.Impl;
using NHibernate.SqlCommand;

namespace NHibernate.Criterion.Lambda
{

	public class QueryOverLockBuilder<S,T> : QueryOverLockBuilderBase<QueryOver<S,T>, S, T>
	{

		public QueryOverLockBuilder(QueryOver<S,T> root, Expression<Func<object>> alias)
			: base(root, alias) { }

	}

	public class IQueryOverLockBuilder<S,T> : QueryOverLockBuilderBase<IQueryOver<S,T>, S, T>
	{

		public IQueryOverLockBuilder(IQueryOver<S,T> root, Expression<Func<object>> alias)
			: base(root, alias) { }

	}

	public class QueryOverLockBuilderBase<R, S, T> where R : IQueryOver<S,T>
	{

		protected R root;
		protected string alias;

		protected QueryOverLockBuilderBase(R root, Expression<Func<object>> alias)
		{
			this.root = root;

			if (alias != null)
				this.alias = ExpressionProcessor.FindMemberExpression(alias.Body);
		}

		private void SetLockMode(LockMode lockMode)
		{
			if (alias != null)
				root.UnderlyingCriteria.SetLockMode(alias, lockMode);
			else
				root.UnderlyingCriteria.SetLockMode(lockMode);
		}

		public R Force
		{
			get
			{
				SetLockMode(LockMode.Force);
				return this.root;
			}
		}

		public R None
		{
			get
			{
				SetLockMode(LockMode.None);
				return this.root;
			}
		}

		public R Read
		{
			get
			{
				SetLockMode(LockMode.Read);
				return this.root;
			}
		}

		public R Upgrade
		{
			get
			{
				SetLockMode(LockMode.Upgrade);
				return this.root;
			}
		}

		public R UpgradeNoWait
		{
			get
			{
				SetLockMode(LockMode.UpgradeNoWait);
				return this.root;
			}
		}

		public R Write
		{
			get
			{
				SetLockMode(LockMode.Write);
				return this.root;
			}
		}

	}

}
