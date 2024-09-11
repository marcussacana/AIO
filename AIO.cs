using System.Reflection;

namespace aio
{
    public static class AIO
    {
        public static PluginCreator[] GetPluginCreators() {
            return new PluginCreator[] { 
                new PluginCreator("AVG System (Filter)", "*.scd; *.txt", true, true, typeof(AVGSys.SCD)),
                new PluginCreator("Buriko General Interpreter (BGI)", "(none)", true, true, typeof(EthornellEditor.BurikoScript)),
                new PluginCreator("Nier: Automata (bin)", "*.bin", true, true, typeof(AutomataTranslator.BinTL)),
                new PluginCreator("AdvGameEngine (Eushully)", "*.bin", true, true, typeof(EushullyEditor.BinEditor)),
                new PluginCreator("Global Data String Editor", "*.dat", true, false, typeof(BruteGDStringEditor.GlobalDataStringEditor)),
                new PluginCreator("Circus Editor", "*.mes", true, true, typeof(CircusEditor.MesEditor)),
                new PluginCreator("Cat Scene Editor (CST)", "*.cst", true, true, typeof(CatSceneEditor.CSTHelper)),
                new PluginCreator("Cat Scene Editor (CSTL)", "*.cstl", true, true, typeof(CatSceneEditor.CSTLHelper)),
                new PluginCreator("PSB JSON (Filter)", "*.json", true, true, typeof(JSON.SCNJSONPlain)),
                new PluginCreator("Kag Script (Filter) (UTF16)", "*.ks; *.txt", true, true, typeof(KrKrFilter.KSFilterUTF16)),
                new PluginCreator("Kag Script (Filter) (UTF8)", "*.ks; *.txt", true, true, typeof(KrKrFilter.KSFilterUTF8)),
                new PluginCreator("Kag Script (Filter) (SJIS)", "*.ks; *.txt", true, true, typeof(KrKrFilter.KSFilter)),
                new PluginCreator("Malie Script Editor", "*.dat", true, true, typeof(LigthStringEditor.DatTL)),
                new PluginCreator("SRL String List (Filter)", "*.lst", true, true, typeof(LST.LSTPlain)),
                new PluginCreator("Neptunia Reader", "*.gstr; *.gbin", true, false, typeof(TXT.PlainNep)),
                new PluginCreator("Propeller Manager", "*.msc", true, true, typeof(PropellerManager.MSCTL)),
                new PluginCreator("PSB String Manager", "*.psb; *.scn", true, true, typeof(KrKrSceneManager.PSBAnalyzer)),
                new PluginCreator("Renpy Script (Filter)", "*.rpy", true, true, typeof(RPYFilter)),
                new PluginCreator("Siglus Scene Manager", "*.ss", true, true, typeof(SiglusSceneManager.SSManager)),
                new PluginCreator("Nier: Automata (smd)", "*.smd", true, true, typeof(AutomataTranslator.SMDManager)),
                new PluginCreator("SubStation Alpha (Filter)", "*.ssa; *.ass", true, true, typeof(SSA.Subtitle)),
                new PluginCreator("KiriKiri TJS2 Manager", "*.tjs", true, true, typeof(KrKrSceneManager.TJS2SManager)),
                new PluginCreator("Nier: Automata (tmd)", "*.tmd", true, true, typeof(AutomataTranslator.TMDEditor)),
                new PluginCreator("Plain Text", "*.txt", true, true, typeof(TXT.PlainTxt)),
                new PluginCreator("Unreal Engine 4 Int (Filter)", "*.int; *.ini", true, true, typeof(INI.UE)),
                new PluginCreator("Will Plus Editor", "*.ws2", true, false, typeof(WillPlusManager.WS2Helper)),
                new PluginCreator("Nitro+ (Filter)", "*.txt; *.nss", true, true, typeof(NPFilter.FullFilter)),
                new PluginCreator("TLBOT (Strs Export)", "*.strs", true, true, typeof(STR.Processor)),
                new PluginCreator("NScript", "*.dat", true, true, typeof(NScript.NSFilter)),
                new PluginCreator("NeXAS Bin String Editor (v1)", "*.bin", true, true, typeof(NXSBinEditor.BinHelper)),
                new PluginCreator("Siglus/RealLive Database Editor", "*.dbs", true, true, typeof(SiglusSceneManager.DBS)),
                new PluginCreator("SoftPal DAT Text Editor (SJIS)", "*.dat", true, false, typeof(UnisonShiftManager.DAT)),
                new PluginCreator("CSV String Editor (GoGoNippon 2015/2016 Format)", "*.csv", true, true, typeof(Plain.CSV)),
                new PluginCreator("OBJ Editor (Toradora! Portable Format)", "*.obj", true, true, typeof(OBJEditor.OBJHelper)),
                new PluginCreator("NeXAS Bin String Editor (v2)", "*.bin", true, true, typeof(NXSBinEditor.BinV2)),
                new PluginCreator("NeXAS Bin String Editor (v3)", "*.bin", true, true, typeof(NXSBinEditor.BruteBin)),
                new PluginCreator("Escude Engine Script Editor", "*.bin", true, true, typeof(EscudeEditor.BinScript)),
                new PluginCreator("RLD String Editor", "*.rld", true, true, typeof(RLDManager.RLD)),
                new PluginCreator("Ains Decompiler Export (Filter)", "*.txt", true, true, typeof(TXTEven.AinPlain)),
                new PluginCreator("Aqua Plus Editor (Script)", "*.bin", true, true, typeof(AquaPlusEditor.CSTS)),
                new PluginCreator("Aqua Plus Editor (Database)", "*.bin", true, true, typeof(AquaPlusEditor.DBD)),
                new PluginCreator("Kamidori Helper", "*.bin", true, true, typeof(KamidoriHelper)),
                new PluginCreator("Worms Revolution Translator", "*.bin", true, true, typeof(WormsTranslator.BinEditor)),
                new PluginCreator("1/2 Summer (Filter)", "*.txt", true, true, typeof(Filter.OneSummer)),
                new PluginCreator("Suiheisen made Nan Mile -ORIGINAL FLIGHT- (Filter)", "*.txt", true, true, typeof(SuiNani.SuiNaniPlain)),
                new PluginCreator("QLIE Script (Filter)", "*.s", true, true, typeof(QLIE.S)),
                new PluginCreator("Lua Viewer", "*.sbc; *.lua", true, false, typeof(Debonosu.LUA)),
                new PluginCreator("Majiro String Editor", "*.mjo", true, true, typeof(MajiroStringEditor.Obj1)),
                new PluginCreator("Prototype Manager", "*.psb", true, true, typeof(PrototypeManager.PSB)),
                new PluginCreator("PSB String Manager (No Compression)", "*.psb; *.scn", true, true, typeof(KrKrSceneManagerNoMDF)),
                new PluginCreator("BS5 String Filter (Aokana)", "*.bs5", true, true, typeof(BS5.AoKana)),
                new PluginCreator("SoftPal DAT Text Editor (UTF-8)", "*.dat", true, false, typeof(UnisonShiftManager.DATUTF8)),
                new PluginCreator("Kami no Rhapsody Helper", "*.bin", true, true, typeof(RhapsodyHelper)),
                new PluginCreator("HTML (Filter)", "*.html; *.htm; *.xhtml; *.xhtm", true, true, typeof(HTML.HTMLPlain)),
                new PluginCreator("MWare NUT Editor (UTF8)", "*.nut", true, true, typeof(NUTEditor.NUTUTF8)),
                new PluginCreator("MWare NUT Editor (SJIS)", "*.nut", true, true, typeof(NUTEditor.NUTSJIS)),
                new PluginCreator("Simple EPUB Editor", "*.epub", true, true, typeof(EPUB)),
                new PluginCreator("PIX STUDIO (Filter)", "*.txt", true, true, typeof(TXT.PIXSTUDIO)),
                new PluginCreator("Artemis Engine (Filter)", "*.ast", true, true, typeof(Artemis.AST)),
                new PluginCreator("PersonaLib Dump (Filter)", "*.txt", true, true, typeof(PersonaLib.PersonaLibTxt)),
                new PluginCreator("SpriteTranslator", "*.sl2", true, true, typeof(SpriteTranslator.SpriteTL)),
                new PluginCreator("UE4LocalizationManager", "*.locres", true, true, typeof(UE4LocalizationManager.LocRes)),
                new PluginCreator("Neptune STCM Editor Dump Filter", "*.txt", true, true, typeof(TXT.PlainNep)),
                new PluginCreator("GBNManager", "*.gbin; *.gstr", true, true, typeof(GBNManager.GBN)),
                new PluginCreator("AtelierManager", "*.ebm", true, true, typeof(AtelierManager.EBM)),
                new PluginCreator("GBNManager (SJIS)", "*.gbin; *.gstr", true, true, typeof(GBNSJIS)),
                new PluginCreator("GBNManager (SJIS)", "*.bin", true, true, typeof(GBNManager.TextData)),
                new PluginCreator("Atelier XML (Filter)", "*.xml", true, true, typeof(AtelierManager.XML)),
//                new PluginCreator("Tales of Xillia", "*.dbs; *.dbseng", true, true, typeof()), //Plugin not made by me
                new PluginCreator("PO (Filter)", "*.po", true, true, typeof(PO.POPlain)),
                new PluginCreator("JSON (Filter)", "*.json", true, true, typeof(Json.Helper)),
                new PluginCreator("NekoNyan KGS File", "*.kgs", true, true, typeof(NekoNyan.KGS)),
                new PluginCreator("EntisGLS Filter", "*.srcxml; *.txt", true, true, typeof(RegexFilter)),
                new PluginCreator("Alice-Tool Text Dump (Filter)", "*.txt", true, true, typeof(AliceTool.Dump)),
                new PluginCreator("Fire Emblem Three Houses STR Files", "*.bin", true, true, typeof(ThreeHousesSlave.TextSFile)),
                new PluginCreator("Bullet Girls Phantasia", "*.bnd", true, false, typeof(Projects.BNDDumper.BNDReader))
            };
        }

    }


    public class PluginCreator {
        public string Name { get; private set; }
        public string Extensions { get; private set; }

        public bool CanRead { get; private set; }
        public bool CanWrite { get; private set; }
        
        public Func<byte[], PluginBase> Create { get; private set; }

        public PluginCreator(string Name, string Extensions, bool CanRead, bool CanWrite, Type ClassType) {
            this.Name = Name;
            this.Extensions = Extensions;

            this.CanRead = CanRead;
            this.CanWrite = CanWrite;

            Create = (byte[] arg) => (PluginBase)Activator.CreateInstance(ClassType, new object[] { arg });
        }
    }
}
