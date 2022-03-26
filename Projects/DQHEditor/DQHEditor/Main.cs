using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DQHEditor
{
    public class LXEditor : PluginBase
    {
        byte[] Script;
        int BasePos = 0;
        int EndPos;
        Encoding Eco;
        public LXEditor(byte[] Script, Encoding Encoding) {
            this.Script = Script;
            Eco = Encoding;
        }

        public override string[] Import() {
            BasePos = GetDWAt(Script, 0xC);
            int Count = GetDWAt(Script, BasePos)/4;
            string[] Strings = new string[Count];
            for (int i = 0; i < Count; i++) {
                int Pos = GetDWAt(Script, BasePos + (i * 4)) + BasePos;
                List<byte> Buffer = new List<byte>();
                while (Script[Pos] != 0x00)
                    Buffer.Add(Script[Pos++]);
                EndPos = Pos;
                Strings[i] = Eco.GetString(Buffer.ToArray());
            }
            return Strings;
        }

        public override byte[] Export(string[] Strings) {
            byte[] Begin = new byte[BasePos];
            Array.Copy(Script, Begin, BasePos);
            byte[] OffsetTable = new byte[0];
            byte[] StringTable = new byte[0];
            for (int i = 0; i < Strings.Length; i++) {
                int Offset = StringTable.Length + (Strings.Length*4);
                Append(ref OffsetTable, BitConverter.GetBytes(Offset));
                Append(ref StringTable, Eco.GetBytes(Strings[i] + "\x0"));
            }
            byte[] Rst = new byte[0];
            Append(ref Rst, Begin);
            Append(ref Rst, OffsetTable);
            Append(ref Rst, StringTable);
            return Rst;
        }

        private void Append<T>(ref T[] Arr1, T[] Arr2) {
            T[] Rst = new T[Arr1.Length + Arr2.Length];
            Arr1.CopyTo(Rst, 0);
            Arr2.CopyTo(Rst, Arr1.Length);
            Arr1 = Rst;
        }
        private int GetDWAt(byte[] Script, int pos) => BitConverter.ToInt32(Script, pos);
    }
}
