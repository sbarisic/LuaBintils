using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiscUtil.IO;

namespace LuaBin {
	static class Dbg {
		public static void Assert(bool B, string Msg = "Assertion failed") {
			if (!B)
				throw new Exception(Msg);
		}

		public static string ReadString(this EndianBinaryReader R, int Len) {
			if (Len == 0)
				return "";
			char[] S = new char[Len - 1];
			for (int i = 0; i < Len - 1; i++)
				S[i] = (char)R.ReadByte();
			Dbg.Assert(R.ReadByte() == 0x0, "Null terminator expected");
			return new string(S);
		}

		public static LType ReadLuaType(this EndianBinaryReader R) {
			return (LType)R.ReadByte();
		}

		public static void WriteLuaType(this EndianBinaryWriter W, LType T) {
			W.Write((byte)T);
		}

		public static string ReadLuaString(this EndianBinaryReader R) {
			return R.ReadString(R.ReadInt32());
		}

		public static void WriteLuaString(this EndianBinaryWriter W, string Str) {
			W.Write(Str.Length + 1);
			for (int i = 0; i < Str.Length; i++)
				W.Write((byte)Str[i]);
			W.Write((byte)0x0);
		}
	}
}