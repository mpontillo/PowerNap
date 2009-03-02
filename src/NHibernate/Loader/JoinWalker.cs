using System;
using System.Collections.Generic;
using System.Text;
using Iesi.Collections.Generic;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Loader
{
	public class JoinWalker
	{
		private readonly ISessionFactoryImplementor factory;
		protected readonly IList<OuterJoinableAssociation> associations = new List<OuterJoinableAssociation>();
		private readonly ISet<AssociationKey> visitedAssociationKeys = new HashedSet<AssociationKey>();
		private readonly IDictionary<string, IFilter> enabledFilters;

		public string[] CollectionSuffixes { get; set; }

		public LockMode[] LockModeArray { get; set; }

		public string[] Suffixes { get; set; }

		public string[] Aliases { get; set; }

		public int[] CollectionOwners { get; set; }

		public ICollectionPersister[] CollectionPersisters { get; set; }

		public EntityType[] OwnerAssociationTypes { get; set; }

		public int[] Owners { get; set; }

		public ILoadable[] Persisters { get; set; }

		public SqlString SqlString { get; set; }

		protected ISessionFactoryImplementor Factory
		{
			get { return factory; }
		}

		protected Dialect.Dialect Dialect
		{
			get { return factory.Dialect; }
		}

		protected IDictionary<string, IFilter> EnabledFilters
		{
			get { return enabledFilters; }
		}

		protected virtual bool IsTooManyCollections
		{
			get { return false; }
		}

		protected JoinWalker(ISessionFactoryImplementor factory, IDictionary<string, IFilter> enabledFilters)
		{
			this.factory = factory;
			this.enabledFilters = enabledFilters;
		}

		/// <summary>
		/// Add on association (one-to-one, many-to-one, or a collection) to a list
		/// of associations to be fetched by outerjoin (if necessary)
		/// </summary>
		private void AddAssociationToJoinTreeIfNecessary(IAssociationType type, string[] aliasedLhsColumns,
			string alias, string path, int currentDepth, JoinType joinType)
		{
			if (joinType >= JoinType.InnerJoin)
			{
				AddAssociationToJoinTree(type, aliasedLhsColumns, alias, path, currentDepth, joinType);
			}
		}

		/// <summary>
		/// Add on association (one-to-one, many-to-one, or a collection) to a list
		/// of associations to be fetched by outerjoin
		/// </summary>
		private void AddAssociationToJoinTree(IAssociationType type, string[] aliasedLhsColumns, string alias,
			string path, int currentDepth, JoinType joinType)
		{
			IJoinable joinable = type.GetAssociatedJoinable(Factory);

			string subalias = GenerateTableAlias(associations.Count + 1, path, joinable);

			OuterJoinableAssociation assoc =
				new OuterJoinableAssociation(type, alias, aliasedLhsColumns, subalias, joinType, Factory, enabledFilters);
			assoc.ValidateJoin(path);
			associations.Add(assoc);

			int nextDepth = currentDepth + 1;

			if (!joinable.IsCollection)
			{
				IOuterJoinLoadable pjl = joinable as IOuterJoinLoadable;
				if (pjl != null)
					WalkEntityTree(pjl, subalias, path, nextDepth);
			}
			else
			{
				IQueryableCollection qc = joinable as IQueryableCollection;
				if (qc != null)
					WalkCollectionTree(qc, subalias, path, nextDepth);
			}
		}

		/// <summary>
		/// For an entity class, return a list of associations to be fetched by outerjoin
		/// </summary>
		protected void WalkEntityTree(IOuterJoinLoadable persister, string alias)
		{
			WalkEntityTree(persister, alias, string.Empty, 0);
		}

		/// <summary>
		/// For a collection role, return a list of associations to be fetched by outerjoin
		/// </summary>
		protected void WalkCollectionTree(IQueryableCollection persister, string alias)
		{
			WalkCollectionTree(persister, alias, string.Empty, 0);
			//TODO: when this is the entry point, we should use an INNER_JOIN for fetching the many-to-many elements!
		}

		/// <summary>
		/// For a collection role, return a list of associations to be fetched by outerjoin
		/// </summary>
		private void WalkCollectionTree(IQueryableCollection persister, string alias, string path, int currentDepth)
		{
			if (persister.IsOneToMany)
			{
				WalkEntityTree((IOuterJoinLoadable)persister.ElementPersister, alias, path, currentDepth);
			}
			else
			{
				IType type = persister.ElementType;
				if (type.IsAssociationType)
				{
					// a many-to-many
					// decrement currentDepth here to allow join across the association table
					// without exceeding MAX_FETCH_DEPTH (i.e. the "currentDepth - 1" bit)
					IAssociationType associationType = (IAssociationType)type;
					string[] aliasedLhsColumns = persister.GetElementColumnNames(alias);
					string[] lhsColumns = persister.ElementColumnNames;

					// if the current depth is 0, the root thing being loaded is the
					// many-to-many collection itself.  Here, it is alright to use
					// an inner join...
					bool useInnerJoin = currentDepth == 0;

					JoinType joinType =
						GetJoinType(associationType, persister.FetchMode, path, persister.TableName, lhsColumns, !useInnerJoin,
												currentDepth - 1, null);

					AddAssociationToJoinTreeIfNecessary(associationType, aliasedLhsColumns, alias, path, currentDepth - 1, joinType);
				}
				else if (type.IsComponentType)
				{
					WalkCompositeElementTree((IAbstractComponentType)type, persister.ElementColumnNames, persister, alias, path,
																	 currentDepth);
				}
			}
		}

		private void WalkEntityAssociationTree(IAssociationType associationType, IOuterJoinLoadable persister, int propertyNumber,
			string alias, string path, bool nullable, int currentDepth)
		{
			string[] aliasedLhsColumns =
				JoinHelper.GetAliasedLHSColumnNames(associationType, alias, propertyNumber, persister, Factory);

			string[] lhsColumns = JoinHelper.GetLHSColumnNames(associationType, propertyNumber, persister, Factory);
			string lhsTable = JoinHelper.GetLHSTableName(associationType, propertyNumber, persister);

			string subpath = SubPath(path, persister.GetSubclassPropertyName(propertyNumber));

			JoinType joinType =
				GetJoinType(associationType, persister.GetFetchMode(propertyNumber), subpath, lhsTable, lhsColumns, nullable,
										currentDepth, persister.GetCascadeStyle(propertyNumber));

			AddAssociationToJoinTreeIfNecessary(associationType, aliasedLhsColumns, alias, subpath, currentDepth, joinType);
		}

		/// <summary>
		/// For an entity class, add to a list of associations to be fetched
		/// by outerjoin
		/// </summary>
		private void WalkEntityTree(IOuterJoinLoadable persister, string alias, string path, int currentDepth)
		{
			int n = persister.CountSubclassProperties();
			for (int i = 0; i < n; i++)
			{
				IType type = persister.GetSubclassPropertyType(i);
				if (type.IsAssociationType)
				{
					WalkEntityAssociationTree((IAssociationType)type, persister, i, alias, path,
																		persister.IsSubclassPropertyNullable(i), currentDepth);
				}
				else if (type.IsComponentType)
				{
					WalkComponentTree((IAbstractComponentType)type, i, 0, persister, alias,
														SubPath(path, persister.GetSubclassPropertyName(i)), currentDepth);
				}
			}
		}

		/// <summary>
		/// For a component, add to a list of associations to be fetched by outerjoin
		/// </summary>
		private void WalkComponentTree(IAbstractComponentType componentType, int propertyNumber, int begin,
			IOuterJoinLoadable persister, string alias, string path, int currentDepth)
		{
			IType[] types = componentType.Subtypes;
			string[] propertyNames = componentType.PropertyNames;
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i].IsAssociationType)
				{
					IAssociationType associationType = (IAssociationType)types[i];
					string[] aliasedLhsColumns =
						JoinHelper.GetAliasedLHSColumnNames(associationType, alias, propertyNumber, begin, persister, Factory);

					string[] lhsColumns = JoinHelper.GetLHSColumnNames(associationType, propertyNumber, begin, persister, Factory);
					string lhsTable = JoinHelper.GetLHSTableName(associationType, propertyNumber, persister);

					string subpath = SubPath(path, propertyNames[i]);
					bool[] propertyNullability = componentType.PropertyNullability;

					JoinType joinType =
						GetJoinType(associationType, componentType.GetFetchMode(i), subpath, lhsTable, lhsColumns,
												propertyNullability == null || propertyNullability[i], currentDepth, componentType.GetCascadeStyle(i));

					AddAssociationToJoinTreeIfNecessary(associationType, aliasedLhsColumns, alias, subpath, currentDepth, joinType);
				}
				else if (types[i].IsComponentType)
				{
					string subpath = SubPath(path, propertyNames[i]);

					WalkComponentTree((IAbstractComponentType)types[i], propertyNumber, begin, persister, alias, subpath, currentDepth);
				}
				begin += types[i].GetColumnSpan(Factory);
			}
		}

		/// <summary>
		/// For a composite element, add to a list of associations to be fetched by outerjoin
		/// </summary>
		private void WalkCompositeElementTree(IAbstractComponentType compositeType, string[] cols,
			IQueryableCollection persister, string alias, string path, int currentDepth)
		{
			IType[] types = compositeType.Subtypes;
			string[] propertyNames = compositeType.PropertyNames;
			int begin = 0;
			for (int i = 0; i < types.Length; i++)
			{
				int length = types[i].GetColumnSpan(factory);
				string[] lhsColumns = ArrayHelper.Slice(cols, begin, length);

				if (types[i].IsAssociationType)
				{
					IAssociationType associationType = types[i] as IAssociationType;

					// simple, because we can't have a one-to-one or collection
					// (or even a property-ref) in a composite element:
					string[] aliasedLhsColumns = StringHelper.Qualify(alias, lhsColumns);
					string subpath = SubPath(path, propertyNames[i]);
					bool[] propertyNullability = compositeType.PropertyNullability;

					JoinType joinType =
						GetJoinType(associationType, compositeType.GetFetchMode(i), subpath, persister.TableName, lhsColumns,
												propertyNullability == null || propertyNullability[i], currentDepth, compositeType.GetCascadeStyle(i));

					AddAssociationToJoinTreeIfNecessary(associationType, aliasedLhsColumns, alias, subpath, currentDepth, joinType);
				}
				else if (types[i].IsComponentType)
				{
					string subpath = SubPath(path, propertyNames[i]);
					WalkCompositeElementTree((IAbstractComponentType)types[i], lhsColumns, persister, alias, subpath, currentDepth);
				}
				begin += length;
			}
		}

		/// <summary>
		/// Extend the path by the given property name
		/// </summary>
		private static string SubPath(string path, string property)
		{
			if (path == null || path.Length == 0)
				return property;
			else
				return StringHelper.Qualify(path, property);
		}

		/// <summary>
		/// Get the join type (inner, outer, etc) or -1 if the
		/// association should not be joined. Override on
		/// subclasses.
		/// </summary>
		protected virtual JoinType GetJoinType(IAssociationType type, FetchMode config, string path, string lhsTable,
			string[] lhsColumns, bool nullable, int currentDepth, CascadeStyle cascadeStyle)
		{
			if (!IsJoinedFetchEnabled(type, config, cascadeStyle))
				return JoinType.None;

			if (IsTooDeep(currentDepth) || (type.IsCollectionType && IsTooManyCollections))
				return JoinType.None;

			bool dupe = IsDuplicateAssociation(lhsTable, lhsColumns, type);
			if (dupe)
				return JoinType.None;

			return GetJoinType(nullable, currentDepth);
		}

		/// <summary>
		/// Use an inner join if it is a non-null association and this
		/// is the "first" join in a series
		/// </summary>
		protected JoinType GetJoinType(bool nullable, int currentDepth)
		{
			//TODO: this is too conservative; if all preceding joins were 
			//      also inner joins, we could use an inner join here
			return !nullable && currentDepth == 0 ? JoinType.InnerJoin : JoinType.LeftOuterJoin;
		}

		protected virtual bool IsTooDeep(int currentDepth)
		{
			int maxFetchDepth = Factory.Settings.MaximumFetchDepth;
			return maxFetchDepth >= 0 && currentDepth >= maxFetchDepth;
		}

		/// <summary>
		/// Does the mapping, and Hibernate default semantics, specify that
		/// this association should be fetched by outer joining
		/// </summary>
		protected bool IsJoinedFetchEnabledInMapping(FetchMode config, IAssociationType type)
		{
			if (!type.IsEntityType && !type.IsCollectionType)
			{
				return false;
			}
			else
			{
				switch (config)
				{
					case FetchMode.Join:
						return true;

					case FetchMode.Select:
						return false;

					case FetchMode.Default:
						if (type.IsEntityType)
						{
							//TODO: look at the owning property and check that it 
							//      isn't lazy (by instrumentation)
							EntityType entityType = (EntityType)type;
							IEntityPersister persister = factory.GetEntityPersister(entityType.GetAssociatedEntityName());
							return !persister.HasProxy;
						}
						else
						{
							return false;
						}

					default:
						throw new ArgumentOutOfRangeException("config", config, "Unknown OJ strategy " + config);
				}
			}
		}

		/// <summary>
		/// Override on subclasses to enable or suppress joining
		/// of certain association types
		/// </summary>
		protected virtual bool IsJoinedFetchEnabled(IAssociationType type, FetchMode config,
																								CascadeStyle cascadeStyle)
		{
			return type.IsEntityType && IsJoinedFetchEnabledInMapping(config, type);
		}

		protected virtual string GenerateTableAlias(int n, string path, IJoinable joinable)
		{
			return StringHelper.GenerateAlias(joinable.Name, n);
		}

		protected virtual string GenerateRootAlias(string description)
		{
			return StringHelper.GenerateAlias(description, 0);
		}

		/// <summary>
		/// Used to detect circularities in the joined graph, note that
		/// this method is side-effecty
		/// </summary>
		protected virtual bool IsDuplicateAssociation(string foreignKeyTable, string[] foreignKeyColumns)
		{
			AssociationKey associationKey = new AssociationKey(foreignKeyColumns, foreignKeyTable);
			return !visitedAssociationKeys.Add(associationKey);
		}

		/// <summary>
		/// Used to detect circularities in the joined graph, note that
		/// this method is side-effecty
		/// </summary>
		protected virtual bool IsDuplicateAssociation(string lhsTable, string[] lhsColumnNames, IAssociationType type)
		{
			string foreignKeyTable;
			string[] foreignKeyColumns;

			if (type.ForeignKeyDirection == ForeignKeyDirection.ForeignKeyFromParent)
			{
				foreignKeyTable = lhsTable;
				foreignKeyColumns = lhsColumnNames;
			}
			else
			{
				foreignKeyTable = type.GetAssociatedJoinable(Factory).TableName;
				foreignKeyColumns = JoinHelper.GetRHSColumnNames(type, Factory);
			}

			return IsDuplicateAssociation(foreignKeyTable, foreignKeyColumns);
		}

		/// <summary>
		/// Uniquely identifier a foreign key, so that we don't
		/// join it more than once, and create circularities
		/// </summary>
		protected sealed class AssociationKey
		{
			private readonly string[] columns;
			private readonly string table;
			private readonly int hashCode;

			public AssociationKey(string[] columns, string table)
			{
				this.columns = columns;
				this.table = table;
				hashCode = table.GetHashCode();
			}

			public override bool Equals(object other)
			{
				AssociationKey that = other as AssociationKey;
				if (that == null)
					return false;

				return that.table.Equals(table) && CollectionHelper.CollectionEquals<string>(columns, that.columns);
			}

			public override int GetHashCode()
			{
				return hashCode;
			}
		}

		/// <summary>
		/// Should we join this association?
		/// </summary>
		protected bool IsJoinable(JoinType joinType, ISet<AssociationKey> visitedAssociationKeys, string lhsTable,
			string[] lhsColumnNames, IAssociationType type, int depth)
		{
			if (joinType < JoinType.InnerJoin) return false;
			if (joinType == JoinType.InnerJoin) return true;

			int maxFetchDepth = Factory.Settings.MaximumFetchDepth;
			bool tooDeep = maxFetchDepth >= 0 && depth >= maxFetchDepth;

			return !tooDeep && !IsDuplicateAssociation(lhsTable, lhsColumnNames, type);
		}

		protected SqlString OrderBy(IList<OuterJoinableAssociation> associations, SqlString orderBy)
		{
			return MergeOrderings(OrderBy(associations), orderBy);
		}

		protected SqlString OrderBy(IList<OuterJoinableAssociation> associations, string orderBy)
		{
			return MergeOrderings(OrderBy(associations), new SqlString(orderBy));
		}

		protected SqlString MergeOrderings(SqlString ass, SqlString orderBy)
		{
			if (ass.Length == 0)
				return orderBy;
			else if (orderBy.Length == 0)
				return ass;
			else
				return ass.Append(StringHelper.CommaSpace).Append(orderBy);
		}

		protected SqlString MergeOrderings(string ass, SqlString orderBy) {
			return this.MergeOrderings(new SqlString(ass), orderBy);
		}

		protected SqlString MergeOrderings(string ass, string orderBy) {
			return this.MergeOrderings(new SqlString(ass), new SqlString(orderBy));
		}

		/// <summary>
		/// Generate a sequence of <c>LEFT OUTER JOIN</c> clauses for the given associations.
		/// </summary>
		protected JoinFragment MergeOuterJoins(IList<OuterJoinableAssociation> associations)
		{
			JoinFragment outerjoin = Dialect.CreateOuterJoinFragment();

			OuterJoinableAssociation last = null;
			foreach (OuterJoinableAssociation oj in associations)
			{
				if (last != null && last.IsManyToManyWith(oj))
					oj.AddManyToManyJoin(outerjoin, (IQueryableCollection)last.Joinable);
				else
					oj.AddJoins(outerjoin);

				last = oj;
			}

			return outerjoin;
		}

		/// <summary>
		/// Count the number of instances of IJoinable which are actually
		/// also instances of ILoadable, or are one-to-many associations
		/// </summary>
		protected static int CountEntityPersisters(IList<OuterJoinableAssociation> associations)
		{
			int result = 0;
			foreach (OuterJoinableAssociation oj in associations)
			{
				if (oj.Joinable.ConsumesEntityAlias())
					result++;
			}

			return result;
		}

		/// <summary>
		/// Count the number of instances of <see cref="IJoinable" /> which
		/// are actually also instances of <see cref="IPersistentCollection" />
		/// which are being fetched by outer join
		/// </summary>
		protected static int CountCollectionPersisters(IList<OuterJoinableAssociation> associations)
		{
			int result = 0;

			foreach (OuterJoinableAssociation oj in associations)
			{
				if (oj.JoinType == JoinType.LeftOuterJoin && oj.Joinable.IsCollection)
					result++;
			}
			return result;
		}

		/// <summary>
		/// Get the order by string required for collection fetching
		/// </summary>
		protected SqlString OrderBy(IList<OuterJoinableAssociation> associations)
		{
			SqlStringBuilder buf = new SqlStringBuilder();

			OuterJoinableAssociation last = null;
			foreach (OuterJoinableAssociation oj in associations)
			{
				if (oj.JoinType == JoinType.LeftOuterJoin)
				{
					if (oj.Joinable.IsCollection)
					{
						IQueryableCollection queryableCollection = (IQueryableCollection)oj.Joinable;
						if (queryableCollection.HasOrdering)
						{
							string orderByString = queryableCollection.GetSQLOrderByString(oj.RHSAlias);
							buf.Add(orderByString).Add(StringHelper.CommaSpace);
						}
					}
					else
					{
						// it might still need to apply a collection ordering based on a
						// many-to-many defined order-by...
						if (last != null && last.Joinable.IsCollection)
						{
							IQueryableCollection queryableCollection = (IQueryableCollection)last.Joinable;
							if (queryableCollection.IsManyToMany && last.IsManyToManyWith(oj))
							{
								if (queryableCollection.HasManyToManyOrdering)
								{
									string orderByString = queryableCollection.GetManyToManyOrderByString(oj.RHSAlias);
									buf.Add(orderByString).Add(StringHelper.CommaSpace);
								}
							}
						}
					}
				}
				last = oj;
			}

			if (buf.Count > 0) {
				buf.RemoveAt(buf.Count-1);
			}

			return buf.ToSqlString();
		}

		/// <summary>
		/// Render the where condition for a (batch) load by identifier / collection key
		/// </summary>
		protected SqlStringBuilder WhereString(string alias, string[] columnNames, int batchSize)
		{
			if (columnNames.Length == 1)
			{
				// if not a composite key, use "foo in (?, ?, ?)" for batching
				// if no batch, and not a composite key, use "foo = ?"
				InFragment inf = new InFragment().SetColumn(alias, columnNames[0]);

				for (int i = 0; i < batchSize; i++)
					inf.AddValue(Parameter.Placeholder);

				return new SqlStringBuilder(inf.ToFragmentString());
			}
			else
			{
				Parameter[] columnParameters = Parameter.GenerateParameters(columnNames.Length);
				ConditionalFragment byId = new ConditionalFragment()
					.SetTableAlias(alias)
					.SetCondition(columnNames, columnParameters);

				SqlStringBuilder whereString = new SqlStringBuilder();

				if (batchSize == 1)
				{
					// if no batch, use "foo = ? and bar = ?"
					whereString.Add(byId.ToSqlStringFragment());
				}
				else
				{
					// if a composite key, use "( (foo = ? and bar = ?) or (foo = ? and bar = ?) )" for batching
					whereString.Add(StringHelper.OpenParen); // TODO: unnecessary for databases with ANSI-style joins
					DisjunctionFragment df = new DisjunctionFragment();
					for (int i = 0; i < batchSize; i++)
					{
						df.AddCondition(byId);
					}
					whereString.Add(df.ToFragmentString());
					whereString.Add(StringHelper.ClosedParen); // TODO: unnecessary for databases with ANSI-style joins
				}

				return whereString;
			}
		}

		protected void InitPersisters(IList<OuterJoinableAssociation> associations, LockMode lockMode)
		{
			int joins = CountEntityPersisters(associations);
			int collections = CountCollectionPersisters(associations);

			this.CollectionOwners = collections == 0 ? null : new int[collections];
			this.CollectionPersisters = collections == 0 ? null : new ICollectionPersister[collections];
			this.CollectionSuffixes = BasicLoader.GenerateSuffixes(joins + 1, collections);

			this.Persisters = new ILoadable[joins];
			this.Aliases = new String[joins];
			this.Owners = new int[joins];
			this.OwnerAssociationTypes = new EntityType[joins];
			this.LockModeArray = ArrayHelper.FillArray(lockMode, joins);

			int i = 0;
			int j = 0;

			foreach (OuterJoinableAssociation oj in associations)
			{
				if (!oj.IsCollection)
				{
					this.Persisters[i] = (ILoadable)oj.Joinable;
					this.Aliases[i] = oj.RHSAlias;
					this.Owners[i] = oj.GetOwner(associations);
					this.OwnerAssociationTypes[i] = (EntityType)oj.JoinableType;
					i++;
				}
				else
				{
					IQueryableCollection collPersister = (IQueryableCollection)oj.Joinable;

					if (oj.JoinType == JoinType.LeftOuterJoin)
					{
						//it must be a collection fetch
						this.CollectionPersisters[j] = collPersister;
						this.CollectionOwners[j] = oj.GetOwner(associations);
						j++;
					}

					if (collPersister.IsOneToMany)
					{
						this.Persisters[i] = (ILoadable)collPersister.ElementPersister;
						this.Aliases[i] = oj.RHSAlias;
						i++;
					}
				}
			}

			if (ArrayHelper.IsAllNegative(this.Owners))
				this.Owners = null;

			if (this.CollectionOwners != null && ArrayHelper.IsAllNegative(this.CollectionOwners))
				this.CollectionOwners = null;
		}

		/// <summary>
		/// Generate a select list of columns containing all properties of the entity classes
		/// </summary>
		public string SelectString(IList<OuterJoinableAssociation> associations)
		{
			if (associations.Count == 0)
			{
				return string.Empty;
			}
			else
			{
				SqlStringBuilder buf = new SqlStringBuilder(associations.Count * 3);
				buf.Add(StringHelper.CommaSpace);

				int entityAliasCount = 0;
				int collectionAliasCount = 0;

				for (int i = 0; i < associations.Count; i++)
				{
					OuterJoinableAssociation join = associations[i];
					OuterJoinableAssociation next = (i == associations.Count - 1) ? null : associations[i + 1];

					IJoinable joinable = join.Joinable;
					string entitySuffix = (this.Suffixes == null || entityAliasCount >= this.Suffixes.Length) ? null : this.Suffixes[entityAliasCount];

					string collectionSuffix = (this.CollectionSuffixes == null || collectionAliasCount >= this.CollectionSuffixes.Length)
																			? null
																			: this.CollectionSuffixes[collectionAliasCount];

					string selectFragment =
						joinable.SelectFragment(next == null ? null : next.Joinable, next == null ? null : next.RHSAlias, join.RHSAlias,
																		entitySuffix, collectionSuffix, join.JoinType == JoinType.LeftOuterJoin);

					buf.Add(selectFragment);

					if (joinable.ConsumesEntityAlias())
						entityAliasCount++;

					if (joinable.ConsumesCollectionAlias() && join.JoinType == JoinType.LeftOuterJoin)
						collectionAliasCount++;

					if (i < associations.Count - 1 && selectFragment.Trim().Length > 0)
						buf.Add(StringHelper.CommaSpace);
				}

				return buf.ToSqlString().ToString();
			}
		}
	}
}
