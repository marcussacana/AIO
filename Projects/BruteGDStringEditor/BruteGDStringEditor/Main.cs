using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BruteGDStringEditor {
    public class GlobalDataStringEditor : PluginBase {
        private byte[] Script;

        private int Pos;
        private int EndPos;

        private Dictionary<int, int[]> StrOffsets = new Dictionary<int, int[]>();
        private int[] Offsets;
        public GlobalDataStringEditor(byte[] Script) {
            this.Script = Script;
        }

        
        public override string[] Import() {
            DetectStrings();
            string[] Strings = new string[Offsets.Length + 1];
            for (int i = 0, p = Pos; i < Strings.Length; i++) {
                int len = 0;
                while (Script[p + len] != 0x00)
                    len++;
                byte[] Buffer = new byte[len];
                Array.Copy(Script, p, Buffer, 0, len);
                Strings[i] = Encoding.UTF8.GetString(Buffer);
                p += len + 1;
            }
            DetectOffsets();
            return Strings;
        }

        public override byte[] Export(string[] Strs) {
            if (Strs.Length - 1 != Offsets.Length)
                throw new Exception("You can't add/del string entries");            
            byte[] FirstPart = new byte[Pos];
            Array.Copy(Script, FirstPart, FirstPart.Length);

            byte[] SecondPart = new byte[0];
            for (int i = 0; i < Strs.Length; i++) {
                bool IsLast = !(i + 1 < Strs.Length);
                byte[] Buffer = Encoding.UTF8.GetBytes(IsLast ? Strs[i] : Strs[i] + "\x0");
                if (i != 0) {
                    int Offset = SecondPart.Length;
                    foreach (int offpos in StrOffsets[i-1])
                        GenDW(Offset).CopyTo(FirstPart, offpos);
                }
                Array.Resize(ref SecondPart, SecondPart.Length + Buffer.Length);
                Array.Copy(Buffer, 0, SecondPart, SecondPart.Length - Buffer.Length, Buffer.Length);
            }

            byte[] ThirdPart = new byte[Script.Length - (EndPos + 1)];
            Array.Copy(Script, EndPos + 1, ThirdPart, 0, ThirdPart.Length);

            byte[] Result = FirstPart.Concat(SecondPart).ToArray().Concat(ThirdPart).ToArray();

            int GBNLOffset = IndexOf(Result, "GBNL") + 0x40;
            GBNLOffset = IndexOf(Result, GBNLOffset, true);
            int NewPos = UpdateSection(ref Result, "GBNL");
            GenDW((NewPos + 0x40) - GBNLOffset).CopyTo(Result, GBNLOffset);

            UpdateSection(ref Result, "CODE_START");

            int Diff = Result.Length - Script.Length;
            GenDW(GetDW(0x20) + Diff).CopyTo(Result, 0x20);
            GenDW(GetDW(0x2C) + Diff).CopyTo(Result, 0x2C);

            return Result;
        }

        private int UpdateSection(ref byte[] File, string Section) {
            byte[] Match = Encoding.ASCII.GetBytes(Section);
            for (int i = 0; i < File.Length; i++) {
                if (EqualsAt(Match, File, i)) {
                    if (i % 16 != 0) {
                        int Del = i % 16;
                        int Add = 16 - Del;
                        byte[] Buff = new byte[Del];
                        if (Add <= Del || !EqualsAt(Buff, File, i - Del)) {
                            byte[] First = new byte[i + Add];
                            Array.Copy(File, 0, First, 0, i);

                            byte[] Second = new byte[File.Length - i];
                            Array.Copy(File, i, Second, 0, Second.Length);

                            File = First.Concat(Second).ToArray();
                            return First.Length;

                        } else {
                            byte[] First = new byte[i - Del];
                            Array.Copy(File, 0, First, 0, i - Del);

                            byte[] Second = new byte[File.Length - i];
                            Array.Copy(File, i, Second, 0, Second.Length);

                            File = First.Concat(Second).ToArray();
                            return First.Length;
                        }
                    }                        
                }
            }
            return -1;
        }

        private void DetectOffsets() {
            for (int SOFF = 0; SOFF < Offsets.Length; SOFF++) {
                byte[] offset = BitConverter.GetBytes(Offsets[SOFF]);
                for (int i = 0x50; i < Pos; i++)
                    if (EqualsAt(offset, Script, i)) {
                        int[] Arr = null;
                        if (StrOffsets.Keys.Contains(SOFF))
                            Arr = StrOffsets[SOFF];
                        else
                            Arr = new int[0];
                        int[] NewArr = new int[Arr.Length + 1];
                        Arr.CopyTo(NewArr, 0);
                        NewArr[Arr.Length] = i;
                        StrOffsets[SOFF] = NewArr;
                    }
            }
        }
        

        private byte[] GenDW(int val) => BitConverter.GetBytes(val);

        private void DetectStrings() {
            byte[] Sig = Encoding.ASCII.GetBytes("GBNL");
            int SigPos = 0x40;
            for (; SigPos < Script.Length; SigPos++)
                if (EqualsAt(Sig, Script, SigPos))
                    break;
            if (SigPos >= Script.Length)
                throw new Exception("BruteForce Unsupported");

            Pos = SigPos;
            while (Script[--Pos] == 0x00)
                continue;
            EndPos = Pos;
            SigPos -= 2;
            while ((GetDW(--Pos) & 0x00FFFFFF) != 0x00)
                continue;
            while (Script[++Pos] == 0x00)
                continue;
            
            List<int> Offs = new List<int>();
            for (int i = Pos; i < EndPos; i++) {
                if (Script[i] == 0x00) {
                    int Offset = (i + 1) - Pos;
                    Offs.Add(Offset);
                }
            }
            Offsets = Offs.ToArray();
        }

        private int IndexOf(byte[] Data, string Str) {
            byte[] Sig = Encoding.ASCII.GetBytes(Str);
            for (int i = 0; i < Data.Length - Sig.Length; i++)
                if (EqualsAt(Sig, Data, i))
                    return i;
            return -1;
        }
        private int IndexOf(byte[] Data, int Value, bool Relative = false) {
            byte[] Sig = BitConverter.GetBytes(Value);
            for (int i = 0; i < Data.Length - Sig.Length; i++) {
                if (Relative)
                    Sig = BitConverter.GetBytes(Value - i);
                if (EqualsAt(Sig, Data, i))
                    return i;
            }
            return -1;
        }
        private bool EqualsAt(byte[] DataToCompare, byte[] Data, int Pos) {
            if (DataToCompare.Length + Pos > Data.Length)
                return false;
            for (int i = 0; i < DataToCompare.Length; i++)
                if (DataToCompare[i] != Data[i + Pos])
                    return false;
            return true;
        }
        private int GetDW(int Pos) => BitConverter.ToInt32(Script, Pos);
    }
}
