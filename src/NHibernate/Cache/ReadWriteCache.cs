using System;
using System.Runtime.CompilerServices;

namespace NHibernate.Cache {

	/// <summary>
	/// Caches data that is sometimes updated while maintaining "read committed"
	/// isolation level.
	/// </summary>
	/// <remarks>
	/// Works at the "Read Committed" isolation level
	/// </remarks>
	public class ReadWriteCache : ICacheConcurrencyStrategy 
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ReadWriteCache));
		private readonly ICache cache;

		public ReadWriteCache(ICache cache) 
		{
			this.cache = cache;
		}

		#region ICacheConcurrencyStrategy Members

		[MethodImpl(MethodImplOptions.Synchronized)]
		public object Get(object key, long txTimestamp) 
		{
			if (log.IsDebugEnabled) log.Debug("Cache lookup: " + key);

			CachedItem item = cache.Get(key) as CachedItem;
			if (
				item!=null &&
				item.FreshTimestamp < txTimestamp &&
				item.IsFresh // || txTimestamp < item.LockTimestamp
				) 
			{
				if (log.IsDebugEnabled) log.Debug("Cache hit: " + key);
				return item.Value;
			} 
			else 
			{
				if (log.IsDebugEnabled) log.Debug("Cache miss: " + key);
				return null;
			}
		}

		//TODO: Actually keep locked CacheItems in a different Hashtable 
		// in this class until unlocked

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Lock(object key) 
		{
			if (log.IsDebugEnabled) log.Debug("Invalidating: " + key);
			
			CachedItem item = cache.Get(key) as CachedItem;
			if ( item==null) item = new CachedItem(null);
			item.Lock();
			cache.Put(key, item);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool Put(object key, object value, long txTimestamp) 
		{
			if (log.IsDebugEnabled) log.Debug("Caching: " + key);
			
			CachedItem item = cache.Get(key) as CachedItem;
			if (
				item==null ||
				(item.IsUnlocked && !item.IsFresh && item.UnlockTimestamp < txTimestamp)
				) 
			{
				cache.Put(key, new CachedItem(value) );
				if (log.IsDebugEnabled) log.Debug("Cached: " + key);
				return true;
			} 
			else
			{
				if (log.IsDebugEnabled) log.Debug("Could not cache: " + key);
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Release(object key) 
		{
			if (log.IsDebugEnabled) log.Debug("Releasing: " + key);

			CachedItem item = cache.Get(key) as CachedItem;
			if (item != null) 
			{
				item.Unlock();
				cache.Put(key, item);
			} 
			else 
			{
				log.Warn("An item was expired by the cache while it was locked");
			}
		}

		public void Clear() 
		{
			cache.Clear();
		}

		public void Remove(object key) 
		{
			cache.Remove(key);
		}

		public void Destroy() 
		{
			try 
			{
				cache.Destroy();
			}
			catch(Exception e) 
			{
				log.Warn("Could not destroy cache", e);
			}
		}

		#endregion

	}
}
