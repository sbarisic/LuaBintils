using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaBin;

namespace LuaVM {
	class LVM {
		LuaChunk Chunk;
		public LTable Global, Env, Registry;
		LObject[] Reg;
		LFunction[] Funcs;
		int FuncPtr;

		LFunction LFunc {
			get {
				return Funcs[FuncPtr];
			}
			set {
				Funcs[FuncPtr] = value;
			}
		}

		Function Func {
			get {
				return Funcs[FuncPtr].Func;
			}
			set {
				Funcs[FuncPtr].Func = value;
			}
		}

		int PC {
			get {
				return Funcs[FuncPtr].PC;
			}
			set {
				Funcs[FuncPtr].PC = value;
			}
		}

		int Base {
			get {
				return Funcs[FuncPtr].Base;
			}
			set {
				Funcs[FuncPtr].Base = value;
			}
		}

		int Top {
			get {
				return Funcs[FuncPtr].Top;
			}
			set {
				Funcs[FuncPtr].Top = value;
			}
		}

		public LVM(LuaChunk C) {
			Chunk = C;
			Funcs = new LFunction[255];
			LFunc = new LFunction(C.Functions[0], 0, 0, C.Functions[0].MaxStackSize);
			Reg = new LObject[255];

			Global = new LTable();
			Env = new LTable();
			Registry = new LTable();
		}

		public int CheckIdx(int Idx) {
			if (Idx > Top) {
				Top = Idx;
				return CheckIdx(Idx);
			}
			if (Idx < 0) {
				Idx = Top - Idx;
				if (Idx < 0)
					throw new Exception("Stack underflow");
			}
			return Base + Idx;
		}

		public void SetReg(int Idx, LObject Val, bool Raw = false) {
			if (!Raw)
				Idx = CheckIdx(Idx);
			Reg[Idx] = Val;
		}

		public LObject GetReg(int Idx, bool Raw = false) {
			if (!Raw)
				Idx = CheckIdx(Idx);
			if (Reg[Idx] == null)
				return LObject.Nil;
			return Reg[Idx];
		}

		public void Call(LFunction F, int A, int NArg, int NRet, bool Tail) {
			if (F == null)
				throw new Exception("Tried to call a null value");
			F.PC = 0;
			F.Base = CheckIdx(A) + 1;
			F.RetPos = CheckIdx(A);
			F.NArg = NArg;
			F.NRet = NRet;
			FuncPtr++;
			LFunc = F;
		}

		public void Return(LFunction F) {
			FuncPtr--;
			if (FuncPtr < 0)
				FuncPtr++;
			for (int i = 0; i < F.NArg + F.Top; i++)
				SetReg(F.RetPos + i, LObject.Nil);
		}

		public void Run() {
			while (PC < LFunc.Length)
				Step();
		}

		public void Step() {
			if (LFunc.Native) {
				LFunc.NativeFunc(this, LFunc);
				Return(LFunc);
				return;
			}

			Instruction I = Func.Code[PC++];

			LObject RKB = GetReg(I.B);
			LObject RKC = GetReg(I.C);
			if (I.B >= (1 << 8))
				RKB = Func.Constants[I.B - (1 << 8)];
			if (I.C >= (1 << 8))
				RKC = Func.Constants[I.C - (1 << 8)];

			if (I == OpCode.MOVE) {
			} else if (I == OpCode.LOADK) {
				SetReg(I.A, Func.Constants[I.Bx]);
			} else if (I == OpCode.LOADBOOL) {
			} else if (I == OpCode.LOADNIL) {
			} else if (I == OpCode.GETUPVAL) {
			} else if (I == OpCode.GETGLOBAL) {
				SetReg(I.A, Global.Get(Func.Constants[I.Bx]));
			} else if (I == OpCode.GETTABLE) {
			} else if (I == OpCode.SETGLOBAL) {
				Global.Set(Func.Constants[I.Bx], GetReg(I.A));
			} else if (I == OpCode.SETUPVAL) {
			} else if (I == OpCode.SETTABLE) {
			} else if (I == OpCode.NEWTABLE) {
			} else if (I == OpCode.SELF) {
			} else if (I == OpCode.ADD) {
				SetReg(I.A, Meta.Add(RKB, RKC));
			} else if (I == OpCode.SUB) {
			} else if (I == OpCode.MUL) {
			} else if (I == OpCode.DIV) {
			} else if (I == OpCode.MOD) {
			} else if (I == OpCode.POW) {
			} else if (I == OpCode.UNM) {
			} else if (I == OpCode.NOT) {
			} else if (I == OpCode.LEN) {
			} else if (I == OpCode.CONCAT) {
			} else if (I == OpCode.JMP) {
			} else if (I == OpCode.EQ) {
			} else if (I == OpCode.LT) {
			} else if (I == OpCode.LE) {
			} else if (I == OpCode.TEST) {
			} else if (I == OpCode.TESTSET) {
			} else if (I == OpCode.CALL) {
				Call(GetReg(I.A).As<LFunction>(), I.A, I.B == 0 ? I.B - 1 : I.B - 1, I.C, false);
			} else if (I == OpCode.TAILCALL) {
			} else if (I == OpCode.RETURN) {
				Return(LFunc);
			} else if (I == OpCode.FORLOOP) {
			} else if (I == OpCode.FORPREP) {
			} else if (I == OpCode.TFORLOOP) {
			} else if (I == OpCode.SETLIST) {
			} else if (I == OpCode.CLOSE) {
			} else if (I == OpCode.CLOSURE) {
				SetReg(I.A, Func.Constants.Functions[I.B]);
			} else if (I == OpCode.VARARG) {
			} else
				throw new Exception("Instruction not implemented: " + I.ToString());

			//Console.WriteLine(I);
		}
	}
}