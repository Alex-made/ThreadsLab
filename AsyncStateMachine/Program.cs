using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStateMachine
{
	class Program
	{
		public static void Method()
		{
			for (int i = 0; i < 80; i++)
			{
				Thread.Sleep(10);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write($"Номер потока задачи в таске: {Thread.CurrentThread.ManagedThreadId}"); Console.Write("-");
			}
		}

		//помечаем ключевым словом async метод
		public async static Task MethodAsync()
		{
			Console.WriteLine($"Номер потока текущего метода MethodAsync: {Thread.CurrentThread.ManagedThreadId}");
			//Эта часть кода будет работать в основном потоке
			//создаем задачу передаем в делегат метод Method
			Task t = new Task(Method);
			//запускаем задачу
			t.Start();

			//а вот эта часть завершится в 2 потоке (в том, котором она запустилась)
			//ожидаем завершения задачи
			await t;
		}


		static void Main(string[] args)
		{
			MethodAsync().GetAwaiter().GetResult();
			Console.WriteLine("Main завершился");
			Console.ReadKey();
		}
    }
}
