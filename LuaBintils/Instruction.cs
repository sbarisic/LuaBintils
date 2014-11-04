using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LuaBin {
	public enum LType {
		None = -1,
		Nil = 0,
		Bool = 1,
		LightUserdata = 2,
		Number = 3,
		String = 4,
		Table = 5,
		Function = 6,
		Userdata = 7,
		Thread = 8,
	}

	public struct Instruction {
		public int I, A, B, C, Bx, sBx;
		public bool IsA, IsB, IsC, IsBx, IsBxSigned;
		public OpCode Code;

		public Instruction(int i) {
			I = i;

			A = ((i) >> POS_A) & MASK1(SIZE_A, 0);
			B = ((i) >> POS_B) & MASK1(SIZE_B, 0);
			C = ((i) >> POS_C) & MASK1(SIZE_C, 0);
			Bx = ((i) >> POS_Bx) & MASK1(SIZE_Bx, 0);
			sBx = (Bx - MAXARG_sBx);
			IsA = IsB = IsC = IsBx = IsBxSigned = false;

			Code = (OpCode)(((i) >> POS_OP) & MASK1(SIZE_OP, 0));
			AssignParams();
		}

		public void Save(BinaryWriter W) {
			W.Write(I);
		}

		public override string ToString() {
			string CodeStr = Code.ToString();
			CodeStr += new string(' ', 10 - CodeStr.Length);

			string Args = null;
			if (IsA)
				Args += ParamFormat("A", A);
			if (IsB)
				Args += ParamFormat("B", B);
			if (IsC)
				Args += ParamFormat("C", C);
			if (IsBx) {
				if (IsBxSigned)
					Args += ParamFormat("sBx", sBx);
				else
					Args += ParamFormat("Bx", Bx);
			}

			return string.Format("{0} {1}", CodeStr, Args).Trim();
		}

		internal void AssignParams() {
			switch (Code) {
				case OpCode.MOVE:
				case OpCode.LOADNIL:
				case OpCode.GETUPVAL:
				case OpCode.SETUPVAL:
				case OpCode.UNM:
				case OpCode.NOT:
				case OpCode.LEN:
				case OpCode.TEST:
				case OpCode.RETURN:
				case OpCode.VARARG:
					IsA = IsB = true;
					break;
				case OpCode.LOADK:
				case OpCode.GETGLOBAL:
				case OpCode.SETGLOBAL:
				case OpCode.CLOSURE:
					IsA = IsBx = true;
					break;
				case OpCode.LOADBOOL:
				case OpCode.GETTABLE:
				case OpCode.SETTABLE:
				case OpCode.NEWTABLE:
				case OpCode.SELF:
				case OpCode.ADD:
				case OpCode.SUB:
				case OpCode.MUL:
				case OpCode.DIV:
				case OpCode.MOD:
				case OpCode.POW:
				case OpCode.CONCAT:
				case OpCode.EQ:
				case OpCode.LT:
				case OpCode.LE:
				case OpCode.TESTSET:
				case OpCode.CALL:
				case OpCode.TAILCALL:
				case OpCode.SETLIST:
					IsA = IsB = IsC = true;
					break;
				case OpCode.JMP:
					IsBx = IsBxSigned = true;
					break;
				case OpCode.FORLOOP:
				case OpCode.FORPREP:
					IsA = IsBx = IsBxSigned = true;
					break;
				case OpCode.TFORLOOP:
					IsA = IsC = true;
					break;
				case OpCode.CLOSE:
					IsA = true;
					break;
				default:
					throw new Exception("Unknown opcode: " + Code.ToString());
			}
		}

		#region ==(Instruction, OpCode)
		public static bool operator ==(Instruction I, OpCode O) {
			return I.Code == O;
		}

		public static bool operator !=(Instruction I, OpCode O) {
			return I.Code != O;
		}
		#endregion

		internal static string ParamFormat(string Name, object Val) {
			return string.Format("{0} = {1}  ", Name, Val);
		}

		const int SIZE_C = 9;
		const int SIZE_B = 9;
		const int SIZE_Bx = (SIZE_B + SIZE_C);
		const int SIZE_A = 8;
		const int SIZE_OP = 6;

		const int POS_OP = 0;
		const int POS_A = (POS_OP + SIZE_OP);
		const int POS_C = (POS_A + SIZE_A);
		const int POS_B = (POS_C + SIZE_C);
		const int POS_Bx = POS_C;

		const int MAXARG_Bx = ((1 << SIZE_Bx) - 1);
		const int MAXARG_sBx = (MAXARG_Bx >> 1);

		const int BITRK = (1 << (SIZE_B - 1));

		internal static int CREATE_ABC(int O, int A, int B, int C) {
			return ((O << POS_OP) | (A << POS_A) | (B << POS_B) | (C << POS_C));
		}

		public static Instruction CreateABC(OpCode O, int A, int B, int C) {
			return new Instruction(CREATE_ABC((int)O, A, B, C));
		}

		internal static int CREATE_ABx(int O, int A, int Bx) {
			return ((O << POS_OP) | (A << POS_A) | (Bx << POS_Bx));
		}

		public static Instruction CreateABx(OpCode O, int A, int Bx) {
			return new Instruction(CREATE_ABx((int)O, A, Bx));
		}

		internal static int CREATE_AB(int O, int A, int B) {
			return ((O << POS_OP) | (A << POS_A) | (B << POS_B));
		}

		public static Instruction CreateAB(OpCode O, int A, int B) {
			return new Instruction(CREATE_AB((int)O, A, B));
		}

		internal static int MASK1(int n, int p) {
			return ((~((~0) << n)) << p);
		}

		internal static int MASK0(int n, int p) {
			return (~MASK1(n, p));
		}

		internal static bool IsConst(int x) {
			return ((x) & BITRK) > 0;
		}

		internal static int Indexk(int r) {
			return ((int)(r) & ~BITRK);
		}
	}
}