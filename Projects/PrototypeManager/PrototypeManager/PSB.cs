using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrototypeManager
{
    public class PSB : PluginBase
    {
        const byte JmpLen = 9;

        byte[] Signature = new byte[] { 0x00, 0x45, 0x64, 0x69, 0x74, 0x65, 0x64, 0x20, 0x57, 0x69, 0x74, 0x68, 0x20, 0x50, 0x72, 0x6F, 0x74, 0x6F, 0x74, 0x79, 0x70, 0x65, 0x4D, 0x61, 0x6E, 0x61, 0x67, 0x65, 0x72, 0x00 };
        ushort BaseOffset; 
        byte[] Script;
        List<uint> PushPos;
        List<byte[]> LostData;
        List<uint> RetPos;

        bool Edited = false;
        uint SignaturePos = 0;

        public Encoding Encoding = Encoding.GetEncoding(932);
        public PSB(byte[] Script) {
            this.Script = Script;
        }

        public override string[] Import() {
            PushPos = new List<uint>();
            LostData = new List<byte[]>();
            RetPos = new List<uint>();
            PsbHeader Header = new PsbHeader();
            Tools.ReadStruct(Script, ref Header, true);
            BaseOffset = Header.EntryPoint;

            for (uint i = BaseOffset; i < Script.Length - Signature.Length; i++) {
                byte[] Data = GetRange(i, (uint)Signature.Length);

                bool Equals = true;
                for (int x = 0; x < Data.Length && Equals; x++)
                    if (Data[x] != Signature[x])
                        Equals = false;

                if (Equals) {
                    SignaturePos = i;
                    Edited = true;
                }
            }


            for (uint i = BaseOffset; i < (Edited ? SignaturePos : Script.LongLength);) {
                if (IsPushString(i) && !Edited)
                    PushPos.Add(i);                   
                if (IsJmp(i) && Edited) {
                    uint Offset = GetJmp(i);
                    if (Offset > SignaturePos && IsPushString(Offset)) {
                        PushPos.Add(i);
                    }
                }

                i += GetCmdLen(i);
            }

            List<string> Strings = new List<string>();

            if (Edited) {
                foreach (uint Pos in PushPos) {
                    uint Offset = GetJmp(Pos);
                    Strings.Add(GetString(Offset));

                    uint Ret = Pos;
                    Ret += JmpLen;
                    while (Script[Ret] == 0x00)
                        Ret++;

                    RetPos.Add(Ret);

                    Offset += GetCmdLen(Offset);

                    uint LostLen = 0;
                    while (!IsJmp(Offset + LostLen) || GetJmp(Offset + LostLen) != Ret)
                        LostLen++;

                    byte[] LData = GetRange(Offset, LostLen);
                    LostData.Add(LData);                    
                }
            } else {
                foreach (uint Pos in PushPos) {
                    uint Len = GetCmdLen(Pos);
                    if (Len < JmpLen) {
                        List<byte> Buffer = new List<byte>();
                        uint i = Pos + Len;
                        int Missing = (int)(JmpLen - Len);
                        while (Missing > 0) {
                            uint CLen = GetCmdLen(i);
                            Buffer.AddRange(GetRange(i, CLen));
                            Missing -= (int)CLen;
                            i += CLen;
                        }
                        LostData.Add(Buffer.ToArray());
                        RetPos.Add(i);
                    } else {
                        LostData.Add(new byte[0]);
                        RetPos.Add(Pos + Len);
                    }
                    Strings.Add(GetString(Pos));
                }
            }

            return Strings.ToArray();
        }

        public override byte[] Export(string[] Strings) {
            byte[] BaseScript;
            if (Edited)
                BaseScript = GetRange(0, SignaturePos);
            else
                BaseScript = GetRange(0, (uint)Script.LongLength);

            List<byte> AppendBuffer = new List<byte>(Signature);


            for (int i = 0; i < PushPos.Count; i++) {
                uint PIndex = PushPos[i];
                uint RPos = RetPos[i];
                byte[] Losted = LostData[i];

                uint Len = RPos - PIndex;
                for (uint x = 0; x < Len; x++)
                    BaseScript[x + PIndex] = 0x00;

                uint ProxyPos = (uint)(BaseScript.Length + AppendBuffer.Count);

                byte[] ToProxyJmp = BuildJmp(ProxyPos);
                ToProxyJmp.CopyTo(BaseScript, PIndex);

                AppendBuffer.AddRange(BuildString(Strings[i]));
                AppendBuffer.AddRange(Losted);
                AppendBuffer.AddRange(BuildJmp(RPos));
            }

            return BaseScript.Concat(AppendBuffer).ToArray();
        }


        private byte[] BuildString(string Content) {
            byte[] Text = Encoding.GetBytes(Content);


            //F0 XX XX 02 YY YY (String) 00

            List<byte> Buffer = new List<byte>();
            Buffer.Add(0xF0);
            Buffer.AddRange(BitConverter.GetBytes(Tools.Reverse((ushort)(Text.Length + 4))));
            Buffer.Add(0x02);
            Buffer.AddRange(BitConverter.GetBytes(Tools.Reverse((ushort)(Text.Length))));
            Buffer.AddRange(Text);
            Buffer.Add(0x00);

            return Buffer.ToArray();
        }

        private byte[] BuildJmp(uint Offset) {
            uint Pos = Offset - BaseOffset;

            //F0 00 05 01 XX XX XX XX 0D - 9 Bytes

            List<byte> Buffer = new List<byte>();
            Buffer.AddRange(new byte[] { 0xF0, 0x00, 0x05, 0x01 });
            Buffer.AddRange(BitConverter.GetBytes(Tools.Reverse(Pos)));
            Buffer.Add(0x0D);

            return Buffer.ToArray();
        }

        private uint GetCmdLen(uint Index) {
            switch (Script[Index]) {
                default:
                    return 1;

                case 0xF1:
                case 0xF0:
                    return GetUI16(Index + 1) + 3u;                    
            }
        }
        
        private string GetString(uint Index) {
            if (!IsPushString(Index))
                throw new Exception("Bad String Pos");

            uint Len = GetUI16(Index + 4);
            byte[] Buffer = GetRange(Index + 6, Len);

            return Encoding.GetString(Buffer);
        }

        private uint GetJmp(uint Index) {
            if (!IsJmp(Index))
                throw new Exception("Bad Jmp Pos");

            byte[] Arr = new byte[4];
            for (uint i = 0; i < Arr.Length; i++)
                Arr[i] = Script[Index + i + 4];

            return Tools.Reverse(BitConverter.ToUInt32(Arr, 0)) + BaseOffset;
        }

        private bool IsPushString(uint Index) {
            if (Script[Index] != 0xF0)
                return false;
            if (Script[Index + 3] != 0x02)
                return false;
            return true;
        }

        private bool IsJmp(uint Index) {
            if (Script[Index] != 0xF0)
                return false;
            if (Script[Index + 2] != 0x05)
                return false;
            if (Script[Index + 3] != 0x01)
                return false;
            if (Script[Index + 8] != 0x0D)
                return false;
            return true;
        }

        private ushort GetUI16(uint Index) {
            return Tools.Reverse(BitConverter.ToUInt16(GetRange(Index, 2), 0));
        }
        private byte[] GetRange(uint Index, uint Len) {
            byte[] Result = new byte[Len];
            for (uint i = 0; i < Len; i++)
                Result[i] = Script[i + Index];

            return Result;
        }

    }


#pragma warning disable 649
    internal class PsbHeader {
        public ushort EntryPoint;
        public ushort SizeOfStruct;
        public uint Crc32;//No idea in the VITA or PS3, but the PSP don't Check it.
        public uint Unk;
        public uint TalkCount;
    }

#pragma warning restore 649
}
