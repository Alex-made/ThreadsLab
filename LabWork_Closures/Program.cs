using System;
using System.Collections.Generic;
using System.Linq;

namespace LabWork_Closures
{
	internal class Program
	{
		#region Private
		private static void Main(string[] args)
		{
			ClosureOnReferenceType();
			Console.WriteLine();
			ClosureOnSignificantType();
			Console.ReadLine();
		}

		private static void ClosureOnReferenceType()
		{
			var str = "Initial value";

			Action action = () =>
			{
				Console.WriteLine(str);
				str = "Modified by closure";
			};

			str = "After delegate creation";
			action();
			Console.WriteLine(str);
		}

		private static void ClosureOnSignificantType()
		{
			var digit = 1;

			Action action = () =>
			{
				Console.WriteLine(digit);
				digit = 222;
			};

			digit = 3;
			action();
			Console.WriteLine(digit);
		}
		#endregion
	}
}
