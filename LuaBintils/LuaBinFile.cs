using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MiscUtil.IO;
using MiscUtil.Conversion;

namespace LuaBin {
	public class LuaBinFile {
		public Header Header;
		public List<Function> Functions;

		public LuaBinFile(Header H = null) {
			Header = H;
			if (H == null)
				Header = new Header();
			Functions = new List<Function>();
		}

		public LuaBinFile(Header H, params Function[] Funcs) : this(H) {
			if (Funcs != null)
				for (int i = 0; i < Funcs.Length; i++)
					Functions.Add(Funcs[i]);
		}

		public void Save(string Path) {
			EndianBinaryWriter W = BeginSave(Path);

			this.Header.Save(W);
			foreach (Function Func in Functions)
				Func.Save(W);

			W.Flush();
			W.Close();
		}

		public static EndianBinaryWriter BeginSave(string Path) {
			if (File.Exists(Path))
				File.Delete(Path);
			return new EndianBinaryWriter(EndianBitConverter.Little, File.OpenWrite(Path));
		}
	}
}