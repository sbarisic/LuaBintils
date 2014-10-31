using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.IO;

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

		internal void Init(int i) {
			I = i;

			A = ((i) >> POS_A) & MASK1(SIZE_A, 0);
			B = ((i) >> POS_B) & MASK1(SIZE_B, 0);
			C = ((i) >> POS_C) & MASK1(SIZE_C, 0);
			Bx = ((i) >> POS_Bx) & MASK1(SIZE_Bx, 0);
			sBx = (Bx - MAXARG_sBx);

			Code = (OpCode)(((i) >> POS_OP) & MASK1(SIZE_OP, 0));
		}

		public Instruction(int i) {
			this.Code = (OpCode)(this.A = this.B = this.C = this.Bx = this.sBx = this.I = 0);
			Init(i);
		}

		public Instruction(OpCode Code, int A, int B, int C, int Bx) {
			this.Code = (OpCode)(this.A = this.B = this.C = this.Bx = this.sBx = this.I = 0);
			Init(CREATE_ABCBx((int)Code, A, B, C, Bx));
		}

		public Instruction(OpCode Code, int A, int B, int C) {
			this.Code = (OpCode)(this.A = this.B = this.C = this.Bx = this.sBx = this.I = 0);
			Init(CREATE_ABCBx((int)Code, A, B, C, 0));
		}

		public Instruction(OpCode Code, int A, int B) {
			this.Code = (OpCode)(this.A = this.B = this.C = this.Bx = this.sBx = this.I = 0);
			Init(CREATE_ABCBx((int)Code, A, B, 0, 0));
		}

		public Instruction(OpCode Code) {
			this.Code = (OpCode)(this.A = this.B = this.C = this.Bx = this.sBx = this.I = 0);
			Init(CREATE_ABCBx((int)Code, 0, 0, 0, 0));
		}

		public void Save(EndianBinaryWriter W) {
			W.Write(I);
		}

		public override string ToString() {
			return Code.ToString();
		}

		const int SIZE_C = 9;
		const int SIZE_B = 9;
		const int SIZE_Bx = (SIZE_C + SIZE_B);
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

		public static int CREATE_ABCBx(int O, int A, int B, int C, int Bx) {
			return ((O << POS_OP) | (A << POS_A) | (B << POS_B) | (C << POS_C) | (Bx << POS_Bx));
		}
		public static int MASK1(int n, int p) {
			return ((~((~0) << n)) << p);
		}

		public static int MASK0(int n, int p) {
			return (~MASK1(n, p));
		}

		public static bool IsConst(int x) {
			return ((x) & BITRK) > 0;
		}

		public static int Indexk(int r) {
			return ((int)(r) & ~BITRK);
		}
	}
}