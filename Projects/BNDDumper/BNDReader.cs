using AdvancedBinary;
using System.Text;

namespace aio.Projects.BNDDumper
{
    public class BNDReader : PluginBase
    {
        byte[] Script;
        public BNDReader(byte[] Script)
        {
            this.Script = Script;
        }

        public override string[] Import()
        {
            List<string> Lines = new List<string>();
            using (var MemStream = new MemoryStream(Script))
            using (var Reader = new StructReader(MemStream, Encoding: Encoding.Unicode))
            {
                var Info = new Data();
                Reader.ReadStruct(ref Info);

                foreach (var Entry in Info.Entries)
                {
                    //if ((Entry.Type & 0xF0) == 0x60 || Entry.Type == 0x00 || )
                    {
                        if (Entry.OffsetD == 0)
                            continue;

                        Reader.BaseStream.Position = (long)Entry.OffsetD;
                        var Data = Reader.ReadBytes((int)(Entry.OffsetE - Entry.OffsetD));
                        Lines.Add(Encoding.Unicode.GetString(Data).TrimEnd('\x0'));
                    }
                }
            }

            return Lines.ToArray();
        }

        public override byte[] Export(string[] Lines)
        {
            throw new NotImplementedException();
        }
    }

    public struct Data
    {
        public ulong Unk;
        public ulong UnkOffset;
        public ulong HeaderSize;
        public ulong DataOffset;

        [PArray(PrefixType = Const.INT64), StructField]
        public TLBlock[] Entries;
    }

    public struct TLBlock
    {
        public ulong OffsetA;//+8
        public int UnkA;//+12
        public short Type;//+16 //????
        public short UnkC;//+18
        public ulong OffsetB;//+24
        public ulong OffsetC;//+32
        public ulong OffsetD;//+40
        public ulong UnkD;//+48
        public ulong OffsetE;//+56
        public ulong UnkE;//+64
        public ulong OffsetF;//+72
        public ulong UnkF;
        public ulong UnkG;
    }
}
