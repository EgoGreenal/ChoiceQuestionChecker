using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HandUpTest
{
	static class General
	{
		public static int prob, all;
		public static List<int> ps = new List<int>(), prob_count = new List<int>();
	}
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
