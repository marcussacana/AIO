using System;

namespace AtelierManager
{
    public class EBM : PluginBase
    {
        byte[] Script;

        static int Mode = -1;
        IEBM Manager;

        public EBM(byte[] Script) {
            this.Script = Script;
        }

        public override string[] Import() {
            switch (Mode)
            {
                case 5:
                    Manager = new EBM5(Script);
                    break;
                case 4:
                    Manager = new EBM4(Script);
                    break;
                case 3:
                    Manager = new EBM3(Script);
                    break;
                case 2:
                    Manager = new EBM2(Script);
                    break;
                case 1:
                    Manager = new EBM1(Script);
                    break;
                case 0:
                    Manager = new EBM0(Script);
                    break;
                default:
                    for (int i = 5; i >= 0; i--)
                    {
                        try
                        {
                            Mode = i;
                            return Import();
                        }
                        catch { }
                    }
                    throw new Exception("Invalid EBM File");
            }
            return Manager.Import();
        }

        public override byte[] Export(string[] Content) {
            return Manager.Export(Content);
        }
    }

    internal interface IEBM {
        string[] Import();
        byte[] Export(string[] Content);        
    }
}
