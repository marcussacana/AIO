using System;

namespace EushullyEditor {

    /// <summary>
    /// Extra Resources, Not Recommended.
    /// </summary>
    public static class Resources {

        /// <summary>
        /// if you set true, you can have problem to edit script after fake breaklines
        /// </summary>
        public static bool RemoveBreakLine;

        public static int MonospacedLengthLimit;
        public static string FakeBreakLine(string text)
        {
            text = text.Replace("-----", "");

            string[] lines = text.Split('\n');
            if (lines.Length == 1)
                return text;
            for (int i = 0; i < lines.Length; i++)
            {
                while (lines[i].Length < MonospacedLengthLimit)
                {
                    lines[i] += " ";
                }
                if (!RemoveBreakLine)
                    lines[i] += "\n";
            }
            string Result = string.Empty;
            foreach (string line in lines)
                Result += line;
            if (Result.EndsWith("\n"))
                Result = Result.Substring(0, Result.Length - 1);
            while (Result.EndsWith(" "))
                Result = Result.Substring(0, Result.Length - 1);
            return Result;
        }

        public static string GetFakedBreakLineText(string text) {
            text = text.Replace("", "-----");
            string[] lines = text.Split('\n');
            if (lines.Length > 1) {
                for (int i = 0; i < lines.Length; i++) {
                    string str = lines[i];
                    while (str.EndsWith(" "))
                        str = str.Substring(0, str.Length - 1);
                    lines[i] = str;
                }
                string Result = string.Empty;
                foreach (string line in lines)
                    if (RemoveBreakLine)
                        Result += line;
                    else
                        Result += line + "\n";
                if (Result.EndsWith("\n"))
                    Result = Result.Substring(0, Result.Length - 1);
                return Result;
            }
            else {
                if (text.StartsWith(" ") || text.EndsWith(" "))//prevent problems
                    return text;
                while (text.Contains("  "))
                    text.Replace("  ", " ");
                return text;
            }

        }

        /// <summary>
        /// The Bytes collection to represent a String ends (wait-for-input 0)
        /// </summary>
        public static object _EndText = new object[] { 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// The Bytes collection to represent a String break line (end-text-line 0)
        /// </summary>
        public static object _EndLine = new object[] { 0x6F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //Return In-Game Text, but make the script Read-Only.
        public static String[] MergeStrings(ref BinEditor WorkSpace, bool DetectOnly) {
            String[] Input = WorkSpace.StringsInfo;
            String[] Result = new String[Input.Length];
            Input.CopyTo(Result, 0);
            //Step 1 - Detect Op Codes
            for (int i = 0; i < Result.Length; i++) {
                if (Result[i].IsString) {
                    int NextEntry = FindEnd(WorkSpace, Input[i].OffsetPos, (object[])WorkSpace.Config.StringEntries[Input[i].OpID]);
                    int ig;
                    int[] ign;//ignore
                    Input[i].EndText = WorkSpace.MaskCheck(_EndText, out ig, out ign, NextEntry);
                    /*if (Input[i].EndText) //allow breakline after end string command
                        Input[i].EndLine = WorkSpace.MaskCheck(_EndLine, out ig, out ign, NextEntry + ((object[])_EndText).Length);
                    else*/
                    Input[i].EndLine = WorkSpace.MaskCheck(_EndLine, out ig, out ign, NextEntry);

                }
            }
            WorkSpace.StringsInfo = Input;
            bool Ck = false;
            if (!DetectOnly)
                for (int main = 0, i = 1; i < Result.Length; i++) {
                    String Main = Result[main];

                    if (Main.IsString || Main.Furigana) {
                        String Next = Result[i];
                        if (Next.Furigana || (!Ck && Main.Furigana)) {
                            if (!Ck && Main.Furigana) {
                                Ck = true;
                                Main.Content = "[" + Main.Content + "/" + Next.Content + "]";
                                Next.Content = "";
                            }
                            else {
                                Main.Content += "[" + Next.Content + "/" + Result[i + 1].Content + "]";
                                Next.Content = "";
                                Result[i + 1].Content = "";
                                i++;
                            }
                            continue;
                        }
                        else {
                            Main.Content += Next.Content;
                            Next.Content = "";
                            if (Next.EndLine)
                                Main.Content += "\\n";
                            if (Next.EndText) {
                                main = i + 1;
                                Ck = false;
                                continue;
                            }
                        }
                    }
                    else { main++; Ck = false; }
                }
            return Result;
        }
        private static int FindEnd(BinEditor WorkSpace, int At, object[] Mask) {
            byte[] script = WorkSpace.Script;
            int disc = 0;
            int StartOpCode = 0;
            for (int i = 0; i < Mask.Length; i++) {
                object entry = Mask[i];
                if (entry is Byte)
                    if ((Byte)entry == Byte.Offset) {
                        disc += 3;
                        int ig;
                        int[] ign;

                        if (WorkSpace.MaskCheck(Mask, out ig, out ign, At - i))
                            StartOpCode = At - i;
                    }
            }
            return StartOpCode + disc + Mask.Length;
        }
    }
}
