using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LabWork_2
{
	class Program
	{
		static void Main(string[] args)
		{
			var initialArray = GetInitialArray(30);
			var a = initialArray.FindSimple(49);


			var commandHandler = new CommandHandler();
			commandHandler.Execute(500000, false);
		}

		private static int[] GetInitialArray(int top)
		{
			var initialArray = new int[top];
			for (var i = 0; i < top; i++)
			{
				initialArray[i] = i;
			}
			initialArray[1] = 0;
			return initialArray;
		}
	}

	internal class CommandHandler
	{
		private int _commonDividend;
		private readonly object _locker = new();
		private int[] _initialArray;

		public void Execute(int topSimpleDigit, bool printOnMonitor)
		{
			// 1 этап - нахождение чисел от 2 до корня из topSimpleDigit включительно
			// заполняем исходный массив числами от 0 до 20. В конце решения выбросим числа 0 из результата. Это сделано для совпадения индексов и значений чисел на первом этапе.
			_initialArray = new int[topSimpleDigit];
			for (var i = 0; i < topSimpleDigit; i++)
			{
				_initialArray[i] = i;
			}
			_initialArray[1] = 0;
			// просеиваем простые числа от 2 до корня из topSimpleDigit (arrayBoundary) не включая само arrayBoundary
			var arrayBoundary = (int) Math.Sqrt(topSimpleDigit);  
			for (var i = 0; i < arrayBoundary; i++)
			{
				if (_initialArray[i] == 0)
				{
					continue;
				}
				//находим число, которое будем последовательно пробовать делить на каждый элемент массива
				var dividend = _initialArray[i];
				for (var j = 0; j < arrayBoundary; j++)
				{
					if (_initialArray[j] == 0)
					{
						continue;
					}
					if (dividend % _initialArray[j] == 0 && _initialArray[j] != dividend) // исключить деление взятого числа само на себя
					{
						_initialArray[i] = 0;
						break;
					}
				}
			}

			InnerExecute(arrayBoundary, printOnMonitor);

			InnerExecuteParallel(5, arrayBoundary, printOnMonitor);
		}

		private void InnerExecute(int arrayBoundary, bool printOnMonitor)
		{
			var stopwatch = Stopwatch.StartNew();

			// просеиваем простые числа от точки разделения массива до конца массива.
			for (var j = arrayBoundary; j < _initialArray.Length; j++)
			{
				if (_initialArray[j] == 0)
				{
					continue;
				}

				var dividend = _initialArray[j];

				for (var t = 0; t < _initialArray.Length; t++)
				{
					if (_initialArray[t] == 0)
					{
						continue;
					}
					//последовательно делим выбранный элемент массива на каждый элемент массива, начиная сначала
					if (dividend % _initialArray[t] == 0 && _initialArray[t] != dividend) // исключить деление взятого числа само на себя
					{
						//если выбранный элемент массива делится на какой-либо элемент массива, кром себя самого, то он не простое число
						_initialArray[j] = 0;
						break;
					}
				}
			}

			stopwatch.Stop();
			
			Console.WriteLine("Метод InnerExecute завершен");

			Console.WriteLine("Время работы программы синхронно: " + stopwatch.Elapsed);

			if (printOnMonitor)
			{
				foreach (var result in _initialArray)
				{
					if (result == 0)
					{
						continue;
					}
					Console.Write(result + ", ");
				}
			}

			Console.WriteLine();
		}

		private void InnerExecuteParallel(int threadsCount, int arrayBoundary, bool printOnMonitor)
		{
			var stopwatch = Stopwatch.StartNew();

			// теперь берем число, которое будет смотрет все потоки - свободное число, с которым еще не работали потоки.
			// Например, 1 поток взял число 4, значит _commonDividend увеличился на 1 и следующий поток может взять в работу с числом 5
			_commonDividend = arrayBoundary;

			var tasks = new Task[threadsCount];

			for (var i = 0; i < tasks.Length; i++)
			{
				var taskNumber = i;  //присвоение нужно, чтобы избежать замыкания
				tasks[taskNumber] = new Task(() =>
				{
					//пока localDividend не равно верхнему значению массива _initialArray нужно брать новый localDividend
					while (_commonDividend < _initialArray.Length)
					{
						// здесь работаем с разделяемой переменной localDividend - выбранное значение массива, которое будет делиться на сотальные элементы массива
						var localDividend = 0;
						lock (_locker)
						{
							localDividend = _commonDividend;
							// увеличить _commonDividend на единицу
							_commonDividend++;
						}

						// просеиваем выбранный кандидат в простые числа localDividend с начала до конца массива. в каждом таске будет свое число, у которого будет проверяться делимость
						for (var t = 0; t < _initialArray.Length; t++)
						{
							if (_initialArray[t] == 0)
							{
								continue;
							}
							//последовательно делим выбранный элемент массива на каждый элемент массива, начиная сначала
							if (localDividend % _initialArray[t] == 0 && _initialArray[t] != localDividend) // исключить деление взятого числа само на себя
							{
								//по идее, лочить обращение к масиву здесь не нужно, т.к. мы в разных потока обращаемся к разным частям массива, т.к. разный индекс localDividend

								//если выбранный элемент массива делится на какой-либо элемент массива, кроме себя самого, то он не простое число
								//в данном случае localDividend является не только значением, но и индексом данного значения
								_initialArray[localDividend] = 0;
								break;
							}
						}
					}
				});
			}

			foreach (var task in tasks)
			{
				task.Start();
			}

			Task.WhenAll(tasks);

			stopwatch.Stop();
			
			Console.WriteLine("Метод InnerExecute завершен");

			Console.WriteLine("Время работы программы с потоками: " + stopwatch.Elapsed);

			if (printOnMonitor)
			{
				foreach (var result in _initialArray)
				{
					if (result == 0)
					{
						continue;
					}
					Console.Write(result + ", ");
				}
			}

			Console.WriteLine();
		}

		
	}
}
