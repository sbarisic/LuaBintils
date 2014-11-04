using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaBin;

namespace LuaVM {
	class Program {
		static void Main(string[] args) {
			Console.Title = "LuaVM";
			LVM VM = new LVM(new LuaChunk("luac.out"));

			VM.Global.Set("print", new LFunction((L, Self) => {
				StringBuilder SB = new StringBuilder();
				for (int i = 0; i < Self.NArg; i++)
					SB.AppendFormat("{0}\t", L.GetReg(i));
				Console.WriteLine(SB.ToString().Trim());
			}));

			VM.Run();
			Console.WriteLine("\nComplete");
			Console.ReadLine();
		}
	}
}