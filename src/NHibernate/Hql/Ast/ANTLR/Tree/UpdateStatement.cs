using System;
using Antlr.Runtime;
using log4net;
using NHibernate.Hql.Ast.ANTLR.Util;

namespace NHibernate.Hql.Ast.ANTLR.Tree
{
	/// <summary> 
	/// Defines a top-level AST node representing an HQL update statement. 
	/// </summary>
	[Serializable]
	[CLSCompliant(false)]
	public class UpdateStatement : AbstractRestrictableStatement
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (UpdateStatement));

		public UpdateStatement(IToken token) : base(token) {}

		public override bool NeedsExecutor
		{
			get { return true; }
		}

		public override int StatementType
		{
			get { return HqlSqlWalker.UPDATE; }
		}

		public IASTNode SetClause
		{
			get { return ASTUtil.FindTypeInChildren(this, HqlSqlWalker.SET); }
		}

		protected override ILog GetLog()
		{
			return log;
		}

		protected override int GetWhereClauseParentTokenType()
		{
			return HqlSqlWalker.SET;
		}
	}
}