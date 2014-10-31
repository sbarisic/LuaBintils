using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace LuaBin {
	public class Header {
		public bool PUCRioImpl;
		public byte Endianess;
		public byte IntSize;
		public byte Size_tSize;
		public byte InstrSize;
		public byte LuaNumSize;
		public bool FloatingPoint;

		internal byte BVer;
		public byte BVersion {
			get {
				return BVer;
			}
			set {
				BVer = value;
				Version = string.Format("{0:X}", BVersion);
				Version = string.Join(".", Version[0], Version[1]);
			}
		}
		public string Version;

		public Header(BinaryReader R) {
			R.BaseStream.Seek(0, SeekOrigin.Begin);

			Dbg.Assert(R.ReadByte() == 0x1B, "Invalid Header");
			Dbg.Assert(R.ReadByte() == 0x4C && R.ReadByte() == 0x75 && R.ReadByte() == 0x61, "Invalid Header");
			BVersion = R.ReadByte();
			Dbg.Assert(Version == "5.1", "Version not supported");

			PUCRioImpl = R.ReadBoolean();
			Endianess = R.ReadByte();
			IntSize = R.ReadByte();
			Size_tSize = R.ReadByte();
			InstrSize = R.ReadByte();
			LuaNumSize = R.ReadByte();
			FloatingPoint = R.ReadBoolean();
		}

		public Header() {
			BVer = 0x51;
			PUCRioImpl = false;
			Endianess = 0x01;
			Size_tSize = IntSize = InstrSize = sizeof(int);
			LuaNumSize = sizeof(long);
			FloatingPoint = false;
		}

		public void Save(BinaryWriter W) {
			W.Seek(0, SeekOrigin.Begin);
			W.Write(new byte[] { 0x1B, 0x4C, 0x75, 0x61 });
			W.Write(BVer);
			W.Write(PUCRioImpl);
			W.Write(Endianess);
			W.Write(IntSize);
			W.Write(Size_tSize);
			W.Write(InstrSize);
			W.Write(LuaNumSize);
			W.Write(FloatingPoint);
		}

		public Header Validate() {
			Dbg.Assert(!PUCRioImpl, "Not official dump file");
			Dbg.Assert(Endianess == 0x01, "Bytecode not little endian");
			Dbg.Assert(IntSize == sizeof(int), "Integer size does not match");
			Dbg.Assert(Size_tSize == sizeof(int), "Size_t size does not match");
			Dbg.Assert(InstrSize == sizeof(int), "VM instruction size does not match");
			Dbg.Assert(LuaNumSize == sizeof(long), "Lua number size does not match");
			Dbg.Assert(!FloatingPoint, "Floating point not supported");
			return this;
		}
	}
}