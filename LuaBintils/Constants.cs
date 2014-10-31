using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace LuaBin {
	public class Constants {
		public List<Tuple<LType, object>> List;
		public List<Function> Functions;

		public Constants(BinaryReader R) {
			int ListLen = R.ReadInt32();
			List = new List<Tuple<LType, object>>(ListLen);

			for (int i = 0; i < ListLen; i++) {
				LType T;
				switch (T = R.ReadLuaType()) {
					case LType.Nil:
						List.Add(new Tuple<LType, object>(T, null));
						break;
					case LType.Bool:
						List.Add(new Tuple<LType, object>(T, R.ReadBoolean()));
						break;
					case LType.Number:
						List.Add(new Tuple<LType, object>(T, R.ReadInt64()));
						break;
					case LType.String:
						List.Add(new Tuple<LType, object>(T, R.ReadLuaString()));
						break;
					default:
						Dbg.Assert(false, "Unexpected lua type in constants");
						break;
				}
			}

			int FuncsLen = R.ReadInt32();
			Functions = new List<Function>(FuncsLen);
			for (int i = 0; i < FuncsLen; i++)
				Functions.Add(new Function(R));
		}

		public Constants() {
			List = new List<Tuple<LType, object>>();
			Functions = new List<Function>();
		}

		public int Add(LType T, object Val) {
			Tuple<LType, object> Item = new Tuple<LType, object>(T, Val);
			List.Add(Item);
			return List.IndexOf(Item);
		}

		public void Add(Function F) {
			Functions.Add(F);
		}

		public void Save(BinaryWriter W) {
			W.Write(List.Count);
			for (int i = 0; i < List.Count; i++) {
				LType T = List[i].Item1;
				W.WriteLuaType(T);
				switch (T) {
					case LType.Bool:
						W.Write((bool)List[i].Item2);
						break;
					case LType.Number:
						W.Write((long)List[i].Item2);
						break;
					case LType.String:
						W.WriteLuaString((string)List[i].Item2);
						break;
				}
			}
			W.Write(Functions.Count);
			foreach (Function Func in Functions)
				Func.Save(W);
		}
	}
}