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
						List.Add(new Tuple<LType, object>(T, R.ReadDouble()));
						break;
					case LType.String:
						List.Add(new Tuple<LType, object>(T, R.ReadLuaString()));
						break;
					default:
						throw new Exception("Type " + T.ToString() + " not implemented");
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

		public Tuple<LType, object> this[int Idx] {
			get {
				return List[Idx];
			}
		}

		public int Add(LType T, object Val) {
			Tuple<LType, object> Item = new Tuple<LType, object>(T, Val);
			if (List.Contains(Item))
				return List.IndexOf(Item);
			List.Add(Item);
			return List.IndexOf(Item);
		}

		public int Add(object O) {
			if (O is string)
				return Add(LType.String, O);
			else if (O is int || O is float || O is double) {
				if (O is int)
					O = (double)(int)O;
				else if (O is float)
					O = (double)(float)O;
				return Add(LType.Number, O);
			} else if (O == null)
				return Add(LType.Nil, null);
			else if (O is bool)
				return Add(LType.Bool, O);

			throw new Exception("Not implemented, can't add constant");
		}

		public void AddFunc(Function F) {
			Functions.Add(F);
		}

		public void Save(BinaryWriter W) {
			W.Write(List.Count);
			for (int i = 0; i < List.Count; i++) {
				LType T = List[i].Item1;
				W.WriteLuaType(T);
				switch (T) {
					case LType.Nil:
						break;
					case LType.Bool:
						W.Write((bool)List[i].Item2);
						break;
					case LType.Number:
						W.Write((double)List[i].Item2);
						break;
					case LType.String:
						W.WriteLuaString((string)List[i].Item2);
						break;
					default:
						throw new Exception("Type " + T.ToString() + " not implemented");
				}
			}
			W.Write(Functions.Count);
			foreach (Function Func in Functions)
				Func.Save(W);
		}
	}
}