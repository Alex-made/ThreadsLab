using System;
using System.Threading;
using System.Threading.Tasks;

namespace LabWork_Intermediate
{
	class Program
	{
		static void Main(string[] args)
		{
			//еще есть CountdownEvent, Barrier, SpinLock, ReaderWriterLockSlim
			
			//UseLock.Run();
			//UseMonitor.Run();
			//UseManualResetEvent.Run();
			//UseAutoResetEvent.Run();
			//UseSemaphore.Run();
			//var useBarrier = new UseBarrier(); useBarrier.Run();
			var useReaderWriterLockSlim = new UseReaderWriterLockSlim(); useReaderWriterLockSlim.Run();
		}
	}

	//несколько потоков имеют поочередный доступ к одной критической секции
	//если выполнить это без синхронизации доступа, то при выводе массивов будет хрень
	static class UseSemaphore
	{
		private static string[] commonString = new string[10];
		private static readonly Semaphore semaphore = new Semaphore(0, 5);
		public static void Run()
		{
			var initialTask = new Task(() =>
			{
				//инициализируем 5 тасков. сначала дадим доступ к работе 3м, а потом еще двум.
				Task[] tasks = new Task[5];
				for (var i = 0; i < 5; i++)
				{
					var taskNumber = i;
					tasks[i] = new Task(() =>
					{
						semaphore.WaitOne();
						//имитация параллельной работы
						Thread.Sleep(1000);

						Console.WriteLine("Работает таск № " + taskNumber);
					});
				}

				foreach (var task in tasks)
				{
					task.Start();
				}

				semaphore.Release(3);
				Thread.Sleep(3000);
				semaphore.Release(2);
				Task.WhenAll(tasks);
			});

			initialTask.Start();
			Console.ReadLine();
		}
	}

	//один поток ждет другого
	static class UseManualResetEvent
	{
		private static string commonString;
		//инициализиуем в несигнальном состоянии. ожидающие потоки не могут начать работать
		private static ManualResetEvent mre = new ManualResetEvent(false);

		public static void Run()
		{
			var task1 = new Task(() =>
			{
				//while (true)
				//{
				//	Console.Write(".");
				//	Thread.Sleep(100);
				//}
				mre.WaitOne();
				Console.WriteLine("Запущен первый поток. Сообщение от второго потока получено: " + commonString);
				//ожидащие потоки снова блокируются
				mre.Reset();
			});

			var task2 = new Task(() =>
			{
				Thread.Sleep(3000);
				Console.WriteLine("Записывается сообщение от второго потока");
				commonString = "Сообщение от второго потока";
				Console.WriteLine("Сообщение от второго потока записано");
				//устанавливаем в сигнальное методом set, чтобы ожидающие потоки могли начать работать
				mre.Set();
			});

			task1.Start();
			task2.Start();
			Console.ReadLine();
			//Task.WhenAll();
		}
	}

	//несколько потоков имеют поочередный доступ к одной критической секции
	//если выполнить это без синхронизации доступа, то при выводе массивов будет хрень
	static class UseAutoResetEvent
	{
		private static string[] commonString = new string[10];
		//инициализиуем в несигнальном состоянии. потоки ожидают выполнения. устанавливаем в сигнальное методом set, чтобы ожидающие потоки могли начать работать
		private static readonly AutoResetEvent are = new AutoResetEvent(false);
		public static void Run()
		{
			var initialTask = new Task(() =>
			{
				Task[] tasks = new Task[5];
				for (var i = 0; i < 5; i++)
				{
					var taskNumber = i;
					tasks[i] = new Task(() =>
					{
						//имитация параллельной работы
						Thread.Sleep(1000);

						//попадаем в критическую секцию. ждем сигнального события
						are.WaitOne();
						//поймали событие, оно автоматически ресетнулось
						{
							for (var j = 0; j < 10; j++)
							{
								commonString[j] = taskNumber.ToString();
							}

							for (var j = 0; j < 10; j++)
							{
								Console.Write(commonString[j]);
							}

							Console.WriteLine();
						}
						//выполнили критическую сессию, снова делаем set
						are.Set();
					});
				}

				foreach (var task in tasks)
				{
					task.Start();
				}
				are.Set();
				Task.WhenAll(tasks);
			});

			initialTask.Start();
			Console.ReadLine();
		}
	}

	static class UseLock
	{
		//разделяемый ресурс
		private static string commonString;
		private static object obj = new ();

		public static void Run()
		{
			var task1 = new Task(() =>
			{
				lock (obj)
				{
					commonString = "AAA";
					Console.WriteLine(commonString);
				}
			});

			var task2 = new Task(() =>
			{
				lock (obj)
				{
					commonString = "BBB";
					Console.WriteLine(commonString);
				}
			});

			task1.Start();
			task2.Start();
			Console.ReadLine();
			//Task.WhenAll();
		}
	}

	static class UseMonitor
	{
		private static string commonString;
		private static object obj = new();

		public static void Run()
		{
			var task1 = new Task(() =>
			{
				try
				{
					Monitor.Enter(obj);
					commonString = "AAA";
					Console.WriteLine(commonString);
				}
				finally
				{
					Monitor.Exit(obj);
				}
			});

			var task2 = new Task(() =>
			{
				try
				{
					Monitor.Enter(obj);
					commonString = "BBB";
					Console.WriteLine(commonString);
				}
				finally
				{
					Monitor.Exit(obj);
				}
			});

			task1.Start();
			task2.Start();
			Console.ReadLine();
			//Task.WhenAll();
		}
	}
}
