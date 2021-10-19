using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LabWork_4
{
	//Несколько потоков работают с общим многоэлементным буфером. Потоки делятся на "читателей" и "писателей"
	//Писатели осуществляют запись в буфер, если есть свободные ячейки. Читатели извлекают содержимое буфера, если есть заполненные ячейки.
	//Работа приложения заканчивается после того, как все сообщения писателей будут обработаны читателями через общий буфер.
	//В качестве буфера используется "кольцевой массив".
	class Program
	{
		static void Main(string[] args)
		{
			//тесты для кольцевого массива
			TestCircleBuffer();
			
			var _locker = new object();
			//инициализируем буфер
			var buffer = new Buffer();
			
			var writerTasks = new Task[3];
			//инициализируем потоки писатели - лишь один поток может писать в буфер в каждый момент времени
			for (var i = 0; i < 3; i++)
			{
				//присвоение нужно, чтобы избежать замыкания. если оставить просто i, то номер таска будет только 3, т.к. в процессе итерирования счетчик
				//дойдет до 3х, но не будет удовлетворять неравенству в цикле. Но увеличен все равно будет и его захватит анонимный метод, где создается Task. 
				//Соответственно, счетчик всех тасков будет 3
				var taskNumber = i; 
				writerTasks[taskNumber] = Task.Run(() =>
				{
					//пул сообщений, которые необходимо передать читателям через многоэлементный буфер
					//в качестве пула будем использовать встроенную очередь. на самом деле не важно, где хранить эти сообщения
					var messageList = new Queue<string>(new[]
					{
						"Сообщение 1 потока № " + taskNumber,
						"Сообщение 2 потока № " + taskNumber,
						//"Сообщение 3 потока № " + taskNumber,
						//"Сообщение 4 потока № " + taskNumber,
						//"Сообщение 5 потока № " + taskNumber
					});
					
					//пока буфер не заполнится, берем элемент и записываем его в буфер
					while (!buffer.IsFull)
					{
						if (!messageList.Any())
						{
							return;
						}
						//Имитация долгой работы
						Thread.Sleep(1500);
						var messageWritedToBuffer = false;
						var message = messageList.Peek();
						//если буфер не заполнен и не занят, пишем в него
						if (!buffer.IsBusy && !buffer.IsFull)
						{
							lock (_locker)
							{
								if (!buffer.IsBusy && !buffer.IsFull)
								{
									buffer.IsBusy = true;
									buffer.Enqueue(message);
									Console.WriteLine($"В буфер записано сообщение \"{message}\" из потока №" + taskNumber);
									buffer.IsBusy = false;
									messageWritedToBuffer = true;
								}
							}

							if (messageWritedToBuffer)
							{
								messageList.Dequeue();
							}
						}
					}
				});
			}

			//инициализируем потоки читатели, которые будут читать верхнее значение буфера.
			//Читатели могут читать значения из буфера параллельно, но если начинается запись, то чтение блокируется и ждет окончания записи
			var readerTasks = new Task[3];
			for (var i = 0; i < 3; i++)
			{
				var taskNumber = i;
				readerTasks[taskNumber] = Task.Run(() =>
				{
					while (true)
					{
						Thread.Sleep(2000);
						if (!buffer.IsBusy)
						{
							//Peek здесь используется для того, чтобы просто прочитать, что там есть. Если делать Dequeue, это буфер нужно блокировать
							var bufferValue = buffer.Peek(); 
							Console.WriteLine($"Из буфера прочитана информация \"{bufferValue}\" в потоке №" + taskNumber);
						}
					}
				});
			}

			
			Task.WhenAll(writerTasks);
			Task.WhenAll(readerTasks);

			Console.ReadLine();
		}

		private static void TestCircleBuffer()
		{
			var buffer = new Buffer(3);
			

			buffer.Enqueue("1");
			buffer.Enqueue("2");
			buffer.Enqueue("3");

			var a1 = buffer.Dequeue(); //1
			var a2 = buffer.Dequeue(); //2
			var a3 = buffer.Dequeue(); //3

			buffer.Enqueue("1");
			buffer.Enqueue("2");
			buffer.Enqueue("3");
			buffer.Enqueue("4");
			a2 = buffer.Dequeue(); //2
			a3 = buffer.Dequeue(); //3
			var a4 = buffer.Dequeue(); //4

			buffer.Enqueue("1");
			buffer.Enqueue("2");
			buffer.Enqueue("3");
			buffer.Enqueue("4");
			buffer.Enqueue("5");
			a3 = buffer.Dequeue(); //3
			a4 = buffer.Dequeue(); //4
			var a5 = buffer.Dequeue(); //5

			
			//проверка, что буфер не полон
			var isNotFull = buffer.IsFull;
			//проверка, что буфер пуст
			var isEmpty = buffer.IsEmpty;
		}
	}

	

	//многоэлементный кольцевой буфер с перезаписью в случае превышения его размера
	class Buffer
	{
		private string[] _buffer;
		//указывает на индекс текущего верхнего элемента массива
		private int _head;
		//указывает на индекс текущего нижнего элемента массива
		private int _tail;
		
		private int _usedLength;

		public bool IsBusy
		{
			get;
			set;
		}


		public Buffer() : this(5)
		{
		}

		//буфер начинаем с 0 индекса массива
		public Buffer(int capacity)
		{
			_buffer = new string[capacity];
			_head = -1;
			_tail = 0;
			_usedLength = 0;
		}

		private int NextPosition(int position)
		{
			return position >= _buffer.Length - 1 ? 0 : position + 1;
		}

		//когда я помещаю элемент в буфер, хвост начинает указывать на него
		public void Enqueue(string item)
		{
			//передвинул голову на один элемент массива вперед
			_head = NextPosition(_head);
			//записал значение в голову буфера
			_buffer[_head] = item;
			//если буфер заполнен
			if (IsFull)
			{
				//передвинул хвост
				_tail = NextPosition(_tail);
			}
			else
			{
				_usedLength++;
			}
		}

		public string Dequeue()
		{
			var result =  _buffer[_tail];
			_tail = NextPosition(_tail);
			_usedLength--;
			return result;
		}

		public string Peek()
		{
			return _buffer[_tail];
		}

		/// <summary>
		/// Возвращает флаг, что буфер заполнен.
		/// </summary>
		public bool IsFull
		{
			get => _usedLength == _buffer.Length;
		}

		/// <summary>
		/// Возвращает флаг, что буфер пуст.
		/// </summary>
		public bool IsEmpty
		{
			get => _usedLength == 0;
		}
	}
}
