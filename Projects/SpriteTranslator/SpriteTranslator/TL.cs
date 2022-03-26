using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteTranslator {
    public class SpriteTL : PluginBase {

        public bool Encrypt { get { return Editor.Encrypt; } set { Editor.Encrypt = value; } }
        public bool Compress { get { return Editor.Compress; } set { Editor.Compress = value; } }
        public Encoding Encoding { get { return Editor.Encoding; } set { Editor.Encoding = value; } }
        public List<string> BlackList = new List<string>(new string[] { "if", "endif", "true", "false" });

        public bool FilterNames = true;

        SpriteListEditor Editor;
        public SpriteTL(byte[] Script) {
            Editor = new SpriteListEditor(Script);
        }

        string Content = string.Empty;
        List<StrInfo> StrMap = new List<StrInfo>();
        public override string[] Import() {
            Content = Editor.Import();
            int Level = 0;
            bool InStr = false;
            bool Escaped = false;
            int Start = 0;
            string Dialog = string.Empty;
            List<string> Strings = new List<string>();
            for (int i = 0; i < Content.Length; i++) {
                char c = Content[i];
                if (!InStr) {
                    switch (c) {
                        case '[':
                            Level++;
                            break;
                        case ']':
                            Level--;
                            break;
                        case '"':
                            InStr = true;
                            Start = i + 1;
                            Dialog = string.Empty;
                            break;
                    }
                    continue;
                } 

                if (!Escaped) {
                    switch (c) {
                        case '\\':
                            Escaped = true;
                            break;
                        case '"':
                            InStr = false;
                            if (Dialog.StartsWith("#") || Dialog.StartsWith("_") || Level != 1 || BlackList.Contains(Dialog.Trim().ToLower()))
                                continue;
                            RegisterStr(ref Dialog, Start, i);
                            Strings.Add(Dialog);
                            break;
                        default:
                            Dialog += c;
                            break;
                    }
                    continue;
                }

                //Looks the game don't use escape chars, but i write this code before see...
                //fuck, just ignore... hahaha
                switch (c) {
                    case 'n':
                        Dialog += '\n';
                        break;
                    case 'r':
                        Dialog += '\r';
                        break;
                    case '\\':
                        Dialog += '\\';
                        break;
                    case '"':
                        Dialog += '"';//not exist
                        break;
                    default:
                        throw new Exception("Unk Escape Char");
                }
                Escaped = false;
            }

            return Strings.ToArray();
        }

        public override byte[] Export(string[] Strings) {
            string Content = this.Content;
            
            for (int i = Strings.Length - 1; i > -1; i--) {
                StrInfo Info = (from I in StrMap where I.ID == i select I).Single();

                string Str = Escape(Info.Prefix + Strings[i]);

                Content = ReplaceRange(Info.Start, Info.Len, Content, Str);
            }

            return Editor.Export(Content);
        }

        private string Escape(string Str) {
            string Result = Str.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r");
            if (Result.Contains("\"") && Result.Replace(" \"", " “").Replace("\" ", "” ").Contains("\"")) {
                string result = string.Empty;
                bool Opened = false;
                foreach (string str in Result.Split('"')) {
                    result += str;
                    result += Opened ? '”' : '“';
                    Opened = !Opened;
                }
                Result = result.Substring(0, result.Length - 1);
            } else
                Result = Result.Replace(" \"", " “").Replace("\" ", "” ");
            if (Result.Contains('"'))
                throw new Exception("Failed to Fix The quote");
            return Result;
        }
        private string ReplaceRange(int Start, int Length, string Content, string DataToReplace) {
            string Begin = Content.Substring(0, Start);
            string Middle = DataToReplace;
            string End = Content.Substring(Start + Length, Content.Length - (Start + Length));

            return Begin + Middle + End;
        }

        private void RegisterStr(ref string Dialog, int Start, int End) {
            int Len = End - Start;
            string Prefix = string.Empty;
            if (FilterNames) {
                if (Dialog.Contains("「")) {
                    while (!Dialog.StartsWith("「")) {
                        Prefix += Dialog[0];
                        Dialog = Dialog.Substring(1, Dialog.Length - 1);
                    }
                }
            }
            if (Dialog.StartsWith("/"))
                while (Dialog[0] == ' ' || Dialog[0] == '/') {
                    Prefix += Dialog[0];
                    Dialog = Dialog.Substring(1, Dialog.Length - 1);
                }
            StrInfo Info = new StrInfo() {
                Start = Start,
                Len = Len,
                ID = StrMap.Count,
                Prefix = Prefix
            };
            StrMap.Add(Info);
        }

        private struct StrInfo {
            internal int Start;
            internal int Len;
            internal int ID;
            internal string Prefix;
        }
    }
}
