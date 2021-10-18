using System;
using System.Collections.Generic;

namespace LabWork_ThreadSafeCache
{
	/// <summary>
	/// Представляет кэш без поддержки потокобезопасности.
	/// </summary>
	/// <typeparam name="TKey">Тип ключа.</typeparam>
	/// <typeparam name="TValue">Тип значения.</typeparam>
	public class Cache<TKey, TValue>
	{
		private Dictionary<TKey, LazyMade<TValue>> _cache;
		
		public Cache()
		{
			_cache = new Dictionary<TKey, LazyMade<TValue>>();
		}

		public TValue AddItem(TKey key, Func<TValue> producer)
		{
			//сначала проверим, есть ли такой элемент в кэше и, если нет, то добавим
			var itemAlreadyExists = false;
			//как rws понимает, какой объект нужно лочить
			itemAlreadyExists = _cache.TryGetValue(key, out var lazyObject);
			
			if (!itemAlreadyExists)
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

			return lazyObject.Value;
		}

		public TValue ReadItem(TKey key)
		{
			_cache.TryGetValue(key, out var lazyObject);
			
			return lazyObject == null ? default : lazyObject.Value;
		}
	}
}
