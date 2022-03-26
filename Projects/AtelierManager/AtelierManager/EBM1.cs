using AdvancedBinary;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AtelierManager
{
    class EBM1 : IEBM
    {
        EBMFormat Data;
        byte[] Script;
        public EBM1(byte[] Script)
        {
            this.Script = Script;
        }

        public string[] Import() {
            using (var Strm = new MemoryStream(Script))
            using (var Reader = new StructReader(Strm, Encoding: Encoding.UTF8))
            {

                Data = new EBMFormat();
                Reader.ReadStruct(ref Data);
                if (Reader.Position != Reader.Length)
                    throw new Exception();
                return Data.Entries.Select(x => x.Message.TrimEnd('\x0')).ToArray();
            }
        }

        public byte[] Export(string[] Strings)
        {
            for (int i = 0; i < Strings.Length; i++)
                Data.Entries[i].Message = Strings[i] + "\x0";

            using (var Output = new MemoryStream())
            using (var Writer = new StructWriter(Output, Encoding: Encoding.UTF8))
            {
                Writer.WriteStruct(ref Data);
                Writer.Flush();
                return Output.ToArray();
            }
        }

        struct EBMFormat
        {
            [PArray(PrefixType = Const.UINT32), StructField]
            public MessageEntry[] Entries;
        }

        struct MessageEntry
        {
            public uint Type;          // always seems to be set to 2
            public uint VoiceId;       // id of the voice for the speaking character
            public uint Unknown1;      // ???
            public uint NameId;        // id of the name to use for the speaking character
            public uint ExtraId;       // seems to be -1 for system messages
            public uint ExprId;        // serious = 0x09, surprise = 0x0a, happy = 0x0c, etc.
            public uint MsgId;         // sequential id of the message
            public uint Unknown2;      // ???

            [PString(PrefixType = Const.UINT32)]
            public string Message;     // text message to display

            public uint Extensions;    // [OPTIONAL] NOA2/Ryza2 extensions
        }
    }
}
