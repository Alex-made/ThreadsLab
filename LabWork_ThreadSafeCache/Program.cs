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

		public class SomeClass<T>
		{
			static SomeClass()
			{
				SomeStaticField = new List<T>();
			}

			public SomeClass()
			{
				var aa = 11;
			}

			public static List<T> SomeStaticField;

			public T Id
			{
				get;
				set;
			}

			public int Sum (int a, int b)
			{
				return a + b;
			}
		}

		static void Main(string[] args)
		{
			//var aa = new SomeClass<int>();
			//var a = SomeClass<string>.SomeStaticField;
			//var b = 11;
			
			//var s1 = string.Format("{0}{1}", "abc", "cba");
			//var s2 = "abc" + "cba";
			//var s3 = "abccba";

			//Console.WriteLine(s1 == s2);
			//Console.WriteLine((object)s1 == (object)s2);
			//Console.WriteLine(s2 == s3);
			//Console.WriteLine((object)s2 == (object)s3);

			//Console.WriteLine((object)s1.GetHashCode());
			//var os1 = (object) s1;
			//var os2 = (object) s2;
			//var os3 = (object)s3;
			//Console.WriteLine((string) os1 == os2 + " оператор сравнения");
			//Console.WriteLine(os1.Equals(os2) + " Equals os1 os2");
			//Console.WriteLine(os2.Equals(os3) + " Equals os2 os3");
			//Console.WriteLine((object)s2.GetHashCode());
			//Console.WriteLine((object)s3.GetHashCode());

			//object sync = new object();
			//var thread = new Thread(() =>
			//{
			//	try
			//	{
			//		Work();
			//	}
			//	finally
			//	{
			//		lock (sync)
			//		{
			//			Monitor.PulseAll(sync);
			//		}
			//	}
			//});
			//thread.Start();
			//lock (sync)
			//{
			//	Monitor.Wait(sync);
			//}
			//Console.WriteLine("test");
		
		//static void Work()
		//{
		//	Thread.Sleep(1000);
		//}

		//Console.ReadLine();

			//почему объект уже инициализирован, хотя value еще не вызван?
			//как можно лучше сымитировать работу потоков?
			var myLazy = new LazyMade<List<int>>(() =>
			{
				return new List<int>
				{
					1,
					2,
					3,
					4,
					5,
					6,
					7,
					8,
					9
				};
			});
			//var @object = myLazy.Value;
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
