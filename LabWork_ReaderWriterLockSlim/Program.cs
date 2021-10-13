using System;

namespace LabWork_ReaderWriterLockSlim
{
	class Program
	{
		static void Main(string[] args)
		{
			//Инициализируем и запускаем модель работы с проектом из 2 разрабов, 3 тестеров и 5 программистов.
			Model model = new Model(2, 3, 5);
			model.ModelWorkWithProject();
			//Выводим историю работы над ПО
			Console.WriteLine(model.Programs);
			Console.WriteLine("писателей, ждущих в очереди - "
							  + model.Writers);
		}
	}
}
