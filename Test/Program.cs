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

			LuaChunk C = new LuaChunk();

			Function F = new Function(new Instruction[] {
				new Instruction(OpCode.GETGLOBAL),
				new Instruction(OpCode.LOADK, 1, 0, 1, 1),
				new Instruction(OpCode.CALL, 0, 2, 1, 1025),
				new Instruction(OpCode.RETURN, 0, 1, 0, 512),
			});

			F.Constants.Add(LType.String, "print");
			F.Constants.Add(LType.String, "Hello World!");

			C.Functions.Add(F);
			C.Save("lua.out");

			Console.WriteLine("Complete");
			Console.ReadLine();
		}
	}
}