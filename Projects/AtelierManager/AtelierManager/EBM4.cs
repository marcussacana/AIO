using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AtelierManager
{
    class EBM4 : IEBM
    {
        MessageEntry[] Entries = new MessageEntry[0];
        List<(int Index, byte[] Data)> UnkData = new List<(int Index, byte[] Data)>();

        byte[] Sufix = new byte[0];

        byte[] Script;
        public EBM4(byte[] Script)
        {
            this.Script = Script;
        }

        public string[] Import() {
            var Count = BitConverter.ToInt32(Script, 0) * -1;

            using (var Strm = new MemoryStream(Script.Skip(4).ToArray()))
            using (var Reader = new StructReader(Strm, Encoding: Encoding.UTF8))
            {
                for (int i = 0; i < Count; i++)
                {
                    switch (Reader.PeekInt())
                    {
                        case 2:
                        case 3:
                            Array.Resize(ref Entries, Entries.Length + 1);
                            var Entry = new MessageEntry();
                            Reader.ReadStruct(ref Entry);
                            Entries[i] = Entry;
                            break;
                        case 1:
                            UnkData.Add((i, Reader.ReadBytes(0x28)));
                            break;
                        default:
                            Console.WriteLine("Unk Field: " + Reader.PeekInt().ToString("X8") + " at " + Reader.Position.ToString("X8"));
                            throw new Exception();
                    }
                }

                if (Reader.Position != Reader.BaseStream.Length)
                    Sufix = Reader.ReadBytes((int)(Reader.BaseStream.Length - Reader.Position));
                else
                    Sufix = new byte[0];
            }

            return Entries.Select(x => x.Message.TrimEnd('\x0')).ToArray();
        }

        public byte[] Export(string[] Strings)
        {
            if (Strings.Length != Entries.Length)
                throw new Exception("You can't remove/add entries");

            for (int i = 0; i < Strings.Length; i++)
                Entries[i].Message = Strings[i] + "\x0";

            using (var Output = new MemoryStream())
            using (var Writer = new StructWriter(Output, Encoding: Encoding.UTF8))
            {
                var Count = BitConverter.ToInt32(Script, 0) * -1;
                Writer.Write(Count * -1);

                for (int i = 0, z = 0; i < Count; i++)
                {
                    var UnkEntry = from x in UnkData where x.Index == i select x.Data;
                    
                    if (UnkEntry.Count() == 1)
                        Writer.Write(UnkEntry.Single());
                    else
                        Writer.WriteStruct(ref Entries[z++]);
                }
                
                Writer.Write(Sufix);

                return Output.ToArray();
            }
        }

        struct MessageEntry
        {
            public uint Type;          // always seems to be set to 2
            public uint VoiceId;       // id of the voice for the speaking character
            public uint Unknown1;      // ???
            public uint NameId;        // id of the name to use for the speaking character
            public uint ExtraId;       // seems to be -1 for system messages
            public uint ExprId;        // serious = 0x09, surprise = 0x0a, happy = 0x0c, etc.
            public uint Unknown3;      // [OPTIONAL] Used by Nelke but set to 0xffffffff
            public uint Unknown4;      // [OPTIONAL] Used by Nelke but set to 0xffffffff
            public uint MsgId;         // sequential id of the message
            public uint Unknown2;      // ???

            [PString(PrefixType = Const.UINT32)]
            public string Message;     // text message to display

            public uint Extensions;    // [OPTIONAL] NOA2/Ryza2 extensions
            
            public uint Unknown5; // Atelier Sophie 2
        }
    }
}
