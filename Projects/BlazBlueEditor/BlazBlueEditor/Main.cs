using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlazBlueEditor
{
    public class ATFStringEditor : PluginBase
    {
        byte[] Script;
        public ATFStringEditor(byte[] Script) {
            this.Script = Script;
        }
        private struct ATFHeader {
#pragma warning disable 0169, 0649
            internal uint Signature;
            internal uint Count;
            private int Unk2;
            private int Unk3;
            internal uint ByteCodeStart;
            private int Count2;
            internal uint ByteCodeLength;
            private int Unk6;
            private int Unk7;
            private int Unk8;
            private int Unk9;
            private int Unk10;
            internal uint StringsTableOffset;
            internal uint StringsTableLenth;
            private int Unk11;
            private int Unk12;
            internal uint TextTableOffset;
            internal uint TextUniTableLength;
            internal uint TextTableLength;
            private int Unk14;
        }

        private struct Entry {
            private int StrPos;
            private int StrLen;
            private long Dummy;
            internal uint TextPos;
            internal uint TextUniLength;
            private long Dummy2;
            internal FieldInvoke TextWork;

            [Ignore]
            internal string Text;
            [Ignore]
            internal ATFHeader Header;
        }
#pragma warning restore 0169, 0649

        List<Entry> Entries;
        private ATFHeader Header;
        public override string[] Import() {
            dynamic OHeader = new ATFHeader();
            Tools.ReadStruct(Script, ref OHeader);
            Header = OHeader;
            if (Header.Signature != 0x00465441)
                throw new Exception("Invalid Input File.");
            Entries = new List<Entry>();
            List<string> Strings = new List<string>();
            StructReader Reader = new StructReader(new MemoryStream(Script), Encoding: Encoding.Unicode);
            Reader.Seek(Header.ByteCodeLength + Header.ByteCodeStart, SeekOrigin.Begin);
            while (Entries.Count < Header.Count) {
                dynamic Entry = new Entry() {
                    Header = Header,
                    TextWork = EntryText
                };
                Reader.ReadStruct(ref Entry);
                Entries.Add(Entry);
                Strings.Add(Entry.Text);
            }
            return Strings.ToArray();
        }

        public override byte[] Export(string[] Strings) {
            uint EntryLen = (uint)Tools.GetStructLength(new Entry());
            byte[] NewScript = new byte[this.Header.TextTableOffset];
            Array.Copy(Script, NewScript, NewScript.Length);
            MemoryStream StringBuffer = new MemoryStream();
            Entry[] EntryArr = Entries.ToArray();
            StructWriter Writer = new StructWriter(StringBuffer, Encoding: Encoding.Unicode);
            uint Length = 0;
            for (int i = 0; i < Strings.Length; i++) {
                dynamic Struct = EntryArr[i];
                Struct.TextPos = Length;
                Struct.TextUniLength = (uint)Strings[i].Length;
                Overwrite(ref NewScript, Tools.BuildStruct(ref Struct), Header.ByteCodeLength + Header.ByteCodeStart + (EntryLen * (uint)i));
                Length += Struct.TextUniLength + 1;
                Writer.Write(Strings[i], StringStyle.UCString);
            }
            Header.TextUniTableLength = (uint)StringBuffer.Length / 2;
            Header.TextTableLength = (uint)StringBuffer.Length;
            dynamic H = Header;
            Tools.BuildStruct(ref H).CopyTo(NewScript, 0);
            NewScript = NewScript.Concat(StringBuffer.ToArray()).ToArray();
            Writer.Close();
            StringBuffer?.Close();
            return NewScript;
        }

        private void Overwrite(ref byte[] Target, byte[] Data, uint At) => Data.CopyTo(Target, At);
        private FieldInvoke EntryText = new FieldInvoke((Stream Stream, bool Reading, bool BigEndian, dynamic Ref) => {
            Entry Entry = Ref;
            if (Reading) {
                long Pointer = Stream.Position;
                Stream.Flush();
                Stream.Position = Entry.Header.TextTableOffset + (Entry.TextPos * 2);
                StructReader Reader = new StructReader(Stream, Encoding: Encoding.Unicode);
                Entry.Text = Reader.ReadString(StringStyle.UCString);
                Stream.Position = Pointer;
                Stream.Flush();
            }
            return Entry;
        });

        public bool EqualsAt(byte[] Arr1, byte[] Arr2, uint Pos) {
            if (Arr2.Length + Pos >= Arr1.Length)
                return false;
            for (uint i = 0; i < Arr2.Length; i++)
                if (Arr1[i + Pos] != Arr2[i])
                    return false;
            return true;
        }
    }
}
