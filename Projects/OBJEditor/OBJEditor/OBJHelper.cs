using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OBJEditor {
    public class OBJHelper : PluginBase {
        Dictionary<int, string> Actors;
        OBJ Editor;
        public OBJHelper(byte[] Script) { Editor = new OBJ(Script); }
        internal const char Open = '「', Close = '」', BreakLine = '＿';

        //Ryuuji「(Aaaaaaaaagh!!!!)」
        public override string[] Import() {
            string[] Strings = Editor.Import();

            Actors = new Dictionary<int, string>();
            for (int i = 0; i < Strings.Length; i++) {
                string Line = Strings[i];
                Actors[i] = null;
                if (Line.EndsWith(Close.ToString()) && Line.Contains(Open)) {
                    string Actor = Line.Substring(0, Line.IndexOf(Open));
                    Line = Line.Substring(Actor.Length, Line.Length - Actor.Length);
                    Actors[i] = Actor;
                }

                Strings[i] = Line.Replace(BreakLine, '\n');
            }

            return Strings;
        }

        public override byte[] Export(string[] Strings) {
            string[] Tmp = new string[Strings.Length];
            for (int i = 0; i < Strings.Length; i++) {
                string Line = Strings[i];
                if (Actors[i] != null)
                    Line = Actors[i] + Line;

                Tmp[i] = Line.Replace('\n', BreakLine);
            }

            return Editor.Export(Tmp);
        }
    }
}
