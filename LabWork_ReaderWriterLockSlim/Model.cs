using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LabWork_ReaderWriterLockSlim
{
	/// <summary>
	/// Моделирование работы над программным проектом.
	/// Каждый раз, когда одному из участников необходимо выполнить свою работу будет создаваться поток, выполняющий эту задачу.
	/// </summary>
	public class Model
	{
		/// <summary>
		/// Число разрабов, тестеров, программистов и суммарное число участников
		/// </summary>
		int ndev, ntes, nprog, n;
		
		/// <summary>
		/// история разработки проекта
		/// </summary>
		string programs;
		
		/// <summary>
		/// интенсивность разработки
		/// </summary>
		int writers;
		
        /// <summary>
        /// Проект, над которым работают.
        /// </summary>
		Project project;

        /// <summary>
        /// Просто константа для увеличения числа потоков, создаваемых при работе <see cref="Model"/>.
        /// </summary>
		const int rep = 5;
        
		/// <summary>
        /// Разделитель для строки с историей изменений ПО.
        /// </summary>
		const string nl = "\r\n";
		Random rnd = new Random();
		Thread[] threads;

		/// <summary>
		/// Число разрабов, тестеров, программистов
		/// </summary>
		/// <param name="nd"></param>
		/// <param name="nt"></param>
		/// <param name="np"></param>
		public Model(int nd, int nt, int np)
		{
			n = nd + nt + np;
			this.ndev = nd;
			this.ntes = nt;
			this.nprog = np;
			programs = "";
			project = new Project();
			threads = new Thread[n * rep];
		}

		public string Programs => programs;

		public int Writers => writers;

        // <summary>
        /// Реализация сценария работы с проектом
        /// </summary>
        public void ModelWorkWithProject()
        {
            int num = 0;
            int version = 1;
            int patch = 1;
            for (int i = 0; i < n * rep; i++)
            {
                //Генерация случайного события
                num = rnd.Next(n);
                if (num < ndev)
                {
                    //Создается поток разработчиков
                    threads[i] = new Thread(() =>
                    {
                        project.WriteNewProgram(String.Format(
                        "version {0}  from Devoleper {1} ", version, num));
                    });
                    threads[i].Start();
                    version++;
                }
                else
                    if (num < ndev + ntes)
                {
                    //Создается поток тестеров
                    threads[i] = new Thread(() =>
                    {
                        project.EditNewProgram(String.Format(
                        " patch {0}  from Tester {1}", patch, num),
                    ref writers);
                    });
                    threads[i].Start();
                    patch++;
                }
                else
                {
                    //Создается поток программистов
                    threads[i] = new Thread(() =>
                    {
                        programs += nl + project.ReadingProgram();
                    });
                    threads[i].Start();
                }
                Thread.Sleep(5);
            }
            for (int i = 0; i < n * rep; i++)
            {
                threads[i].Join();
            }
        }
    }
}
