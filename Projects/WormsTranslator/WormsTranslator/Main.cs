using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WormsTranslator
{
    public class BinEditor : PluginBase
    {
        byte[] Script;
        public BinEditor(byte[] Script) {
            this.Script = Script;
        }

        public override string[] Import() {
            using (MemoryStream Data = new MemoryStream(Script))
            using (StructReader Reader = new StructReader(Data)) {
                BinFormat Header = new BinFormat();
                Reader.ReadStruct(ref Header);

                string[] Strings = new string[Header.EntryCount];
                for (uint i = 0; i < Strings.LongLength; i++) {
                    uint Offset = Header.Entries[i].Offset + Header.BaseOffset;
                    Reader.Seek(Offset, SeekOrigin.Begin);
                    Strings[i] = Reader.ReadString(StringStyle.CString);
                }

                Reader.Close();
                return Strings;
            }
        }

        public override byte[] Export(string[] Strings) {
            using (MemoryStream StringBuffer = new MemoryStream())
            using (MemoryStream Buffer = new MemoryStream())
            using (StructWriter Writer = new StructWriter(Buffer))
            using (MemoryStream Data = new MemoryStream(Script))
            using (StructReader Reader = new StructReader(Data)) {
                BinFormat Header = new BinFormat();
                Reader.ReadStruct(ref Header);
                uint BaseOffset = (uint)(Reader.BaseStream.Position - Header.BaseOffset);
                Reader.Close();

                
                for (uint i = 0; i < Strings.LongLength; i++) {
                    uint Offset = (uint)(BaseOffset + StringBuffer.Length);
                    Header.Entries[i].Offset = Offset;
                    byte[] tmp = Encoding.UTF8.GetBytes(Strings[i] + "\x0");
                    StringBuffer.Write(tmp, 0, tmp.Length);
                }

                Writer.WriteStruct(ref Header);
                StringBuffer.Position = 0;
                StringBuffer.CopyTo(Writer.BaseStream);
                Writer.Flush();
                byte[] Result = Buffer.ToArray();
                Writer.Close();
                StringBuffer.Close();
                return Result;
            }
        }
    }

#pragma warning disable 649
    internal struct BinFormat {
        public uint Unk;
        public uint Len;
        public uint EntryCount;
        public uint BaseOffset;

        [RArray("EntryCount"), StructField]
        public Entry[] Entries;
    }

    internal struct Entry {
        public uint ID;
        public uint Offset;
    }

#pragma warning restore 649
}
