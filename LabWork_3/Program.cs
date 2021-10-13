using System;
using System.Threading;
using System.Threading.Tasks;

namespace LabWork_3
{
	class Program
	{
		private static object _locker = new();

		static void Main(string[] args)
		{
			//ReadWriteWithLock();
			ReadWriteWithAutoResetEvent();
		}

		//метод записи/чтения из буфера с использованием lock
		private static void ReadWriteWithLock()
		{
			var buffer = new Buffer();

			var writerTasks = new Task[3];
			//инициализируем потоки писатели
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				writerTasks[taskNumber] = Task.Run(() =>
				{
					while (true)
					{
						Thread.Sleep(1500);
						//если буфер пуст, пишем в него
						lock (_locker)
						{
							if (buffer.IsEmpty)
							{
								buffer.Write("В буфер записана информация из потока №" + taskNumber);
								Console.WriteLine("В буфер записана информация из потока №" + taskNumber);
							}
						}
					}
				});
			}

			var readerTasks = new Task[3];
			//инициализируем потоки читатели
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				readerTasks[taskNumber] = Task.Run(() =>
				{
					//пока буфер заполнен, читаем из него
					while (true)
					{
						Thread.Sleep(4000);
						//используем двойную проверку для уменьшения кол-ва блокировок
						//ждем, пока буфер заполненится писателем, а затем заходим в критическую секцию и читаем из буфера, обновляя его статус isEmpty
						if (!buffer.IsEmpty)
						{
							lock (_locker)
							{
								if (!buffer.IsEmpty)
								{
									buffer.Read();
									Console.WriteLine("Из буфера прочитана информация в потоке №" + taskNumber);
								}
							}
						}
					}
				});
			}

			Task.WhenAll(writerTasks);
			Task.WhenAll(readerTasks);

			Console.ReadLine();
		}

		//метод записи/чтения из буфера с использованием AutoResetEvent
		private static void ReadWriteWithAutoResetEvent()
		{
			//инициализиуем в несигнальном состоянии и позже устанавливаем в сигнальное методом set, чтобы ожидающие потоки могли начать работать
			var are = new AutoResetEvent(false);
			//инициализируем буфер
			var buffer = new Buffer();

			var writerTasks = new Task[3];
			//инициализируем потоки писатели
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				writerTasks[taskNumber] = Task.Run(() =>
				{
					while (true)
					{
						//попадаем в критическую секцию. ждем сигнального события
						are.WaitOne();
						//поймали событие, оно автоматически ресетнулось, т.е. никто не может больше проваливаться в критические секции

						Thread.Sleep(1500);
						//если буфер пуст, пишем в него
						if (buffer.IsEmpty)
						{
							buffer.Write("В буфер записана информация из потока №" + taskNumber);
							Console.WriteLine("В буфер записана информация из потока №" + taskNumber);
						}

						are.Set();
					}
				});
			}

			var readerTasks = new Task[3];
			//инициализируем потоки читатели
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				readerTasks[taskNumber] = Task.Run(() =>
				{
					//пока буфер заполнен, читаем из него
					while (true)
					{
						Thread.Sleep(4000);
						//используем двойную проверку для уменьшения кол-ва блокировок
						//ждем, пока буфер заполненится писателем, а затем заходим в критическую секцию и читаем из буфера, обновляя его статус isEmpty
						if (!buffer.IsEmpty)
						{
							are.WaitOne();
							
							if (!buffer.IsEmpty)
							{
								buffer.Read();
								Console.WriteLine("Из буфера прочитана информация в потоке №" + taskNumber);
							}

							are.Set();
						}
					}
				});
			}

			are.Set();
			Task.WhenAll(writerTasks);
			Task.WhenAll(readerTasks);

			Console.ReadLine();
		}
	}


	class Buffer
	{
		private string _buffer;
		
		public void Write(string item)
		{
			_buffer = item;
		}

		public string Read()
		{
			var result = _buffer;
				_buffer = null;
				return result;
		}

		/// <summary>
		/// Возвращает флаг, что буфер пустой.
		/// </summary>
		public bool IsEmpty
		{
			get => _buffer == null ? true : false;
		}

	}
}
