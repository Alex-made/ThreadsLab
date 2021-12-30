using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWork_Intermediate
{
	//Объект Barrier приостанавливает параллельное выполнение отдельных задач, пока все задачи не достигнут этого барьера.
	//На первом этапе каждая задача заполняет данными свой сегмент буфера.Каждая задача, которая завершает заполнение своего сегмента,
	//сообщает барьеру о готовности и переходит в режим ожидания.Когда все задачи отправят барьеру сигналы, блокировка снимается и начинается второй этап.
	//Этот барьер нужен для того, чтобы каждая задача на втором этапе получила доступ ко всем данным, созданным до этого момента.
	//Без этого барьера первая задача, выполнившая свою работу, может начать чтение из буферов, которые еще не заполнены другими задачами.
	//Этот подход позволяет синхронизировать любое количество этапов.
	public class UseBarrier
	{
		public void Run()
		{
			const int numberTasks = 2;
			const int partitionSize = 1000000;
			var data = new List<string>(FillData(partitionSize * numberTasks));

			//Инициализируем барьер числом тасков + сам метод run тоже является участником
			var barrier = new System.Threading.Barrier(numberTasks + 1);

			var taskFactory = new TaskFactory();
			var tasks = new Task<int[]>[numberTasks];
			for (int i = 0; i < numberTasks; i++)
			{
				tasks[i] = taskFactory.StartNew<int[]>(CalculationInTask,
					Tuple.Create(i, partitionSize, barrier, data));
			}
			barrier.SignalAndWait();

			//объединяем результаты тасков
			var resultCollection = tasks[0].Result.Zip(tasks[1].Result, (c1, c2) =>
			{
				return c1 + c2;
			});
			char ch = 'a';
			int sum = 0;
		}

		public static IEnumerable<string> FillData(int size)
		{
			List<string> data = new List<string>(size);
			Random r = new Random();
			for (int i = 0; i < size; i++)
				data.Add(GetString(r));
			return data;
		}

		private static string GetString(Random r)
		{
			StringBuilder sb = new StringBuilder(6);
			for (int i = 0; i < 6; i++)
				sb.Append((char)(r.Next(26) + 97));
			return sb.ToString();
		}

		static int[] CalculationInTask(object p)
		{
			var p1 = p as Tuple<int, int, System.Threading.Barrier, List<string>>;
			System.Threading.Barrier barrier = p1.Item3;
			List<string> data = p1.Item4;

			int start = p1.Item1 * p1.Item2;
			int end = start + p1.Item2;

			Console.WriteLine("Задача {0}: раздел от {1} до {2}",
				Task.CurrentId, start, end);

			int[] charCount = new int[26];
			for (int j = start; j < end; j++)
			{
				char c = data[j][0];
				charCount[c - 97]++;
			}

			Console.WriteLine("Задача {0} завершила вычисление. {1} раз а, {2} раз z",
				Task.CurrentId, charCount[0], charCount[25]);
			barrier.RemoveParticipant();
			Console.WriteLine("Задача {0} удалена; количество оставшихся участников: {1}",
				Task.CurrentId, barrier.ParticipantsRemaining);
			return charCount;
		}
    }
}
