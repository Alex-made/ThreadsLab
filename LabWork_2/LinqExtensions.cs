using System;
using System.Linq;
using System.Threading.Tasks;

namespace LabWork_2
{
	public static class LinqExtensions
	{
		public static int[] FindSimple(this int[] initialArray, int topEdge)
		{
			//тут деление на этапы нецелесообрано, но оставил, т.к. унифицировано с параллельным выводом


			// 1 этап - нахождение чисел от 2 до корня из topSimpleDigit включительно
			
			// просеиваем простые числа от начала до корня из topSimpleDigit (arrayBoundary) не включая само arrayBoundary
			var arrayBoundary = (int)Math.Sqrt(topEdge);
			for (var i = 0; i < arrayBoundary; i++)
			{
				if (initialArray[i] == 0)
				{
					continue;
				}
				//находим число, которое будем последовательно пробовать делить на каждый элемент массива
				var dividend = initialArray[i];
				for (var j = 0; j < arrayBoundary; j++)
				{
					if (initialArray[j] == 0)
					{
						continue;
					}
					if (dividend % initialArray[j] == 0 && initialArray[j] != dividend) // исключить деление взятого числа само на себя
					{
						initialArray[i] = 0;
						break;
					}
				}
			}

			// просеиваем простые числа от точки разделения массива до конца массива.
			for (var j = arrayBoundary; j < initialArray.Length; j++)
			{
				if (initialArray[j] == 0)
				{
					continue;
				}

				var dividend = initialArray[j];

				for (var t = 0; t < initialArray.Length; t++)
				{
					if (initialArray[t] == 0)
					{
						continue;
					}
					//последовательно делим выбранный элемент массива на каждый элемент массива, начиная сначала
					if (dividend % initialArray[t] == 0 && initialArray[t] != dividend) // исключить деление взятого числа само на себя
					{
						//если выбранный элемент массива делится на какой-либо элемент массива, кром себя самого, то он не простое число
						initialArray[j] = 0;
						break;
					}
				}
			}

			return initialArray.Where(element => element != 0).ToArray();
		}

		//вопрос - а сколько брать потоков? и делать ли async?
		public static int[] FindSimpleAsync(this int[] initialArray, int topEdge)
		{
			// 1 этап - нахождение чисел от 2 до корня из topSimpleDigit включительно

			// просеиваем простые числа от начала до корня из topSimpleDigit (arrayBoundary) не включая само arrayBoundary
			var arrayBoundary = (int)Math.Sqrt(topEdge);
			for (var i = 0; i < arrayBoundary; i++)
			{
				if (initialArray[i] == 0)
				{
					continue;
				}
				//находим число, которое будем последовательно пробовать делить на каждый элемент массива
				var dividend = initialArray[i];
				for (var j = 0; j < arrayBoundary; j++)
				{
					if (initialArray[j] == 0)
					{
						continue;
					}
					if (dividend % initialArray[j] == 0 && initialArray[j] != dividend) // исключить деление взятого числа само на себя
					{
						initialArray[i] = 0;
						break;
					}
				}
			}

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
					while (_commonDividend < initialArray.Length)
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
						for (var t = 0; t < initialArray.Length; t++)
						{
							if (_initialArray[t] == 0)
							{
								continue;
							}
							//последовательно делим выбранный элемент массива на каждый элемент массива, начиная сначала
							if (localDividend % initialArray[t] == 0 && initialArray[t] != localDividend) // исключить деление взятого числа само на себя
							{
								//по идее, лочить обращение к масиву здесь не нужно, т.к. мы в разных потока обращаемся к разным частям массива, т.к. разный индекс localDividend

								//если выбранный элемент массива делится на какой-либо элемент массива, кром себя самого, то он не простое число
								//в данном случае localDividend является не только значением, но и индексом данного значения
								initialArray[localDividend] = 0;
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

			return initialArray.Where(element => element != 0).ToArray();
		}
	}
}
