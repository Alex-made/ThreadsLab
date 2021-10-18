using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LabWork_ThreadSafeCache
{
	/// <summary>
	/// Представляет кэш с поддержкой потокобезопасности.
	/// </summary>
	/// <typeparam name="TKey">Тип ключа.</typeparam>
	/// <typeparam name="TValue">Тип значения.</typeparam>
	public class ThreadSafeCache<TKey, TValue>
	{
		private Dictionary<TKey, LazyMade<TValue>> _cache;
		private ReaderWriterLockSlim rws = new ReaderWriterLockSlim();


		public ThreadSafeCache()
		{
			_cache = new Dictionary<TKey, LazyMade<TValue>>();
		}

		public bool IsEmpty()
		{
			rws.EnterReadLock();
			try
			{
				return _cache.Count == 0;
			}
			finally
			{
				rws.ExitReadLock();
			}
		}

		public TValue AddItem(TKey key, Func<TValue> producer)
		{
			LazyMade<TValue> lazyObject;

			//сначала проверим, есть ли такой элемент в кэше и, если нет, то добавим
			var itemAlreadyExists = false;
			//как rws понимает, какой объект нужно лочить
			rws.EnterReadLock();
			try
			{
				itemAlreadyExists = _cache.TryGetValue(key, out lazyObject);
			}
			finally
			{
				rws.ExitReadLock();
			}
			if (!itemAlreadyExists)
			{
				rws.EnterWriteLock();
				try
				{
					//пока я заходил в критическую секицю действительно ничего не изменилось?
					itemAlreadyExists = _cache.TryGetValue(key, out lazyObject);

					//добавляем новый объект в Dictionary
					lazyObject = new LazyMade<TValue>(producer);
					if (!itemAlreadyExists && !_cache.TryAdd(key, lazyObject))
					{
						throw new ArgumentException($"Элемент {lazyObject.Value} с ключом {key} не добавить в кэш");
					}
				}
				finally
				{
					rws.ExitWriteLock();
				}
			}

			return lazyObject.Value;
		}

		public TValue ReadItem(TKey key)
		{
			LazyMade<TValue> lazyObject;

			rws.EnterReadLock();
			try
			{
				_cache.TryGetValue(key, out lazyObject);
			}
			finally
			{
				rws.ExitReadLock();
			}

			return lazyObject == null ? default : lazyObject.Value;
		}
	}
}
