using System;
using System.Collections;
using System.Text;
using NHibernate.Collection;
using NHibernate.Persister;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Hql
{
	/// <summary> 
	/// Parses an expression of the form foo.bar.baz and builds up an expression
	/// involving two less table joins than there are path components.
	/// </summary>
	public class PathExpressionParser : IParser
	{
		//TODO: this class does too many things! we need a different 
		//kind of path expression parser for each of the different 
		//ways in which path expressions can occur 

		//We should actually rework this class to not implement Parser 
		//and just process path expressions in the most convenient way.

		//The class is now way to complex!

		/// <summary></summary>
		public const string EntityID = "id";
		/// <summary></summary>
		public const string EntityClass = "class";
		/// <summary></summary>
		public const string CollectionSize = "size";
		/// <summary></summary>
		public const string CollectionElements = "elements";
		/// <summary></summary>
		public const string CollectionIndices = "indices";
		/// <summary></summary>
		public const string CollectionMaxIndex = "maxIndex";
		/// <summary></summary>
		public const string CollectionMinIndex = "minIndex";
		/// <summary></summary>
		public const string CollectionMaxElement = "maxElement";
		/// <summary></summary>
		public const string CollectionMinElement = "minElement";

		private int dotcount;

		/// <summary></summary>
		protected string currentName;

		/// <summary></summary>
		protected string currentProperty;

		/// <summary></summary>
		protected QueryJoinFragment join;

		/// <summary></summary>
		protected string[ ] columns;

		/// <summary></summary>
		protected string[ ] collectionElementColumns;

		private string collectionName; //protected
		private string collectionOwnerName;
		private string collectionRole;
		private string collectionTable;

		/// <summary></summary>
		protected IType collectionElementType;

		private string componentPath;

		/// <summary></summary>
		protected IType type;

		private string path;
		private bool ignoreInitialJoin;
		private bool continuation;
		private JoinType joinType = JoinType.InnerJoin; //default mode
		private bool useThetaStyleJoin = true;

		/// <summary></summary>
		public JoinType JoinType
		{
			get { return joinType; }
			set { joinType = value; }
		}

		/// <summary></summary>
		public bool UseThetaStyleJoin
		{
			get { return useThetaStyleJoin; }
			set { useThetaStyleJoin = value; }
		}

		private void AddJoin( string table, string name, string[ ] rhsCols, QueryTranslator q )
		{
			string[ ] lhsCols = CurrentColumns( q );
			join.AddJoin( table, name, lhsCols, rhsCols, joinType );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clazz"></param>
		/// <param name="joinColumns"></param>
		/// <param name="q"></param>
		/// <returns></returns>
		public string ContinueFromManyToMany( System.Type clazz, string[ ] joinColumns, QueryTranslator q )
		{
			Start( q );
			continuation = true;
			currentName = q.CreateNameFor( clazz );
			q.AddType( currentName, clazz );
			ILoadable p = q.GetPersister( clazz );
			join.AddJoin( p.TableName, currentName, joinColumns, p.IdentifierColumnNames, joinType );
			return currentName;
		}

		/// <summary></summary>
		public void IgnoreInitialJoin()
		{
			ignoreInitialJoin = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="q"></param>
		public void Token( string token, QueryTranslator q )
		{
			if( token != null )
			{
				path += token;
			}

			string alias = q.GetPathAlias( path );
			if( alias != null )
			{
				Reset( q ); //reset the dotcount (but not the path)
				currentName = alias; //after reset!
				if( !ignoreInitialJoin )
				{
					JoinFragment ojf = q.GetPathJoin( path );
					join.AddCondition( ojf.ToWhereFragmentString ); //after reset!
					// we don't need to worry about any condition in the ON clause
					// here (toFromFragmentString), since anything in the ON condition 
					// is already applied to the whole query
				}
			}
			else if( ".".Equals( token ) )
			{
				dotcount++;
			}
			else
			{
				if( dotcount == 0 )
				{
					if( !continuation )
					{
						if( !q.IsName( token ) )
						{
							throw new QueryException( "undefined alias or unknown mapping: " + token );
						}
						currentName = token;
					}
				}
				else if( dotcount == 1 )
				{
					if( currentName != null )
					{
						currentProperty = token;
					}
					else if( collectionName != null )
					{
						CollectionPersister p = q.GetCollectionPersister( collectionRole );
						DoCollectionProperty( token, p, collectionName );
						continuation = false;
					}
					else
					{
						throw new QueryException( "unexpected" );
					}
				}
				else
				{ // dotcount>=2

					// Do the corresponding RHS
					IType propertyType = GetPropertyType( q );

					if( propertyType == null )
					{
						throw new QueryException( "unresolved property: " + currentProperty );
					}

					if( propertyType.IsComponentType || propertyType.IsObjectType )
					{
						if( componentPath == null )
						{
							componentPath = token;
						}
						else
						{
							if( token != null )
							{
								componentPath += StringHelper.Dot + token;
							}
						}
					}
					else
					{
						if( propertyType.IsEntityType )
						{
							System.Type memberClass = ( ( EntityType ) propertyType ).PersistentClass;
							IQueryable memberPersister = q.GetPersister( memberClass );
							if(
								// if its "id"
								EntityID.Equals( token ) || (
									//or its the id property name
									memberPersister.HasIdentifierProperty &&
										memberPersister.IdentifierPropertyName.Equals( token ) ) )
							{
								// special shortcut for id properties, skip the join!
								// this must only occur at the _end_ of a path expression
								if( componentPath == null )
								{
									componentPath = "id";
								}
								else
								{
									componentPath += ".id";
								}
							}
							else
							{
								string name = q.CreateNameFor( memberClass );
								q.AddType( name, memberClass );
								string[ ] keyColNames = memberPersister.IdentifierColumnNames;
								AddJoin( memberPersister.TableName, name, keyColNames, q );
								currentName = name;
								currentProperty = token;
								q.AddPathAliasAndJoin( path.Substring( 0, ( path.LastIndexOf( StringHelper.Dot ) ) - ( 0 ) ), name, join );
								componentPath = null;
							}
						}
						else if( propertyType.IsPersistentCollectionType )
						{
							collectionRole = ( ( PersistentCollectionType ) propertyType ).Role;
							CollectionPersister collPersister = q.GetCollectionPersister( collectionRole );
							string[ ] colNames = collPersister.KeyColumnNames;

							string name = q.CreateNameForCollection( collectionRole );
							string tableName = collPersister.QualifiedTableName;
							AddJoin( tableName, name, colNames, q );
							if( collPersister.HasWhere )
							{
								join.AddCondition( collPersister.GetSQLWhereString( name ) );
							}
							DoCollectionProperty( token, collPersister, name );
							collectionName = name;
							collectionOwnerName = currentName;
							collectionTable = collPersister.QualifiedTableName;
							currentName = null;
							currentProperty = null;
							componentPath = null;
						}
						else
						{
							if( token != null )
							{
								throw new QueryException( "dereferenced: " + currentProperty );
							}
						}
					}
				}
			}
		}

		private string PropertyPath
		{
			get
			{
				if( currentProperty == null )
				{
					return EntityID;
				}
				else
				{
					return currentProperty +
						( ( componentPath == null ) ? String.Empty : StringHelper.Dot + componentPath );
				}
			}
		}

		private void SetType( QueryTranslator q )
		{
			if( currentProperty == null )
			{
				IClassPersister p = q.GetPersisterForName( currentName );
				type = NHibernate.Entity( p.MappedClass );
			}
			else
			{
				type = GetPropertyType( q );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		protected IType GetPropertyType( QueryTranslator q )
		{
			string path = PropertyPath;
			IType type = q.GetPersisterForName( currentName ).GetPropertyType( path );

			if( type == null )
			{
				throw new QueryException( "could not resolve property type: " + path );
			}

			return type;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		protected string[ ] CurrentColumns( QueryTranslator q )
		{
			string path = PropertyPath;
			string[ ] columns = q.GetPersisterForName( currentName ).ToColumns( currentName, path );
			if( columns == null )
			{
				throw new QueryException( "could not resolve property columns: " + path );
			}
			return columns;
		}

		private void Reset( QueryTranslator q )
		{
			join = q.CreateJoinFragment( useThetaStyleJoin );
			dotcount = 0;
			currentName = null;
			currentProperty = null;
			collectionName = null;
			collectionRole = null;
			collectionTable = null;
			collectionElementColumns = null;
			collectionElementType = null;
			componentPath = null;
			type = null;
			collectionName = null;
			columns = null;
			expectingCollectionIndex = false;
			continuation = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		public void Start( QueryTranslator q )
		{
			if( !continuation )
			{
				Reset( q );
				path = String.Empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		public virtual void End( QueryTranslator q )
		{
			ignoreInitialJoin = false;
			if( IsCollectionValued )
			{
				columns = collectionElementColumns;
				type = collectionElementType;
			}
			else
			{
				if( !continuation )
				{
					IType propertyType = GetPropertyType( q );
					if( propertyType != null && propertyType.IsPersistentCollectionType )
					{
						collectionRole = ( ( PersistentCollectionType ) propertyType ).Role;
						collectionName = q.CreateNameForCollection( collectionRole );
					}
				}
				if( collectionRole != null )
				{
					//special case; expecting: [index]
					CollectionPersister memberPersister = q.GetCollectionPersister( collectionRole );

					if( !memberPersister.HasIndex )
					{
						throw new QueryException( "unindexed collection before []" );
					}
					string[ ] indexCols = memberPersister.IndexColumnNames;
					if( indexCols.Length != 1 )
					{
						throw new QueryException( "composite-index appears in []: " + path );
					}
					string[ ] keyCols = memberPersister.KeyColumnNames;

					JoinFragment ojf = q.CreateJoinFragment( useThetaStyleJoin );
					ojf.AddCrossJoin( memberPersister.QualifiedTableName, collectionName );
					if( memberPersister.IsOneToMany )
					{
						IQueryable persister = q.GetPersister( ( ( EntityType ) memberPersister.ElementType ).PersistentClass );
						ojf.AddJoins(
							persister.FromJoinFragment( collectionName, true, false ),
							persister.WhereJoinFragment( collectionName, true, false )
							);
					}

					if( !continuation )
					{
						AddJoin( memberPersister.QualifiedTableName, collectionName, keyCols, q );
					}
					join.AddCondition( collectionName, indexCols, " = " );

					string[ ] eltCols = memberPersister.ElementColumnNames;
					//if ( eltCols.Length!=1 ) throw new QueryException("composite-id collection element []");

					CollectionElement elem = new CollectionElement();
					elem.ElementColumns = StringHelper.Prefix( eltCols, collectionName + StringHelper.Dot );
					elem.Type = memberPersister.ElementType;
					elem.IsOneToMany = memberPersister.IsOneToMany;
					elem.Alias = collectionName;
					elem.Join = join;
					collectionElements.Add( elem ); //addlast
					SetExpectingCollectionIndex();

					q.AddCollection( collectionName, collectionRole );
					q.AddJoin( collectionName, ojf );
				}
				else
				{
					columns = CurrentColumns( q );
					SetType( q );
				}

			}

			//important!!
			continuation = false;

		}

		/// <summary></summary>
		public sealed class CollectionElement // struct?
		{
			/// <summary></summary>
			public IType Type;

			/// <summary></summary>
			public bool IsOneToMany;

			/// <summary></summary>
			public string Alias;

			/// <summary></summary>
			public string[ ] ElementColumns;

			/// <summary></summary>
			public JoinFragment Join;

			/// <summary></summary>
			public StringBuilder IndexValue = new StringBuilder();
		}

		private bool expectingCollectionIndex;
		private ArrayList collectionElements = new ArrayList();

		/// <summary></summary>
		public CollectionElement LastCollectionElement()
		{
			CollectionElement ce = ( CollectionElement ) collectionElements[ collectionElements.Count - 1 ];
			collectionElements.RemoveAt( collectionElements.Count - 1 );
			return ce; //remove last
		}

		/// <summary></summary>
		public string LastCollectionElementIndexValue
		{
			set
			{
				( ( CollectionElement ) collectionElements[ collectionElements.Count - 1 ] ).IndexValue.Append( value ); //getlast
			}
		}

		/// <summary></summary>
		public bool IsExpectingCollectionIndex
		{
			get { return expectingCollectionIndex; }
			set { expectingCollectionIndex = value; }
		}

		/// <summary></summary>
		protected virtual void SetExpectingCollectionIndex()
		{
			expectingCollectionIndex = true;
		}

		/// <summary></summary>
		public JoinFragment WhereJoin
		{
			get { return join; }
		}

		/// <summary></summary>
		public string WhereColumn
		{
			get
			{
				if( columns.Length != 1 )
				{
					throw new QueryException( "path expression ends in a composite value" );
				}
				return columns[ 0 ];
			}
		}

		/// <summary></summary>
		public string[ ] WhereColumns
		{
			get { return columns; }
		}

		/// <summary></summary>
		public IType WhereColumnType
		{
			get { return type; }
		}

		/// <summary></summary>
		public string Name
		{
			get { return currentName == null ? collectionName : currentName; }
		}

		/// <summary></summary>
		public string GetCollectionSubquery()
		{
			//TODO: refactor to .sql package
			return new StringBuilder( "SELECT " )
				.Append( String.Join( ", ", collectionElementColumns ) )
				.Append( " FROM " )
				/*.Append(collectionTable)
				.Append(' ')
				.Append(collectionName)*/
				.Append( join.ToFromFragmentString.Substring( 2 ) ) //remove initial ", "
				.Append( " WHERE " )
				.Append( join.ToWhereFragmentString.Substring( 5 ) )
				.ToString();
		}

		/// <summary></summary>
		public bool IsCollectionValued
		{
			get { return collectionElementColumns != null; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		public void AddAssociation( QueryTranslator q )
		{
			q.AddJoin( Name, join );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		public string AddFromAssociation( QueryTranslator q )
		{
			q.AddFrom( currentName, join );
			return currentName;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		public string AddFromCollection( QueryTranslator q )
		{
			if( collectionElementType == null )
			{
				throw new QueryException(
					"must specify 'elements' for collection valued property in from clause: " + path
					);
			}
			if( !collectionElementType.IsEntityType )
			{
				throw new QueryException(
					"collection of values in from clause: " + path
					);
			}
			EntityType elemType = ( EntityType ) collectionElementType;
			System.Type clazz = elemType.PersistentClass;
			CollectionPersister persister = q.GetCollectionPersister( collectionRole );

			string elementName;
			if( persister.IsOneToMany )
			{
				elementName = collectionName;
			}
			else
			{
				q.AddCollection( collectionName, collectionRole );
				IQueryable p = q.GetPersister( clazz );
				elementName = q.CreateNameFor( clazz );
				string[ ] keyColumnNames = p.IdentifierColumnNames;
				join.AddJoin( p.TableName, elementName, collectionElementColumns, keyColumnNames, joinType );
			}
			q.AddFrom( elementName, clazz, join );

			return elementName;
		}

		/// <summary></summary>
		public string CollectionName
		{
			get { return collectionName; }
		}

		/// <summary></summary>
		public string CollectionRole
		{
			get { return collectionRole; }
		}

		/// <summary></summary>
		public string CollectionTable
		{
			get { return collectionTable; }
		}

		private void DoCollectionProperty( string token, CollectionPersister memberPersister, string name )
		{
			if( token.Equals( CollectionElements ) )
			{
				string[ ] cols = memberPersister.ElementColumnNames;
				collectionElementColumns = StringHelper.Prefix( cols, name + StringHelper.Dot );
				collectionElementType = memberPersister.ElementType;
			}
			else if( token.Equals( CollectionIndices ) )
			{
				if( !memberPersister.HasIndex )
				{
					throw new QueryException( "unindexed collection before .indices" );
				}
				string[ ] cols = memberPersister.IndexColumnNames;
				collectionElementColumns = StringHelper.Prefix( cols, name + StringHelper.Dot );
				collectionElementType = memberPersister.IndexType;
			}
			else if( token.Equals( CollectionSize ) )
			{
				collectionElementColumns = new string[ ] {"count(*)"};
				collectionElementType = NHibernate.Int32;
			}
			else if( token.Equals( CollectionMaxIndex ) )
			{
				if( !memberPersister.HasIndex )
				{
					throw new QueryException( "unindexed collection before .maxIndex" );
				}
				string[ ] cols = memberPersister.IndexColumnNames;
				if( cols.Length != 1 )
				{
					throw new QueryException( "composite collection index in maxIndex" );
				}
				collectionElementColumns = new string[ ] {"max(" + cols[ 0 ] + StringHelper.ClosedParen};
				collectionElementType = memberPersister.IndexType;
			}
			else if( token.Equals( CollectionMinIndex ) )
			{
				if( !memberPersister.HasIndex )
				{
					throw new QueryException( "unindexed collection before .minIndex" );
				}
				string[ ] cols = memberPersister.IndexColumnNames;
				if( cols.Length != 1 )
				{
					throw new QueryException( "composite collection index in minIndex" );
				}
				collectionElementColumns = new string[ ] {"min(" + cols[ 0 ] + StringHelper.ClosedParen};
				collectionElementType = memberPersister.IndexType;
			}
			else if( token.Equals( CollectionMaxElement ) )
			{
				string[ ] cols = memberPersister.ElementColumnNames;
				if( cols.Length != 1 )
				{
					throw new QueryException( "composite collection element in maxElement" );
				}
				collectionElementColumns = new string[ ] {"max(" + cols[ 0 ] + StringHelper.ClosedParen};
				collectionElementType = memberPersister.ElementType;
			}
			else if( token.Equals( CollectionMinElement ) )
			{
				string[ ] cols = memberPersister.ElementColumnNames;
				if( cols.Length != 1 )
				{
					throw new QueryException( "composite collection element in minElement" );
				}
				collectionElementColumns = new string[ ] {"min(" + cols[ 0 ] + StringHelper.ClosedParen};
				collectionElementType = memberPersister.ElementType;
			}
			else
			{
				throw new QueryException( "expecting 'elements' or 'indices' after " + path );
			}
		}

		/// <summary></summary>
		public String CollectionOwnerName
		{
			get { return collectionOwnerName; }
		}
	}
}