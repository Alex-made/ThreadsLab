using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ThreadsLabWork
{
	class Program
	{
		static void Main(string[] args)
		{
			var commandHandler = new CommandHandler();
			commandHandler.Execute();
		}
	}

	class CommandHandler
	{
		private const double DoubleConst = 119274.0293847;
		private double[] _resultArray = new double[1000000];

		private object _locker = new object();

		public void Execute()
		{
			var stopwatch = Stopwatch.StartNew();
			InnerExecuteParallel(20);
			stopwatch.Stop();
			Console.WriteLine();
			Console.WriteLine("Время работы программы с потоками: " + stopwatch.Elapsed);

			stopwatch.Start(); 
			InnerExecute();
			stopwatch.Stop();
			Console.WriteLine();
			Console.WriteLine("Время работы программы синхронно: " + stopwatch.Elapsed);
		}

		private void InnerExecute()
		{

			var array = new double[1000000];
			var r = new Random(DateTime.Now.Millisecond);
			for (var i = 0; i < array.Length; i++)
			{
				array[i] = r.NextDouble();
			}

			for (var j = 0; j < array.Length; j++)
			{
				_resultArray[j] = Math.Sqrt(array[j] * DoubleConst * DoubleConst / 0.22);
			}
			
			//Console.WriteLine("Метод InnerExecute завершен");
			//foreach (var result in _resultArray)
			//{
			//	Console.Write(result);
			//}
		}

		private void InnerExecuteParallel(int threadsCount)
		{
			var array = new double[1000000];
			var r = new Random(DateTime.Now.Millisecond);
			for (var i = 0; i < array.Length; i++)
			{
				array[i] = r.NextDouble();
			}

			var tasks = new Task[threadsCount];

			for (var i = 0; i < tasks.Length; i++)
			{
				var taskNumber = i;
				tasks[taskNumber] = new Task(() =>
				{
					var startIndex = taskNumber * 50000;
					var arrayPart = array.Skip(startIndex).Take(50000).ToArray();
					for (var j = 0; j < arrayPart.Length; j++)
					{
						//lock (_locker) //по идее, локер не нужен, т.к. обращение к разным частям массива
						//{
							_resultArray[startIndex] = Math.Sqrt(arrayPart[j] * DoubleConst / 0.22);  //_result - разделяемая переменная	
							startIndex++;
						//}
					}
				});
			}

			foreach (var task in tasks)
			{
				task.Start();
			}

			Console.WriteLine("Метод InnerExecute завершен");
			foreach (var result in _resultArray)
			{
				Console.Write(result);
			}
		}
	}
}
