using AdvancedBinary;

namespace ThreeHousesSlave
{
    public class TextSFile : PluginBase
    {
        struct TextSHeader
        {
            public uint unk1;
            public uint unk2;
            public uint unk3;
            public uint textPointerLength;
            public uint count;
            public uint unk4;
            public uint unk5;
            public uint unk6;
            [RArray(nameof(count))]
            public uint[] textpointers;
            public uint textTableSize;
        }

        byte[] Script;
        public TextSFile(byte[] Script)
        {
            this.Script = Script;
        }

        public override string[] Import()
        {

            using MemoryStream Stream = new MemoryStream(Script);
            StructReader Reader = new StructReader(Stream);

            if (!IsValid(Stream))
                throw new FileLoadException();

            Stream.Position = 0;


            var header = new TextSHeader();
            Reader.ReadStruct(ref header);
            var PaddingSize = header.textPointerLength - (header.count + 1) * 4;
            var StringOffset = Stream.Position + PaddingSize;

            var Strings = new List<string>();
            for (int i = 0; i < header.textpointers.Length; i++)
            {
                Stream.Position = header.textpointers[i] + StringOffset;

                Strings.Add(Reader.ReadString(StringStyle.CString));
            }

            return Strings.ToArray();
        }

        public override byte[] Export(string[] Strings)
        {


            using MemoryStream Stream = new MemoryStream(Script);
            StructReader Reader = new StructReader(Stream);

            if (!IsValid(Stream))
                throw new FileLoadException();

            Stream.Position = 0;


            var header = new TextSHeader();
            Reader.ReadStruct(ref header);

            var PaddingSize = header.textPointerLength - (header.count + 1) * 4;
            var StringOffset = Stream.Position + PaddingSize;

            using MemoryStream OutStream = new MemoryStream();
            StructWriter Writer = new StructWriter(OutStream);

            for (int i = 0; i < header.textpointers.Length; ++i)
            {
                header.textpointers[i] = (uint)Writer.Position;
                Writer.WriteString(Strings[i], StringStyle.CString);
            }

            var PaddingData = new byte[PaddingSize];
            for (int i = 0; i < PaddingData.Length; i++)
                PaddingData[i] = 0xDD;

            Writer.Flush();

            var StringBuffer = OutStream.ToArray();

            header.textTableSize = (uint)StringBuffer.LongLength;

            OutStream.Position = 0;

            Writer.WriteStruct(ref header);

            Writer.Write(PaddingData);

            Writer.Write(StringBuffer);

            Writer.Flush();

            return OutStream.ToArray();
        }

        public static bool IsValid(Stream stream)
        {
            try
            {
                var header = new TextSHeader();
                StructReader Reader = new StructReader(stream);
                stream.Position = 12;

                uint PtrLen = Reader.ReadUInt32();
                uint Count = Reader.ReadUInt32();

                var PaddingSize = PtrLen - (Count + 1) * 4;

                if (PtrLen != (Count + 1 + (PaddingSize/4)) * 4)
                    return false;

                if (PtrLen == Count || PtrLen == 0 || PtrLen > stream.Length - 16 || Count * 4 > stream.Length)
                    return false;

                stream.Position = 0;
                Reader.ReadStruct(ref header);

                var StringOffset = stream.Position + PaddingSize;

                if (header.textpointers.First() != 0)
                    return false;

                long CurrentSize = 0;
                var Offset = Reader.Position;
                Reader.Position = StringOffset;
                for (var i = 0; i < header.textpointers.Length; i++)
                {
                    bool IsLast = i + 1 >= header.textpointers.Length;
                    var Pointer = header.textpointers[i];
                    if (Pointer != CurrentSize || Pointer > stream.Length)
                            return false;
                    

                    var CurrentOffset = Reader.Position;
                    _ = Reader.ReadString(StringStyle.CString);
                    CurrentSize += Reader.Position - CurrentOffset;
                    
                    if (IsLast && Reader.Read(new byte[1], 0, 1) != 0)
                        return false;
                }

                Reader.Position = Offset;

                return true;
            }
            catch {
                return false;
            }
        }
    }

}
