using System;
using System.Collections;

using NHibernate.Type;
using NHibernate.Persister;

namespace NHibernate.Impl
{
	/// <summary>
	/// Abstract superclass of algorithms that walk a tree of property values
	/// of an entity, and perform specific functionality for collections,
	/// components and associated entities.
	/// </summary>
	internal abstract class AbstractVisitor
	{
		private readonly SessionImpl session;

		protected AbstractVisitor(SessionImpl session)
		{
			this.session = session;
		}

		/// <summary>
		/// Dispatch each property value to <see cref="ProcessValue" />.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="types"></param>
		public virtual void ProcessValues(object[] values, IType[] types)
		{
			for (int i = 0; i < values.Length; i++)
			{
				ProcessValue(values[i], types[i]);
			}
		}

		protected virtual object ProcessComponent(object component, IAbstractComponentType componentType)
		{
			if (component != null)
			{
				ProcessValues(componentType.GetPropertyValues(component, session),
					componentType.Subtypes);
			}

			return null;
		}

		/// <summary>
		/// Visit a property value. Dispatch to the correct handler
		/// for the property type.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		protected object ProcessValue(object value, IType type)
		{
			if (type.IsPersistentCollectionType)
			{
				// Even process null collections
				return ProcessCollection(value, (PersistentCollectionType) type);
			}
			else if (type.IsEntityType)
			{
				return ProcessEntity(value, (EntityType) type);
			}
			else if (type.IsComponentType)
			{
				//TODO: what about a null component with a collection!
				//      we also need to clean up that "null collection"
				return ProcessComponent(value, (IAbstractComponentType) type);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Walk the tree starting from the given entity.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="persister"></param>
		public virtual void Process(object obj, IClassPersister persister)
		{
			ProcessValues(
				persister.GetPropertyValues(obj),
				persister.PropertyTypes);
		}

		/// <summary>
		/// Visit a collection. Default superclass implementation is a no-op.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		protected virtual object ProcessCollection(object collection, PersistentCollectionType type)
		{
			return null;
		}

		/// <summary>
		/// Visit a many-to-one or one-to-one associated entity. Default
		/// superclass implementation is a no-op.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		protected virtual object ProcessEntity(object value, EntityType entityType)
		{
			return null;
		}

		protected SessionImpl Session
		{
			get { return session; }
		}
	}
}
