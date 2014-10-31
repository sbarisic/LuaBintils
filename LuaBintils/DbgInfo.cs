using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LuaBin {
	public class DbgInfo {
		public IntVector LineInfo;
		public Tuple<string, int, int>[] LocalVars;
		public string[] UpValues;

		public DbgInfo(BinaryReader R) {
			LineInfo = new IntVector(R, sizeof(int));
			LocalVars = new Tuple<string, int, int>[R.ReadInt32()];
			for (int i = 0; i < LocalVars.Length; i++)
				LocalVars[i] = new Tuple<string, int, int>(R.ReadLuaString(),
					R.ReadInt32() + 1, R.ReadInt32() + 1);

			UpValues = new string[R.ReadInt32()];
			for (int i = 0; i < UpValues.Length; i++)
				UpValues[i] = R.ReadLuaString();
		}

		public DbgInfo() {
			LineInfo = new IntVector(new int[] { });
			LocalVars = new Tuple<string, int, int>[] { };
			UpValues = new string[] { };
		}

		public void Save(BinaryWriter W) {
			LineInfo.Save(W);
			W.Write(LocalVars.Length);
			for (int i = 0; i < LocalVars.Length; i++) {
				W.WriteLuaString(LocalVars[i].Item1);
				W.Write(LocalVars[i].Item2 - 1);
				W.Write(LocalVars[i].Item3 - 1);
			}
			W.Write(UpValues.Length);
			for (int i = 0; i < UpValues.Length; i++)
				W.WriteLuaString(UpValues[i]);
		}
	}
}
