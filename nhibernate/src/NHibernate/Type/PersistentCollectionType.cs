using System;
using System.Collections;
using System.Data;

using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Sql;

namespace NHibernate.Type {

	/// <summary>
	/// PersistentCollectionType.
	/// </summary>
	public abstract class PersistentCollectionType : AbstractType, IAssociationType	{
		
		private readonly string role;
		private static readonly DbType[] NoSqlTypes = {};
		
		public PersistentCollectionType(string role) {
			this.role = role;
		}

		public virtual string Role {
			get { return role; }
		}

		public override bool IsPersistentCollectionType {
			get { return true; }
		}

		public override sealed bool Equals(object x, object y) {
            //TODO: proxies?
			return x==y;
		}

		protected abstract PersistentCollection Instantiate(ISessionImplementor session, CollectionPersister persister);

		public override object NullSafeGet(IDataReader rs, string name, ISessionImplementor session, object owner) {
			throw new AssertionFailure("bug in PersistentCollectionType");
		}
	
		public override object NullSafeGet(IDataReader rs, string[] name, ISessionImplementor session, object owner) {
			return ResolveIdentifier( Hydrate(rs, name, session, owner), session, owner );
		}

		public virtual object GetCollection(object id, object owner, ISessionImplementor session) {
		
			CollectionPersister persister = session.GetFactory().GetCollectionPersister(role);
			
			PersistentCollection collection = persister.GetCachedCollection(id, owner, session);
			if (collection!=null) {
				session.AddInitializedCollection(collection, persister, id);
				return collection.GetCachedValue();
			}
			else {
				collection = Instantiate(session, persister);
				session.AddUninitializedCollection(collection, persister, id);
				return collection.GetInitialValue( persister.IsLazy );
			}
		}

		public override void NullSafeSet(IDbCommand cmd, object value, int index, ISessionImplementor session) {
		}
	
		public override DbType[] SqlTypes(IMapping session) {
			return NoSqlTypes;
		}
	
		public override int GetColumnSpan(IMapping session) {
			return 0;
		}	
	
		public override string ToXML(object value, ISessionFactoryImplementor factory) {
			return (value==null) ? null : value.ToString();
		}
	
		public override object DeepCopy(object value) {
			return value;
		}
	
		public override string Name {
			get {
				return ReturnedClass.Name;
			}
		}

		//Is it correct?
		//Was:
		//public Iterator getElementsIterator(Object collection) {
		//	return ( (java.util.Collection) collection ).iterator();
		//}
		public virtual IEnumerator GetElementsEnumerator(object collection) {
			return ( (IEnumerable)collection ).GetEnumerator();
		}
	
		public override bool IsMutable {
			get { return false; }
		}
	
		public override object Disassemble(object value, ISessionImplementor session) {
			if (value==null) {
				return null;
			}
			else {
				object id = session.GetLoadedCollectionKey( (PersistentCollection) value );
				if (id==null)
					throw new AssertionFailure("Null collection id");
				return id;
			}
		}

		public override object Assemble(object cached, ISessionImplementor session, object owner) {
			return ResolveIdentifier(cached, session, owner);
		}
	
		public override bool IsDirty(object old, object current, ISessionImplementor session) {
		
			System.Type ownerClass = session.GetFactory().GetCollectionPersister(role).OwnerClass;
		
			if ( !session.GetFactory().GetPersister(ownerClass).IsVersioned ) {
				// collections don't dirty an unversioned parent entity
				return false;
			}
			else {
				return base.IsDirty(old, current, session);
			}
		}

		public override bool HasNiceEquals {
			get { return false; }
		}
	
		public abstract PersistentCollection Wrap(ISessionImplementor session, object collection);
	
		/**
		 * Note: return true because this type is castable to IAssociationType. Not because
		 * all collections are associations.
		 */
		public override bool IsAssociationType {
			get { return true; }
		}
	
		public virtual ForeignKeyType ForeignKeyType {
			get { return ForeignKeyType.ForeignKeyToParent;	}
		}
	
		public override object Hydrate(IDataReader rs, string[] name, ISessionImplementor session, object owner) {
			return session.GetEntityIdentifier(owner);
		}
	
		public override object ResolveIdentifier(object value, ISessionImplementor session, object owner) {
			if (value==null) {
				return null;
			}
			else {
				return GetCollection( value, owner, session);
			}
		}
	
		public virtual bool IsArrayType {
			get { return false; }
		}
	
		public abstract PersistentCollection AssembleCachedCollection(ISessionImplementor session, CollectionPersister persister, object disassembled, object owner);
	}
}