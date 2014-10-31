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
		public int I;

		public int A, B, C, Bx, sBx;
		public OpCode Code;

		public Instruction(int i) {
			I = i;

			A = ((i) >> POS_A) & MASK1(SIZE_A, 0);
			B = ((i) >> POS_B) & MASK1(SIZE_B, 0);
			C = ((i) >> POS_C) & MASK1(SIZE_C, 0);
			Bx = ((i) >> POS_Bx) & MASK1(SIZE_Bx, 0);
			sBx = (Bx - MAXARG_sBx);

			Code = (OpCode)(((i) >> POS_OP) & MASK1(SIZE_OP, 0));
		}

		public void Save(BinaryWriter W) {
			W.Write(I);
		}

		public override string ToString() {
			return Code.ToString();
		}

		public static Instruction Create(OpCode Code) {
			return new Instruction(CREATE_ABC((int)Code, 0, 0, 0));
		}

		public static Instruction Create(OpCode Code, int A, int Bx) {
			return new Instruction(CREATE_ABx((int)Code, A, Bx));
		}

		public static Instruction Create(OpCode Code, int A, int B, int C) {
			return new Instruction(CREATE_ABC((int)Code, A, B, C));
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

		internal static int CREATE_ABx(int O, int A, int Bx) {
			return ((O << POS_OP) | (A << POS_A) | (Bx << POS_Bx));
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