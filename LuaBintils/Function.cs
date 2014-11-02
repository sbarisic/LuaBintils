using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


namespace LuaBin {
	public class Function {
		public string Src;
		public int LineDefined, LastLineDefined;
		public bool IsVararg;
		public byte NUps, NumParams, MaxStackSize;
		public InstrVector Code;
		public int CodeOffset;
		public Constants Constants;
		public DbgInfo DebugInfo;

		public Function(BinaryReader R) {
			Src = R.ReadLuaString();
			LineDefined = R.ReadInt32();
			LastLineDefined = R.ReadInt32();

			NUps = R.ReadByte();
			NumParams = R.ReadByte();
			IsVararg = R.ReadBoolean();
			MaxStackSize = R.ReadByte();

			CodeOffset = (int)R.BaseStream.Position + sizeof(int);
			Code = new InstrVector(R, sizeof(int));
			Constants = new Constants(R);
			DebugInfo = new DbgInfo(R);
		}

		public Function(Instruction[] Instrs) {
			Src = "CODE";
			IsVararg = true;
			if (Instrs != null)
				Code = new InstrVector(Instrs);
			else
				Code = new InstrVector();
			Constants = new Constants();
			DebugInfo = new DbgInfo();
		}

		public Function()
			: this((Instruction[])null) {
		}

		public int Push(LType LuaType, object Val) {
			return Constants.Add(LuaType, Val);
		}

		public void Save(BinaryWriter W) {
			W.WriteLuaString(Src);
			W.Write(LineDefined);
			W.Write(LastLineDefined);

			W.Write(NUps);
			W.Write(NumParams);
			W.Write(IsVararg);
			W.Write(MaxStackSize);

			Code.Save(W);
			Constants.Save(W);
			DebugInfo.Save(W);
		}

		public override string ToString() {
			StringBuilder SB = new StringBuilder();

			SB.Append("<")
			.Append(Src)
			.Append(":")
			.Append(LineDefined)
			.Append(",")
			.Append(LastLineDefined)
			.Append("> (")
			.Append(Code.Length)
			.Append(" instructions, ")
			.Append(Code.Length * sizeof(int))
			.Append(" bytes ")
			.Append(Src)
			.Append(string.Format(":0x{0:X}", CodeOffset))
			.AppendLine(")")
			.Append(NumParams);

			if (IsVararg)
				SB.Append("+");

			SB.Append(" param, ")
			.Append(MaxStackSize)
			.Append(" stacksize, ")
			.Append(NUps)
			.Append(" upvalue, ")
			.Append(DebugInfo.LocalVars.Length)
			.Append(" local, ")
			.Append(Constants.List.Count)
			.Append(" constant, ")
			.Append(Constants.Functions.Count)
			.AppendLine(" function");

			for (int i = 0; i < Code.Length; i++) {
				SB.Append("    ")
				.Append(i)
				.Append("  ")
				.Append(Code[i])
				.AppendLine();
			}
			if (Code.Length == 0)
				SB.AppendLine("    -  NONE");

			SB.Append("\nconstants (")
			.Append(Constants.List.Count)
			.Append(") ")
			.Append(Src)
			.AppendLine();

			for (int i = 0; i < Constants.List.Count; i++) {
				SB.Append("    ")
				.Append(i)
				.Append("  ")
				.Append(Constants.List[i])
				.AppendLine();
			}
			if (Constants.List.Count == 0)
				SB.AppendLine("    -  NONE");

			SB.Append("\nlocals (")
			.Append(Constants.List.Count)
			.Append(") ")
			.Append(Src)
			.AppendLine();

			for (int i = 0; i < DebugInfo.LocalVars.Length; i++) {
				SB.Append("    ")
				.Append(i)
				.Append("  ")
				.Append(DebugInfo.LocalVars[i])
				.AppendLine();
			}
			if (DebugInfo.LocalVars.Length == 0)
				SB.AppendLine("    -  NONE");

			SB.Append("\nupvalues (")
			.Append(Constants.List.Count)
			.Append(") ")
			.Append(Src)
			.AppendLine();

			for (int i = 0; i < DebugInfo.UpValues.Length; i++) {
				SB.Append("    ")
				.Append(i)
				.Append("  ")
				.Append(DebugInfo.UpValues[i])
				.AppendLine();
			}
			if (DebugInfo.UpValues.Length == 0)
				SB.AppendLine("    -  NONE");

			SB.Append("\nfunctions (")
			.Append(Constants.Functions.Count)
			.Append(") ")
			.Append(Src)
			.AppendLine();

			for (int i = 0; i < Constants.Functions.Count; i++)
				SB.AppendLine("    " + i + " " + Constants.Functions[i].ToString().Replace("\n", "\n      "));
			if (Constants.Functions.Count == 0)
				SB.AppendLine("    -  NONE");

			return SB.ToString();
		}

		public string ToString2() {
			StringBuilder SB = new StringBuilder();

			for (int i = 0; i < Code.Length; i++) {
				SB.Append(Code[i])
				.AppendLine();
				if (Code[i].Code == OpCode.CLOSURE)
					SB.Append("    ")
					.AppendLine(Constants.Functions[Code[i].Bx].ToString2().Replace("\n", "\n    ").TrimEnd());
			}

			return SB.ToString();
		}
	}
}