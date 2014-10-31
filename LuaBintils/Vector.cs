using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.IO;

namespace LuaBin {
	public class Vector<T> {
		public T[] Data;

		public T this[int i] {
			get {
				return Data[i];
			}
		}

		public void Push(T Item) {
			Array.Resize(ref Data, Data.Length + 1);
			Data[Data.Length - 1] = Item;
		}

		public T Pop() {
			T Item = Data[Data.Length - 1];
			Array.Resize(ref Data, Data.Length - 1);
			return Item;
		}

		public int Length {
			get {
				return Data.Length;
			}
		}
	}

	public class IntVector : Vector<int> {
		public IntVector(BinaryReader R) {
			Data = new int[R.ReadInt32()];
			for (int i = 0; i < Data.Length; i++)
				Data[i] = R.ReadInt32();
		}

		public IntVector(BinaryReader R, int Size)
			: this(R) {
		}

		public IntVector(int[] Ints) {
			Data = Ints;
		}

		public IntVector() {
			Data = new int[] { };
		}

		public void Save(BinaryWriter W) {
			W.Write(Data.Length);
			for (int i = 0; i < Data.Length; i++)
				W.Write(Data[i]);
		}
	}

	public class InstrVector : Vector<Instruction> {
		public InstrVector(BinaryReader R) {
			Data = new Instruction[R.ReadInt32()];
			for (int i = 0; i < Data.Length; i++)
				Data[i] = new Instruction(R.ReadInt32());
		}

		public InstrVector(BinaryReader R, int Size)
			: this(R) {
		}

		public InstrVector(Instruction[] Instrs) {
			Data = Instrs;
		}

		public InstrVector() {
			Data = new Instruction[] { };
		}

		public void Push(OpCode O, int A, int B, int C) {
			Push(Instruction.Create(O, A, B, C));
		}

		public void Push(OpCode O, int A, int Bx) {
			Push(Instruction.Create(O, A, Bx));
		}

		public void Save(BinaryWriter W) {
			W.Write(Data.Length);
			for (int i = 0; i < Data.Length; i++)
				Data[i].Save(W);
		}
	}
}