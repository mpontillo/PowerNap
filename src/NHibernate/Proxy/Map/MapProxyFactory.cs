using System;
using System.Reflection;
using Iesi.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Proxy.Map
{
	public class MapProxyFactory : IProxyFactory
	{
		private string entityName;

		#region IProxyFactory Members

		public void PostInstantiate(string entityName, System.Type persistentClass, ISet<System.Type> interfaces,
																MethodInfo getIdentifierMethod, MethodInfo setIdentifierMethod,
																IAbstractComponentType componentIdType)
		{
			this.entityName = entityName;
		}

		public INHibernateProxy GetProxy(object id, ISessionImplementor session)
		{
			return new MapProxy(new MapLazyInitializer(entityName, id, session));
		}

		public object GetLazyPropertyInterceptionProxy()
		{
			throw new NotSupportedException("Map proxies do not support lazy properties: " + entityName);
		}

		#endregion
	}
}
