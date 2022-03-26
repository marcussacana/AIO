using AdvancedBinary;
using System.IO;
using System.Text;

namespace GBNManager
{
    public class TextData : PluginBase
    {
        byte[] Script;
        Encoding Encoding;
        public TextData(byte[] Script) : this(Script, Encoding.UTF8) { }
        public TextData(byte[] Script, Encoding Encoding) {
            this.Encoding = Encoding;
            this.Script = Script;
        }


        public override string[] Import() {
            using (Stream Strm = new MemoryStream(Script))
            using (StructReader Reader = new StructReader(Strm, Encoding: Encoding))
            {
                var File = new TextDataFormat();
                Reader.ReadStruct(ref File);

                File.Content = new string[File.Count];
                for (int i = 0; i < File.Offsets.Length; i++) {
                    Reader.Position = File.Offsets[i].Offset;
                    File.Content[i] = Reader.ReadString(StringStyle.CString);
                }

                return File.Content;
            }
        }

        public override byte[] Export(string[] Content)
        {
            using (var Strm = new MemoryStream())
            using (StructWriter Writer = new StructWriter(Strm, Encoding: Encoding))
            {

                var File = new TextDataFormat();
                File.Content = Content;

                File.Count = Content.Length;
                File.Offsets = new TextDataEntryOffset[File.Count];

                var Offset = 16 + (16 * File.Count);

                for (int i = 0; i < Content.Length; i++) {
                    File.Offsets[i] = new TextDataEntryOffset() { 
                        Offset = Offset
                    };

                    Offset += Encoding.GetByteCount(Content[i] + '\x0');
                }

                Writer.WriteStruct(ref File);
                Writer.Flush();

                foreach (var String in Content)
                    Writer.WriteString(String, StringStyle.CString);


                return Strm.ToArray();
            }
        }
    }



#pragma warning disable 649, 169
    struct TextDataFormat
    {
        public long Count;
        public long Padding;

        [RArray("Count"), StructField]
        public TextDataEntryOffset[] Offsets;

        [Ignore]
        public string[] Content;
    }

    struct TextDataEntryOffset {
        public long Offset;
        public long Padding;
    }
#pragma warning restore 649, 169
}
