using GBNManager;
using System.Text;

public class GBNSJIS : PluginBase {
    GBN Editor;
	
    public GBNSJIS(byte[] Script) {
        Editor = new GBN(Script, Encoding.GetEncoding(932));
    }

    public override string[] Import() {
        return Editor.Import();
    }

    public override byte[] Export(string[] Strings) {
        return Editor.Export(Strings);
	}
}