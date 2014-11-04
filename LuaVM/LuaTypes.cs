using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaBin;

namespace LuaVM {
	class LObject {
		public object Boxed;
		public LType LuaType;
		public static LObject Nil = new LObject(null, LType.Nil);

		public LObject(object Boxed, LType LuaType) {
			this.Boxed = Boxed;
			this.LuaType = LuaType;
		}

		public T As<T>() {
			if (Boxed == null)
				return default(T);
			return (T)Boxed;
		}

		public override int GetHashCode() {
			return LuaType.GetHashCode() ^ Boxed.GetHashCode();
		}

		public override bool Equals(object Obj) {
			LObject O = Obj as LObject;
			return O != null && LuaType.Equals(O.LuaType) && Boxed.Equals(O.Boxed);
		}

		public override string ToString() {
			if (Boxed == null)
				return "nil";
			return Boxed.ToString();
		}

		public static implicit operator LObject(int I) {
			return new LObject((double)I, LType.Number);
		}

		public static implicit operator LObject(float F) {
			return new LObject((double)F, LType.Number);
		}

		public static implicit operator LObject(double D) {
			return new LObject(D, LType.Number);
		}

		public static implicit operator LObject(Tuple<LType, object> T) {
			return new LObject(T.Item2, T.Item1);
		}

		public static implicit operator LObject(LFunction F) {
			return new LObject(F, LType.Function);
		}

		public static implicit operator LObject(Function F) {
			return new LObject(new LFunction(F), LType.Function);
		}

		public static implicit operator LObject(string Str) {
			return new LObject(Str, LType.String);
		}
	}

	class LTable {
		Dictionary<LObject, LObject> Tbl;

		public LTable() {
			Tbl = new Dictionary<LObject, LObject>();
		}

		public void Set(LObject Key, LObject Val) {
			if (Tbl.ContainsKey(Key))
				Tbl.Remove(Key);
			Tbl.Add(Key, Val);
		}

		public LObject Get(LObject Key) {
			if (Tbl.ContainsKey(Key))
				return Tbl[Key];
			return null;
		}
	}

	delegate void LFunc(LVM L, LFunction F);
	class LFunction {
		public int PC, Base, Top, RetPos, NArg, NRet;
		public Function Func;

		public bool Native;
		public LFunc NativeFunc;

		public int Length {
			get {
				if (Native)
					return 1;
				return Func.Code.Length;
			}
		}

		public LFunction(Function F, int PC = 0, int Base = 0, int Top = -1, int RetPos = 0) {
			Func = F;
			this.PC = PC;
			this.Base = Base;
			if (Top == -1)
				Top = F.MaxStackSize;
			this.Top = Top;
			this.RetPos = RetPos;
			Native = false;
		}

		public LFunction(LFunc F) {
			Native = true;
			NativeFunc = F;
			Top = 16;
		}

		public static implicit operator LFunction(LFunc F) {
			return new LFunction(F);
		}
	}

	static class Meta {
		public static LObject Add(LObject O1, LObject O2) {
			if (O1.LuaType == LType.Number && O2.LuaType == LType.Number)
				return new LObject((double)O1.Boxed + (double)O2.Boxed, LType.Number);

			throw new Exception("Can not add");
		}
	}
}