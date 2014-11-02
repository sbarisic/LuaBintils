using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaBin;
using LuaBin.API;

namespace Test {
	class Program {
		static void Main(string[] args) {
			Console.Title = "LuaBintils Test";

			/*
			{
				LuaChunk LC = new LuaChunk("luac.out");
				Console.WriteLine(LC.Functions[0].ToString2());
				Console.ReadLine();
				return;
			}
			//*/

			LuaChunk MainChunk = new LuaChunk();

			Lua L = new Lua(MainChunk.CreateFunction(0, "@luac.lua"));
			L.Load("Hello World!");
			L.GetGlobal("print");
			L.Swap(0, 1);
			L.Call(0, 2, 1);
			L.Return(0, 1);

			L.Func.MaxStackSize = 50;
			MainChunk.Save("lua.out");

			Console.WriteLine(MainChunk.Functions[0]);
			Console.ReadLine();
		}
	}
}