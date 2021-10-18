using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LabWork_ThreadSafeCache
{
	class Program
	{
		private static readonly object Locker = new object();
		private static List<Guid> WritedGuids = new List<Guid>();

		//TODO сделать пример с использ. безопасного и небезопасного кэша
		static void Main(string[] args)
		{
			//почему объект уже инициализирован, хотя value еще не вызван?
			//как можно лучше сымитировать работу потоков?
			var myLazy = new LazyMade<List<int>>(() => new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			var @object = myLazy.Value;
			var lazy = new Lazy<List<int>>(() => new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 });

			var cache = new ThreadSafeCache<Guid, string>();
			//var cache = new Cache<Guid, string>();

			var writerTasks = new Task[3];
			//инициализируем потоки писатели - лишь один поток может писать в буфер в каждый момент времени
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				writerTasks[taskNumber] = Task.Run(() =>
				{
					//пул сообщений, которые необходимо передать читателям через многоэлементный буфер
					//в качестве пула будем использовать очередь.
					var messageList = new Queue<string>(new[]
					{
						"Сообщение 1 потока № " + taskNumber,
						"Сообщение 2 потока № " + taskNumber,
						//"Сообщение 3 потока № " + taskNumber,
						//"Сообщение 4 потока № " + taskNumber,
						//"Сообщение 5 потока № " + taskNumber
					});

					//пока не освободится messageList, будем писать в Cache
					while (messageList.Any())
					{
						//Имитация долгой работы
						Thread.Sleep(1500);
						var message = messageList.Peek();
						var key = Guid.NewGuid();
						var writedMessage = cache.AddItem(key, () => message);
						lock (Locker)
						{
							WritedGuids.Add(key);
						}
						Console.WriteLine($"В буфер записано сообщение \"{writedMessage}\" из потока №" + taskNumber);
						messageList.Dequeue();
					}
				});
			}

			//инициализируем потоки читатели, которые будут читать значения буфера.
			//Читатели могут читать значения из буфера параллельно, но если начинается запись, то чтение блокируется и ждет окончания записи
			var readerTasks = new Task[3];
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				readerTasks[taskNumber] = Task.Run(() =>
				{
					while (true) //почему эта задача работает, если тут стоит только true?
					{
						Thread.Sleep(2000);
						Guid key = Guid.Empty;
						lock (Locker)
						{
							if (WritedGuids.Any())
							{
								key = WritedGuids.First();
								WritedGuids.Remove(key);
							}
						}

						if (key == Guid.Empty)
						{
							continue;
						}

						var bufferValue = cache.ReadItem(key);
						Console.WriteLine($"Из буфера прочитана информация \"{bufferValue}\" в потоке №" + taskNumber);
					}
				});
			}

			Console.ReadLine();
		}
	}
}
