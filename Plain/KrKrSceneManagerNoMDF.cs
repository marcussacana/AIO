using KrKrSceneManager;

public class KrKrSceneManagerNoMDF : PluginBase {
    PSBAnalyzer Editor;
    public KrKrSceneManagerNoMDF(byte[] Script) {
        Editor = new PSBAnalyzer(Script);
        Editor.CompressPackage = false;
    }

    public override string[] Import() {
        return Editor.Import();
    }

    public override byte[] Export(string[] Strings) {
        return Editor.Export(Strings);
     }
}