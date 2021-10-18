using System;
using System.Collections.Generic;
using System.Text;

namespace LabWork_ThreadSafeCache
{
	/// <summary>
	/// Представляет собственную реализацию класса <see cref="Lazy{T}"/>.
	/// </summary>
	/// <remarks>Создана в целях обучения.</remarks>
	public class LazyMade<T>
	{
		private Func<T> _creationFunc;
		private bool _alreadyCreated = false;
		private T _objectInstance;
		private object _lock = new object();

		//TODO добавить конструктор без праметров, при вызове value вызывающий соответствующий конструктор без параметров переданного типа
		public LazyMade(Func<T> creationFunc)
		{
			_creationFunc = creationFunc ?? throw new ArgumentNullException(nameof(creationFunc));
		}

		public T Value
		{
			get
			{
				if (!_alreadyCreated)
				{
					lock (_lock)
					{
						if (!_alreadyCreated)
						{
							_objectInstance = _creationFunc.Invoke();
							_alreadyCreated = true;
							_creationFunc = null;
						}
					}
				}
				return _objectInstance;
			} 
		}
	}
}
