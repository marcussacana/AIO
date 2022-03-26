﻿using AdvancedBinary;
using System;
using System.Linq;
using System.Text;

namespace EscudeEditor {
    public class BinScript : PluginBase {
        const string Signature = "ESCR1_00";

        //Taken from: https://github.com/regomne/chinesize/blob/master/ACPX/extBin.py#L6-L7
        string halfgana = "!?｡｢｣､･ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ";
        string fullgana = "！？　。「」、…をぁぃぅぇぉゃゅょっーあいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわん゛゜";

        public Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Script;
        BinFormat Bin;
        public BinScript(byte[] Script) => this.Script = Script;

        public override string[] Import() {
            if (Script.GetString(0, 0x8) != Signature)
                throw new Exception("Bad Escude Script");

            Bin = new BinFormat();
            Tools.ReadStruct(Script, ref Bin, Encoding: Encoding);
            
            return (from x in Bin.Strings select Encoder(x, false)).ToArray();
        }

        public string Encoder(string String, bool Encode) {
            string Source = Encode ? fullgana : halfgana;
            string Target = Encode ? halfgana : fullgana;

            string Result = string.Empty;
            for (int i = 0; i < String.Length; i++) {
                char c = String[i];
                int Index = Source.IndexOf(c);
                if (Index >= 0)
                    c = Target[Index];

                Result += c;
            }

            return Result;
        }

        public override byte[] Export(string[] Strings) {
            string[] Strs = (from x in Strings select Encoder(x, true)).ToArray();
            Bin.Strings = Strs;
            uint Len = 0;
            for (int i = 0; i < Strs.Length; i++) {
                Bin.Offsets[i] = Len;
                Len += (uint)Encoding.GetByteCount(Strs[i]) + 1;//+1 = \x0
            }
            return Tools.BuildStruct(ref Bin, Encoding: Encoding);
        }
    }
    
#pragma warning disable 649, 169
    internal struct BinFormat {
        [FString(0x8)]
        public string Signature;

        public uint StringCount;

        [RArray("StringCount")]
        public uint[] Offsets;

        [PArray(PrefixType = Const.UINT32)]
        public byte[] VM;

        uint Unk1;

        [RArray("StringCount"), CString()]
        public string[] Strings;
    }
#pragma warning restore 649, 169
}
