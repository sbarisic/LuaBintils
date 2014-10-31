using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace LuaBin {
	public class Vector<T> {
		public T[] Data;

		public T this[int i] {
			get {
				return Data[i];
			}
		}

		public int Length {
			get {
				return Data.Length;
			}
		}
	}

	public class IntVector : Vector<int> {
		public IntVector(EndianBinaryReader R) {
			Data = new int[R.ReadInt32()];
			for (int i = 0; i < Data.Length; i++)
				Data[i] = R.ReadInt32();
		}

		public IntVector(EndianBinaryReader R, int Size)
			: this(R) {
		}

		public IntVector(int[] Ints) {
			Data = Ints;
		}

		public void Save(EndianBinaryWriter W) {
			W.Write(Data.Length);
			for (int i = 0; i < Data.Length; i++)
				W.Write(Data[i]);
		}
	}

	public class InstrVector : Vector<Instruction> {
		public InstrVector(EndianBinaryReader R) {
			Data = new Instruction[R.ReadInt32()];
			for (int i = 0; i < Data.Length; i++)
				Data[i] = new Instruction(R.ReadInt32());
		}

		public InstrVector(EndianBinaryReader R, int Size)
			: this(R) {
		}

		public InstrVector(Instruction[] Instrs) {
			Data = Instrs;
		}

		public void Save(EndianBinaryWriter W) {
			W.Write(Data.Length);
			for (int i = 0; i < Data.Length; i++)
				Data[i].Save(W);
		}
	}
}