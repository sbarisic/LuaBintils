using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaBin.API {
	public class Lua {
		public Function Func;

		int _Idx;
		int Idx {
			get {
				return _Idx;
			}
			set {
				_Idx = value;
				if (FreeSlot < _Idx)
					FreeSlot = _Idx;
				if (_Idx > Func.MaxStackSize)
					Func.MaxStackSize = (byte)(_Idx);
			}
		}

		int FreeSlot;

		public Lua(Function F) {
			this.Func = F;
		}

		public void Move(int From, int To) {
			Func.Code.Push(Instruction.CreateAB(OpCode.MOVE, To, From));
		}

		public void Swap(int A, int B) {
			Move(A, FreeSlot);
			Move(B, A);
			Move(FreeSlot, B);
			if (FreeSlot > Func.MaxStackSize)
				Func.MaxStackSize = (byte)FreeSlot;
		}

		#region GetGlobal
		public void GetGlobal(int Idx, object O) {
			int KIdx = Func.Constants.Add(O);
			Func.Code.Push(Instruction.CreateABx(OpCode.GETGLOBAL, Idx, KIdx));
		}

		public void GetGlobal(object O) {
			GetGlobal(Idx++, O);
		}
		#endregion

		#region Load
		public void Load(int Idx, object O) {
			if (O == null)
				LoadNil(Idx, Idx);
			else if (O is bool)
				LoadBool(Idx, (bool)O);
			else
				LoadK(Idx, O);
		}

		public void Load(object O) {
			Load(Idx++, O);
		}

		public void LoadK(int Idx, object O) {
			int KIdx = Func.Constants.Add(O);
			Func.Code.Push(Instruction.CreateABx(OpCode.LOADK, Idx, KIdx));
		}

		public void LoadK(object O) {
			LoadK(Idx++, O);
		}

		public void LoadBool(int Idx, bool B, bool IncPC = false) {
			Func.Code.Push(Instruction.CreateABC(OpCode.LOADBOOL, Idx, B ? 1 : 0, IncPC ? 1 : 0));
		}

		public void LoadNil(int Idx, int B) {
			Func.Code.Push(Instruction.CreateAB(OpCode.LOADNIL, Idx, B));
		}
		#endregion

		public void Call(int Idx, int Funct, int Rets) {
			Func.Code.Push(Instruction.CreateABC(OpCode.CALL, Idx, Funct, Rets));
			this.Idx = Idx;
		}

		public void Return(int StartIdx = 0, int Count = 0) {
			Func.Code.Push(Instruction.CreateAB(OpCode.RETURN, StartIdx, Count));
		}
	}
}