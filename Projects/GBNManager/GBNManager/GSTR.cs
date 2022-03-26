using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GBNManager
{
    public class GSTR : IGBN
    {
        public GSTR(byte[] Script) {
            this.Script = Script;
            Encoding = Encoding.UTF8;
        }
        public GSTR(byte[] Script, Encoding Encoding)
        {
            this.Script = Script;
            this.Encoding = Encoding;
        }

        public Encoding Encoding;

        byte[] Script;

        //GSTR Padding found at Trillion Steam Ver.
        bool HasPadding;

        Header Header;

        TypeDescriptor[] Types;

        List<uint> OffsetPos;

        public string[] Import() {
            return Import(false);
        }

        string[] Import(bool BigEndian) {
            using (MemoryStream Stream = new MemoryStream(Script))
            using (StructReader Reader = new StructReader(Stream, Encoding: Encoding))
            {
                OffsetPos = new List<uint>();
                Header = new Header();
                Reader.ReadStruct(ref Header);

                if (Header.Flags != 1)
                    return null;

                if (Header.Endian == 'B' && !BigEndian)
                {
                    return Import(true);
                }

                HasPadding = Header.StructOffset == 0x40; 

                Reader.Position = Header.TypesOffset;

                Types = new TypeDescriptor[Header.TypesCount];
                for (int i = 0; i < Types.Length; i++) {
                    var CurrType = new TypeDescriptor();
                    Reader.ReadStruct(ref CurrType);
                    Types[i] = CurrType;
                }

                Reader.Position = Header.StructOffset;

                var StructEnd = Header.StructOffset + (Header.StructSize * Header.StructCount);

                for (int i = 0; i < Header.StructCount; i++) {
                    var EntryOffset = Reader.Position;
                    using (MemoryStream StructData = new MemoryStream(Reader.ReadBytes((int)Header.StructSize)))
                    using (StructReader StructStream = new StructReader(StructData, Reader.BigEndian)) {
                        var StructOffset = StructStream.Position;
                        for (int x = 0; x < Types.Length; x++) {
                            if (Types[x].Type != Type.STRING || StructStream.Position + EntryOffset > StructEnd)
                                continue;

                            StructStream.Position = StructOffset + Types[x].Offset;

                            var StrOffset = (uint)(StructStream.Position + EntryOffset - Header.StructOffset);

                            var Offset = StructStream.ReadUInt32();

                            if (Offset == uint.MaxValue)
                                continue;

                            OffsetPos.Add(StrOffset);
                        }
                    }
                }

                string[] Strings = new string[OffsetPos.Count];

                for (int i = 0; i < Strings.Length; i++) {
                    Reader.Position = OffsetPos[i] + Header.StructOffset;
                    var Offset = Reader.ReadUInt32();
                    Reader.Position = Offset + Header.StringOffset;
                    Strings[i] = Reader.ReadString(StringStyle.CString);
                }

                return Strings;
            }
        }

        public byte[] Export(string[] Content) {
            var NewHeader = new Header();
            Tools.CopyStruct(Header, ref NewHeader);

            using (MemoryStream OriScript = new MemoryStream(Script))
            using (MemoryStream Stream = new MemoryStream())
            using (StructWriter Writer = new StructWriter(Stream, Header.Endian == 'B', Encoding)) {
                NewHeader.StructOffset = (uint)Tools.GetStructLength(NewHeader);

                if (HasPadding)
                    NewHeader.StructOffset += 12;

                OriScript.Position = Header.StructOffset;
                byte[] StructData = new byte[Header.StructCount * Header.StructSize];
                OriScript.Read(StructData, 0, StructData.Length);


                NewHeader.TypesOffset = (uint)(NewHeader.StructOffset + StructData.Length);

                OriScript.Position = Header.TypesOffset;
                byte[] TypeData = new byte[Header.TypesCount * Tools.GetStructLength(new TypeDescriptor())];
                OriScript.Read(TypeData, 0, TypeData.Length);

                NewHeader.StringOffset = (uint)(NewHeader.TypesOffset + TypeData.Length);

                byte[] StrData;
                using (MemoryStream StrStream = new MemoryStream())
                using (StructWriter StrWriter = new StructWriter(StrStream, Writer.BigEndian, Encoding)) {
                    Dictionary<string, uint> OffsetMap = new Dictionary<string, uint>();
                    for (int i = 0; i < Content.Length; i++) { 
                        if (OffsetMap.ContainsKey(Content[i]))
                        {
                            var Offset = OffsetMap[Content[i]];
                            BitConverter.GetBytes(Offset).CopyTo(StructData, OffsetPos[i]);
                        }
                        else
                        {
                            BitConverter.GetBytes((uint)StrWriter.Position).CopyTo(StructData, OffsetPos[i]);
                            OffsetMap[Content[i]] = (uint)StrWriter.Position;
                            StrWriter.Write(Content[i], StringStyle.CString);
                        }
                    }

                    StrWriter.Flush();

                    StrData = StrStream.ToArray();
                }


                Writer.WriteStruct(ref NewHeader);
                
                if (HasPadding)
                    Writer.Write(new byte[12]);

                Writer.Write(StructData);
                Writer.Write(TypeData);
                Writer.Write(StrData);
                Writer.Flush();

                return Stream.ToArray();
            }
        }
    }

#pragma warning disable 649, 169
    //Structure defined by https://github.com/u3shit/neptools/blob/master/doc/formats/gbin.md
    struct Header
    {
        [FString(3)]
        public string Signature;

        public byte Endian; //L = 0x4C, B = 0x42

        ushort Field_04; // always 1
        ushort Field_06; // always 0
        uint Field_08; // always 16
        uint Field_0c; // always 4
        public uint Flags;
        public uint StructOffset;
        public uint StructCount;
        public uint StructSize;
        public uint TypesCount;
        public uint TypesOffset;
        uint Field_28; // ??
        public uint StringOffset;
        uint Field_30; // 0 or 4
    };

#pragma warning restore 649, 169
}
