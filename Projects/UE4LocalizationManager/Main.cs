using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UE4LocalizationManager
{
    public class LocRes : PluginBase
    {
        LocResFormat Resource;
        byte[] Script;
        public LocRes(byte[] Script) {
            this.Script = Script;
        }


        public override string[] Import() {
            using (var Stream = new MemoryStream(Script))
            using (var Reader = new StructReader(Stream, Encoding: Encoding.Unicode)) {
                Reader.ReadStruct(ref Resource);   
            }
            return (from x in Resource.Localizations select x.Content).ToArray();
        }

        public override byte[] Export(string[] Strings) {
            if (Resource.Localizations.Length != Strings.Length)
                throw new Exception("You can't add/remove strings from a locres file");
            
            for (int i = 0; i < Resource.Localizations.Length; i++) {
                Resource.Localizations[i].Content = Strings[i];
            }

            using (var Stream = new MemoryStream())
            using (var Writer = new StructWriter(Stream, Encoding: Encoding.Unicode)) {
                Writer.WriteStruct(ref Resource);
                Writer.Flush();
                return Stream.ToArray();
            }
        }
    }

#pragma warning disable 649, 169
    struct LocResFormat {
        [FArray(0x10)]
        public byte[] Signature;
        public byte Version;

        public ulong StringTableOffset;

        [StructField, PArray]
        public Group[] Groups;

        [StructField, PArray]
        public PString[] Localizations;
    }

    struct Group {
        [StructField]
        PString Name;

        [StructField, PArray]
        GroupKey[] Keys;
    }

    struct GroupKey {
        [StructField]
        PString Name;

        uint Hash;
        uint LocId;
    }

    struct PString
    {

        [PreProcess("PreProcessString"), PostProcess("PostProcessString")]
        public int Length;


        [Ignore]
        Encoding EntryEncoding;

        [Ignore]
        public string Content;


        //Encode the Length Field Before Write
        FieldInvoke PreProcessString => new FieldInvoke((Strm, Reading, BigEndian, Struct) =>
        {
            if (Reading)
                return Struct;

            byte[] Data = Struct.EntryEncoding.GetBytes(Struct.Content);
            Struct.Length = Data.Length;

            if (Struct.EntryEncoding.EncodingName == "Unicode")
            {
                Struct.Length /= 2;
                Struct.Length ^= 0xFFFFFFFF;
            }              

            return Struct;
        });

        FieldInvoke PostProcessString => new FieldInvoke((Strm, Reading, BigEndian, Struct) =>
        {
            if (Reading)
            {
                bool Unicode = Struct.Length < 0;
                Struct.EntryEncoding = Struct.Length < 0 ? Encoding.Unicode : Encoding.ASCII;
                if (Struct.Length < 0)
                    Struct.Length ^= 0xFFFFFFFF;

                byte[] Buff = new byte[Unicode ? (Struct.Length + 1) * 2 : (Struct.Length + 1)];
                Strm.Read(Buff, 0, Buff.Length);
                Struct.Content = Struct.EntryEncoding.GetString(Buff).TrimEnd('\x0');
            }
            else
            {
                byte[] Data = Struct.EntryEncoding.GetBytes(Struct.Content + '\x0');
                Strm.Write(Data, 0, Data.Length);
            }
            return Struct;
        });
    }
#pragma warning restore 649, 169
}
