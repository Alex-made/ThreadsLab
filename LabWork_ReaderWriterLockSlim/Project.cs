using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LabWork_ReaderWriterLockSlim
{
	/// <summary>
	/// Программный проект, над которым работают
	/// (developers), (testers), (programmers)
	/// (писатели), (редакторы), (читатели) 
	/// </summary>
	public class Project
	{
		string program = "";
		string Program
		{
			get { return program; }
		}
		ReaderWriterLockSlim wer = new ReaderWriterLockSlim();

		/// <summary>
		///For Writers 
		/// </summary>
		/// <param name="prog">новая версия программы</param>
		public void WriteNewProgram(string prog)
		{
			//Когда мы вызываем EnterWriteLock, выполняемый поток встанет в очередь с пометкой Write и будет ждать своего выполнения
			wer.EnterWriteLock();
			try
			{
				program = prog;
				Thread.Sleep(10);
			}
			finally
			{
				wer.ExitWriteLock();
			}
		}

		/// <summary>
		///For Editors 
		/// </summary>
		/// <param name="prog">новое изменение программы</param>
		public void EditNewProgram(string patch, ref int writers)
		{
			//Когда мы вызываем EnterWriteLock, выполняемый поток встанет в очередь с пометкой Write и будет ждать своего выполнения
			wer.EnterUpgradeableReadLock();
			try
			{
				if (program != "")
					program += patch;
				Thread.Sleep(10);
				if (wer.WaitingWriteCount > writers)
					writers = wer.WaitingWriteCount;
			}
			finally
			{
				wer.ExitUpgradeableReadLock();
			}
		}

		/// <summary>
		///For Readers 
		/// </summary>
		/// <param name="prog">чтение программы</param>
		public string ReadingProgram()
		{
			string p;
			//Когда мы вызываем EnterReadLock, выполняемый поток будет ждать, когда закончится очередь Write и может параллельно читать ресурс
			wer.EnterReadLock();
			try
			{
				p = program;
				Thread.Sleep(10);
			}
			finally
			{
				wer.ExitReadLock();
			}
			return p;
		}
	}
}
