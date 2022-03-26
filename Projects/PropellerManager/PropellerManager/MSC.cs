using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropellerManager {
    public class MSCStringEditor {

        //public Encoding Encoding = Encoding.GetEncoding(932);//sjis JANAI
        public Encoding Encoding = Encoding.GetEncoding(1252);//LATIN ANSI DESU


        byte[] Script;
        public MSCStringEditor(byte[] Script) { this.Script = Script; }

        List<uint> LabelsOffsets;
        List<uint> Offsets;
        uint ByteCodeStart = 0;
        public string[] Import() {
            List<string> Strings = new List<string>();
            LabelsOffsets = GetLabels();
            uint AtualId = 0;
            uint LID = 0;
            Offsets = new List<uint>();
            for (uint i = 0; i < Script.Length; i++) {
                bool Flag = false;

                //10 = Max Miss Index Distance
                //Big Values: Less rigid string detection; 
                //Small Values: More rigid string detection;
                for (uint x = 0; x < 10; x++)
                    if (EqualsAt(Script, GetStrOp(AtualId + x), i)) { 
                        Flag = true;
                        LID = AtualId;
                        AtualId += x;
                        break;
                    }

                if (Flag) {
                    uint oi = i;
                    i += 7;//+= GetStrOp(0).Length;
                    
                    Offsets.Add(i);
                    string String = string.Empty;
                    try { GetString(out String, ref i); }
                    catch { }
                    if (string.IsNullOrWhiteSpace(String) || String.Contains("\u0006") || String.Contains("\x0") || String == "\\n") {
                        Offsets.RemoveAt(Offsets.Count - 1);
                        i = oi;
                        AtualId = LID;
                        continue;
                    }
                    Strings.Add(String);
                }
            }

            return Strings.ToArray();
        }

        public byte[] Export(string[] Strings) {
            byte[] OutScript = new byte[Script.Length];
            Script.CopyTo(OutScript, 0);

            List<uint> NewLabels = new List<uint>(LabelsOffsets.ToArray());

            for (int i = Strings.Length - 1; i >= 0; i--) {
                uint Offset = Offsets[i];
                uint Len = GetStringLength(Offset);
                OutScript = CutRegion(OutScript, Offset, Len);
                byte[] String = CompileString(Strings[i]);
                OutScript = InsertArray(OutScript, String, Offset);
                int Diff = String.Length - (int)Len;


                UpdateOffsets(ref NewLabels, Offset, Diff);
            }

            CompileOffsets(ref OutScript, NewLabels.ToArray());

            return OutScript;
        }

        private void CompileOffsets(ref byte[] Script, uint[] Offsets) {
            uint index = 0x6;
            uint Offset = 0;
            for (int x = 0; x < 2; x++) {
                uint loops = (GetDW(index) / 9);//wtf count
                index += 4;
                for (uint l = 0; l < loops; l++) {
                    index++;//dummy?
                    index += 4;//offset id?
                    BitConverter.GetBytes(Offsets[Offset++]).CopyTo(Script, index);
                    index += 4;//offset
                }
            }
        }

        private void UpdateOffsets(ref List<uint> Offsets, uint Offset, int Difference) {
            for (int i = 0; i < Offsets.Count; i++)
                if (Offsets[i] + ByteCodeStart > Offset)
                    Offsets[i] = (uint)(Offsets[i] + Difference);
        }

        private byte[] CompileString(string String) {
            byte[] StringData = Encoding.GetBytes(String);
            byte[] Out = new byte[StringData.Length + 4];
            BitConverter.GetBytes(StringData.Length).CopyTo(Out, 0);
            StringData.CopyTo(Out, 4);
            return Out;
        }

        private List<uint> GetLabels() {
            ByteCodeStart = GetDW(2);
            List<uint> Labels = new List<uint>();
            uint index = 0x6;
            for (int x = 0; x < 2; x++) {
                uint loops = (GetDW(index) / 9);//wtf count
                index += 4;
                for (uint l = 0; l < loops; l++) {
                    index++;//dummy?
                    index += 4;//offset id?
                    Labels.Add(GetDW(index));
                    index += 4;//offset
                }
            }
            return Labels;
        }

        private uint GetDW(uint pos) {
            byte[] DW = new byte[4];
            for (uint i = 0; i < DW.Length; i++)
                DW[i] = Script[pos + i];
            return BitConverter.ToUInt32(DW, 0);
        }

        private void GetString(out string String, ref uint Index) {
            uint Len = GetDW(Index);
            Index += 4;

            byte[] Buffer = new byte[Len];
            for (uint i = 0; i < Len; i++)
                Buffer[i] = Script[Index++];
            Index--;
            String = Encoding.GetString(Buffer);
        }

        private uint GetStringLength(uint Index) {
            byte[] DW = new byte[4];
            for (int i = 0; i < 4; i++)
                DW[i] = Script[Index++];

            return 4 + BitConverter.ToUInt32(DW, 0);
        }

        private byte[] GetStrOp(uint i) {
            byte[] Arr = new byte[7];
            (new byte[] { 0x05, 0x00, 0x00 }).CopyTo(Arr, 0);
            BitConverter.GetBytes(i).CopyTo(Arr, 3);
            return Arr;
        }

        #region ArrayOperations
        private bool EqualsAt(byte[] Data, byte[] DataToCompare, uint Pos) {
            if (DataToCompare.LongLength + Pos > Data.LongLength)
                return false;
            for (uint i = 0; i < DataToCompare.LongLength; i++)
                if (Data[i + Pos] != DataToCompare[i])
                    return false;
            return true;
        }
        private byte[] InsertArray(byte[] Data, byte[] DataToInsert, uint Pos) {
            byte[] tmp = CutAt(Data, Pos);
            byte[] tmp2 = CutAfter(Data, Pos);
            byte[] Rst = new byte[Data.Length + DataToInsert.Length];
            tmp.CopyTo(Rst, 0);
            DataToInsert.CopyTo(Rst, tmp.Length);
            tmp2.CopyTo(Rst, tmp.Length + DataToInsert.Length);
            return Rst;
        }
        private byte[] CutRegion(byte[] Data, uint pos, uint length) {
            byte[] tmp = CutAt(Data, pos);
            byte[] tmp2 = CutAfter(Data, pos + length);
            byte[] rst = new byte[tmp.Length + tmp2.Length];
            tmp.CopyTo(rst, 0);
            tmp2.CopyTo(rst, tmp.Length);
            return rst;
        }
        private byte[] CutAt(byte[] data, uint pos) {
            byte[] rst = new byte[pos];
            for (uint i = 0; i < pos; i++)
                rst[i] = data[i];
            return rst;
        }
        private byte[] CutAfter(byte[] data, uint pos) {
            byte[] rst = new byte[data.Length - pos];
            for (uint i = pos; i < data.Length; i++)
                rst[i - pos] = data[i];
            return rst;
        }
        #endregion
    }
}
