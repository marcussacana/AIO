using System;
using System.Collections.Generic;
using System.IO;

namespace OBJEditor {
    public class OBJ {
        byte Version = 0;

        const byte vTORADORA = 0, vOREIMO = 1;

        byte[] Script;
        public OBJ(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            DetectVersion();
            List<string> Strings = new List<string>();
            int BlockCount = Script.GetInt32(0x00);
            int BlockLen = Script.GetInt32(0x04);
            for (int i = BlockLen, x = 0; x < BlockCount; x++, i += BlockLen) {
                BlockLen = Script.GetInt32(i);
                if (Version == vOREIMO) {
                    try {
                        int Index = i + 6;
                        int Entries = 0;
                        switch (Script.GetInt16(i + 4)) {
                            case DIALOGUE2:
                            case DIALOGUE:
                                Strings.Add(Script.GetString(i + 11));
                                break;

                            case CHOICE:
                                Entries = Script.GetInt32(Index);
                                for (int y = 0; y < Entries; y++) {
                                    Index += 0x8;
                                    Strings.Add(Script.GetString(Index));
                                    Index += (Script.GetInt32(Index) * 2) + 4;
                                }
                                break;

                            case CHOICE2:
                                Entries = Script.GetInt32(Index);
                                Index += 0x8;

                                for (int y = 0; y < Entries; y++) {

                                    Strings.Add(Script.GetString(Index));
                                    Index += (Script.GetInt32(Index) * 2) + 4;

                                    if (Script.GetInt32(Index) == 0x00) {
                                        Index += 8;
                                    } else {
                                        System.Diagnostics.Debug.Assert(Script.GetInt32(Index) == 0x01);

                                        Index += 4;
                                        Index += (Script.GetInt32(Index) * 2) + 4;
                                        Index += 4;
                                    }
                                }
                                break;

                            case QUESTION:
                                Index += 4;
                                Entries = Script.GetInt32(Index);
                                Index += 4;

                                Strings.Add(Script.GetString(Index));
                                Index += 0x4 + (Script.GetInt32(Index) * 2);

                                for (int y = 0; y < Entries; y++) {
                                    Strings.Add(Script.GetString(Index));
                                    Index += 0x4 + (Script.GetInt32(Index) * 2) + 0x24;
                                }
                                break;
                            case CHAPTER:
                                Strings.Add(Script.GetString(Index));
                                break;
                        }
                    } catch (Exception ex) {
#if DEBUG
                        throw ex;
#else
                    continue;
#endif
                    }
                } else if (Version == vTORADORA) {
                    try {
                        int Index = i + 6;
                        int Entries = 0;
                        switch (Script.GetInt16(i + 4)) {
                            case DIALOGUE2:
                            case DIALOGUE:
                                Strings.Add(Script.GetString(i + 10));
                                break;

                            case CHOICE:
                                Entries = Script.GetInt32(Index);
                                for (int y = 0; y < Entries; y++) {
                                    Index += 0x8;
                                    Strings.Add(Script.GetString(Index));
                                    Index += (Script.GetInt32(Index) * 2) + 4;
                                }
                                break;

                            case CHOICE2:
                                Entries = Script.GetInt32(Index);
                                Index += 0x8;

                                for (int y = 0; y < Entries; y++) {

                                    Strings.Add(Script.GetString(Index));
                                    Index += (Script.GetInt32(Index) * 2) + 4;

                                    if (Script.GetInt32(Index) == 0x00) {
                                        Index += 8;
                                    } else {
                                        System.Diagnostics.Debug.Assert(Script.GetInt32(Index) == 0x01);

                                        Index += 4;
                                        Index += (Script.GetInt32(Index) * 2) + 4;
                                        Index += 4;
                                    }
                                }
                                break;

                            case QUESTION:
                                Index += 4;
                                Entries = Script.GetInt32(Index);
                                Index += 4;

                                Strings.Add(Script.GetString(Index));
                                Index += 0x4 + (Script.GetInt32(Index) * 2);

                                for (int y = 0; y < Entries; y++) {
                                    Strings.Add(Script.GetString(Index));
                                    Index += 0x4 + (Script.GetInt32(Index) * 2) + 0x24;
                                }
                                break;
                            case CHAPTER:
                                Strings.Add(Script.GetString(Index));
                                break;
                        }
                    } catch (Exception ex) {
#if DEBUG
                        throw ex;
#else
                        continue;
#endif
                    }
                }
            }

            return Strings.ToArray();
        }

        private void DetectVersion() {
#if TORADORA
            Version = vTORADORA;
            return;
#endif
#if OREIMO
            Version = vOREIMO;
            return;
#endif

            int BlockCount = Script.GetInt32(0x00);
            int BlockLen = Script.GetInt32(0x04);
            bool OREIMO = true;
            for (int i = BlockLen, x = 0; x < BlockCount; x++, i += BlockLen) {
                BlockLen = Script.GetInt32(i);
                int TYPE = Script.GetInt16(i + 4);
                if (TYPE == DIALOGUE || TYPE == DIALOGUE2) {
                    if (Script[i + 6] != 0x00)
                        OREIMO = false;
                }
            }

            Version = OREIMO ? vOREIMO : vTORADORA;
        }

        public byte[] Export(string[] Strings) {
            int BlockCount = Script.GetInt32(0x00);
            int BlockLen = Script.GetInt32(0x04);

            MemoryStream Output = new MemoryStream();
            Script.CopyTo(Output, 0x00, BlockLen);

            for (int i = BlockLen, x = 0, ID = 0; x < BlockCount; x++, i += BlockLen) {
                BlockLen = Script.GetInt32(i);
                MemoryStream NewBlock;
                int Index = i;
                int Count;

                if (Version == vOREIMO) {
                    try {
                        switch (Script.GetInt16(i + 4)) {
                            case DIALOGUE2:
                            case DIALOGUE:
                                NewBlock = new MemoryStream();
                                Script.CopyTo(NewBlock, i + 4, 0x7);
                                Strings[ID++].WriteTo(NewBlock);

                                WriteBlock(NewBlock, Output);
                                break;

                            case CHOICE:
                                NewBlock = new MemoryStream();
                                Count = Script.GetInt32(Index + 0x6);
                                Script.CopyTo(NewBlock, Index + 4, 0x2);
                                Index += 0x06;

                                for (int y = 0; y < Count; y++) {
                                    Script.CopyTo(NewBlock, Index, 0x8);
                                    LimitString(Strings[ID++]).WriteTo(NewBlock);

                                    Index += 0x8;
                                    Index += (Script.GetInt32(Index) * 2) + 4;
                                }

                                WriteBlock(NewBlock, Output);
                                break;
                            case CHOICE2:
                                NewBlock = new MemoryStream();
                                Index += 4;

                                Count = Script.GetInt32(Index + 0x2);

                                Script.CopyTo(NewBlock, Index, 0xA);
                                Index += 0xA;


                                for (int y = 0; y < Count; y++) {
                                    LimitString(Strings[ID++]).WriteTo(NewBlock);
                                    Index += (Script.GetInt32(Index) * 2) + 4;


                                    Script.CopyTo(NewBlock, Index, 0x4);
                                    Index += 4;

                                    if (Script.GetInt32(Index - 4) == 0x00) {
                                        Script.CopyTo(NewBlock, Index, 0x4);
                                        Index += 4;
                                    } else {

                                        //I think is a boolean, but just in case
                                        System.Diagnostics.Debug.Assert(Script.GetInt32(Index - 4) == 0x1);


                                        int LabelLen = (Script.GetInt32(Index) * 2) + 4;
                                        Script.CopyTo(NewBlock, Index, LabelLen);
                                        Index += LabelLen;

                                        Script.CopyTo(NewBlock, Index, 0x4);
                                        Index += 4;
                                    }
                                }

                                WriteBlock(NewBlock, Output);
                                break;

                            case QUESTION:
                                NewBlock = new MemoryStream();

                                Count = Script.GetInt32(Index + 0xA);
                                Script.CopyTo(NewBlock, Index + 4, 0xA);
                                Index += 0xE;

                                Index += (Script.GetInt32(Index) * 2) + 4;
                                LimitString(Strings[ID++]).WriteTo(NewBlock);

                                for (int y = 0; y < Count; y++) {
                                    Index += (Script.GetInt32(Index) * 2) + 4;

                                    LimitString(Strings[ID++]).WriteTo(NewBlock);


                                    Script.CopyTo(NewBlock, Index, 0x24);
                                    Index += 0x24;
                                }

                                WriteBlock(NewBlock, Output);
                                break;

                            case CHAPTER:
                                NewBlock = new MemoryStream();
                                Script.CopyTo(NewBlock, Index + 4, 0x2);

                                Index += 0x6;
                                Index += (Script.GetInt32(Index) * 2) + 4;

                                Strings[ID++].WriteTo(NewBlock);
                                Script.CopyTo(NewBlock, Index, 0x6);

                                WriteBlock(NewBlock, Output);
                                break;

                            default:
                                Script.CopyTo(Output, i, BlockLen);
                                break;
                        }
                    } catch (Exception ex) {
#if DEBUG
                        throw ex;
#else
                        Script.CopyTo(Output, i, BlockLen);
                        continue;
#endif
                    }
                } else if (Version == vTORADORA) {
                    try {
                        switch (Script.GetInt16(i + 4)) {
                            case DIALOGUE2:
                            case DIALOGUE:
                                NewBlock = new MemoryStream();
                                Script.CopyTo(NewBlock, i + 4, 0x6);
                                Strings[ID++].WriteTo(NewBlock);

                                WriteBlock(NewBlock, Output);
                                break;

                            case CHOICE:
                                NewBlock = new MemoryStream();
                                Count = Script.GetInt32(Index + 0x6);
                                Script.CopyTo(NewBlock, Index + 4, 0x2);
                                Index += 0x06;

                                for (int y = 0; y < Count; y++) {
                                    Script.CopyTo(NewBlock, Index, 0x8);
                                    LimitString(Strings[ID++]).WriteTo(NewBlock);

                                    Index += 0x8;
                                    Index += (Script.GetInt32(Index) * 2) + 4;
                                }

                                WriteBlock(NewBlock, Output);
                                break;
                            case CHOICE2:
                                NewBlock = new MemoryStream();
                                Index += 4;

                                Count = Script.GetInt32(Index + 0x2);

                                Script.CopyTo(NewBlock, Index, 0xA);
                                Index += 0xA;


                                for (int y = 0; y < Count; y++) {
                                    LimitString(Strings[ID++]).WriteTo(NewBlock);
                                    Index += (Script.GetInt32(Index) * 2) + 4;


                                    Script.CopyTo(NewBlock, Index, 0x4);
                                    Index += 4;

                                    if (Script.GetInt32(Index - 4) == 0x00) {
                                        Script.CopyTo(NewBlock, Index, 0x4);
                                        Index += 4;
                                    } else {

                                        //I think is a boolean, but just in case
                                        System.Diagnostics.Debug.Assert(Script.GetInt32(Index - 4) == 0x1);


                                        int LabelLen = (Script.GetInt32(Index) * 2) + 4;
                                        Script.CopyTo(NewBlock, Index, LabelLen);
                                        Index += LabelLen;

                                        Script.CopyTo(NewBlock, Index, 0x4);
                                        Index += 4;
                                    }
                                }

                                WriteBlock(NewBlock, Output);
                                break;

                            case QUESTION:
                                NewBlock = new MemoryStream();

                                Count = Script.GetInt32(Index + 0xA);
                                Script.CopyTo(NewBlock, Index + 4, 0xA);
                                Index += 0xE;

                                Index += (Script.GetInt32(Index) * 2) + 4;
                                LimitString(Strings[ID++]).WriteTo(NewBlock);

                                for (int y = 0; y < Count; y++) {
                                    Index += (Script.GetInt32(Index) * 2) + 4;

                                    LimitString(Strings[ID++]).WriteTo(NewBlock);


                                    Script.CopyTo(NewBlock, Index, 0x24);
                                    Index += 0x24;
                                }

                                WriteBlock(NewBlock, Output);
                                break;

                            case CHAPTER:
                                NewBlock = new MemoryStream();
                                Script.CopyTo(NewBlock, Index + 4, 0x2);

                                Index += 0x6;
                                Index += (Script.GetInt32(Index) * 2) + 4;

                                Strings[ID++].WriteTo(NewBlock);
                                Script.CopyTo(NewBlock, Index, 0x6);

                                WriteBlock(NewBlock, Output);
                                break;

                            default:
                                Script.CopyTo(Output, i, BlockLen);
                                break;
                        }
                    } catch (Exception ex) {
#if DEBUG
                        throw ex;
#else
                    Script.CopyTo(Output, i, BlockLen);
                    continue;
#endif
                    }
                }
            }

            return Output.ToArray();
        }

        private string LimitString(string Input) {
            string Result = Input.Replace(OBJHelper.BreakLine.ToString(), @" ");
#if DEBUG
            if (Result.Length > 44)
                Result = Result.Substring(0, 44);
#endif
            return Result;
        }
        public void WriteBlock(Stream Content, Stream Output) {
            int NewLen = (int)Content.Length + 4;
            int Blank = 0;

            while ((NewLen + Blank) % 0x10 != 0x00)
                Blank++;

            if (Blank <= 0x8)
                Blank += 0x10;

            NewLen += Blank;
            BitConverter.GetBytes(NewLen).CopyTo(Output, 0, 4);
            Content.Seek(0, 0);
            Content.CopyTo(Output);
            (new byte[Blank]).CopyTo(Output, 0, Blank);
        }


        //Not Sure about my names....
        const short DIALOGUE = 0x64;
        const short DIALOGUE2 = 0x68;

        const short CHOICE = 0x69;
        const short CHOICE2 = 0x67;

        const short QUESTION = 0x0323;

        const short CHAPTER = 0x2BC;
    }
}
