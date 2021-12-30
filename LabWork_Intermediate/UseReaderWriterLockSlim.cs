using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LabWork_Intermediate
{
	class NotSynchronizedCache
	{
		private Dictionary<int, string> cache = new Dictionary<int, string>();

		public int Count
		{
			get => cache.Count;
		}

		public string Read(int key)
		{
			return cache[key];
		}

		public void Add(int key, string value)
		{
			cache.Add(key, value);
		}

		

		public AddOrUpdateStatus AddOrUpdate(int key, string value)
		{
			
				string result = null;
				if (cache.TryGetValue(key, out result))
				{
					if (result == value)
					{
						return AddOrUpdateStatus.Unchanged;
					}
					else
					{
						cache[key] = value;
						return AddOrUpdateStatus.Updated;
					}
				}
				else
				{
					cache.Add(key, value);
					return AddOrUpdateStatus.Added;
				}
		}
	}

	public enum AddOrUpdateStatus
	{
		Added,
		Updated,
		Unchanged
	};

	class SynchronizedCache
	{
		private System.Threading.ReaderWriterLockSlim rwls = new System.Threading.ReaderWriterLockSlim();
		private Dictionary<int, string> cache = new ();

		public int Count
		{
			get => cache.Count;
		}

		public string Read(int key)
		{
			rwls.EnterReadLock();
			try
			{
				return cache[key];
			}
			finally
			{
				rwls.ExitReadLock();
			}
		}

		public void Add(int key, string value)
		{
			rwls.EnterWriteLock();
			try
			{
				cache.Add(key, value);
			}
			finally
			{
				rwls.ExitWriteLock();
			}
		}

		public bool AddWithTimeout(int key, string value, int timeout)
		{
			if (rwls.TryEnterWriteLock(timeout))
			{
				try
				{
					cache.Add(key, value);
				}
				finally
				{
					rwls.ExitWriteLock();
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public AddOrUpdateStatus AddOrUpdate(int key, string value)
		{
			rwls.EnterUpgradeableReadLock();
			try
			{
				string result = null;
				if (cache.TryGetValue(key, out result))
				{
					if (result == value)
					{
						return AddOrUpdateStatus.Unchanged;
					}
					else
					{
						rwls.EnterWriteLock();
						try
						{
							cache[key] = value;
						}
						finally
						{
							rwls.ExitWriteLock();
						}
						return AddOrUpdateStatus.Updated;
					}
				}
				else
				{
					rwls.EnterWriteLock();
					try
					{
						cache.Add(key, value);
					}
					finally
					{
						rwls.ExitWriteLock();
					}
					return AddOrUpdateStatus.Added;
				}
			}
			finally
			{
				rwls.ExitUpgradeableReadLock();
			}
		}

		public void Delete(int key)
		{
			rwls.EnterWriteLock();
			try
			{
				cache.Remove(key);
			}
			finally
			{
				rwls.ExitWriteLock();
			}
		}

		~SynchronizedCache()
		{
			if (rwls != null) rwls.Dispose();
		}
    }

	class UseReaderWriterLockSlim
	{
		public void Run()
		{
			//var sc = new NotSynchronizedCache();
			var sc = new SynchronizedCache();
			var tasks = new List<Task>();
			int itemsWritten = 0;

			// Выполнить писателя.
			tasks.Add(Task.Run(() => {
				String[] vegetables = { "broccoli", "cauliflower",
														  "carrot", "sorrel", "baby turnip",
														  "beet", "brussel sprout",
														  "cabbage", "plantain",
														  "spinach", "grape leaves",
														  "lime leaves", "corn",
														  "radish", "cucumber",
														  "raddichio", "lima beans" };
				for (int ctr = 1; ctr <= vegetables.Length; ctr++)
				{
					Thread.Sleep(100);
					sc.Add(ctr, vegetables[ctr - 1]);
				}

				itemsWritten = vegetables.Length;
				Console.WriteLine("Task {0} wrote {1} items\n",
								  Task.CurrentId, itemsWritten);
			}));
			// Выполнить 2 читателя, один читает с начала до конца и второй от конца до начала.
			//чтение будет до тех пор, пока не будет что-то записано в кэш
			for (int ctr = 0; ctr <= 1; ctr++)
			{
				bool desc = Convert.ToBoolean(ctr);
				tasks.Add(Task.Run(() => {
					int start, last, step;
					int items;
					do
					{
						String output = String.Empty;
						items = sc.Count;
						if (!desc) //если от начала до конца
						{
							start = 1;
							step = 1;
							last = items;
						}
						else //если от конца до начала
						{
							start = items;
							step = -1;
							last = 1;
						}
						//читаем данные из кэша
						for (int index = start; desc ? index >= last : index <= last; index += step)
							output += String.Format("[{0}] ", sc.Read(index));

						Console.WriteLine("Task {0} read {1} items: {2}\n",
										  Task.CurrentId, items, output);
					} while (items < itemsWritten | itemsWritten == 0);
				}));
			}
			// Выполнить таск чтения/обновления. Не совсем корректно работает, т.к. пробегается по кэшу 1 раз и в момент прохода элемента "cucumber" еще может не быть
			tasks.Add(Task.Run(() => {
				Thread.Sleep(100);
				for (int ctr = 1; ctr <= sc.Count; ctr++)
				{
					String value = sc.Read(ctr);
					if (value == "cucumber")
					{
						if (sc.AddOrUpdate(ctr, "green bean") != AddOrUpdateStatus.Unchanged)
							Console.WriteLine("Changed 'cucumber' to 'green bean'");
					}
				}
			}));

			// Ждем до завершения всех тасков.
			Task.WaitAll(tasks.ToArray());

			// Покажем содержимое кэша в итоге.
			Console.WriteLine();
			Console.WriteLine("Values in synchronized cache: ");
			for (int ctr = 1; ctr <= sc.Count; ctr++)
				Console.WriteLine("   {0}: {1}", ctr, sc.Read(ctr));
		}
	}
}
