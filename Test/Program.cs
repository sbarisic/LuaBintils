using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaBin;

namespace Test {
	class Program {
		static void Main(string[] args) {
			Console.Title = "LuaBintils Test";

			LuaChunk MainChunk = new LuaChunk();
			Function MainFunc = MainChunk.CreateFunction(2);

			int PrintIdx = MainFunc.Push(LType.String, "print");
			int Str1Idx = MainFunc.Push(LType.String, "Hello World #1!");
			int Str2Idx = MainFunc.Push(LType.String, "Hello World #2!");

			MainFunc.Push(OpCode.GETGLOBAL, 0, PrintIdx);
			MainFunc.Push(OpCode.LOADK, 1, Str1Idx);
			MainFunc.Push(OpCode.CALL, 0, 2, 1);
			MainFunc.Push(OpCode.GETGLOBAL, 0, PrintIdx);
			MainFunc.Push(OpCode.LOADK, 1, Str2Idx);
			MainFunc.Push(OpCode.CALL, 0, 2, 1);
			MainFunc.Push(OpCode.RETURN, 0, 0);

			MainChunk.Save("lua.out");

			Console.WriteLine("Complete");
			Console.ReadLine();
		}
	}
}