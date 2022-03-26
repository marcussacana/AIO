using EushullyEditor;
using String = EushullyEditor.String;

public class KamidoriHelper : PluginBase {
    BinEditor Editor;
    public KamidoriHelper(byte[] Script) {
        Editor = new BinEditor(Script);
    }

    public override string[] Import() {
        Editor.Import();
        Resources.MergeStrings(ref Editor, true);

        List<string> Dialogues = new List<string>();
        for (uint i = 0; i < Editor.StringsInfo.Length; i++) {
            String Current = Editor.StringsInfo[i];
            String After = null;
			if (i + 1 < Editor.StringsInfo.Length)
				After = Editor.StringsInfo[i + 1];
            if (After != null && Current.IsString && !Current.EndText && After.IsString == true && Current.EndLine && After.EndText) {
                    Dialogues.Add(Current.Content + "\n" + After.Content);
                    i++;
            } else
                Dialogues.Add(Current.Content);
        }

        return Dialogues.ToArray();
    }

    public override byte[] Export(string[] Strings) {
        for (uint i = 0, x = 0; i < Editor.StringsInfo.Length; i++) {
            String Current = Editor.StringsInfo[i];
            String After = null;
			if (i + 1 < Editor.StringsInfo.Length)
				After = Editor.StringsInfo[i + 1];

            if (After != null && Current.IsString && !Current.EndText && After.IsString == true && Current.EndLine && After.EndText) {
                string[] Lines = Strings[x++].Split('\n');
                Current.Content = Lines[0];
                if (Lines.Length > 1) {
					After.Content = string.Empty;
                    for (int y = 1; y < Lines.Length; y++) {
                        After.Content = After.Content.TrimEnd() + " " + Lines[y];
                    }
                    Editor.StringsInfo[i + 1].Content = After.Content.Trim();
                } else {
					Editor.StringsInfo[i + 1].Content = "";
				}
                i++;
            } else
                Editor.StringsInfo[i].Content = Strings[x++];

        }

        return Editor.Export();
     }
}