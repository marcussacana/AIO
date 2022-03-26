using System;
using System.IO;

namespace SDBManager
{
    public class SQLMem : PluginBase, IDisposable
    {
        string CurrentTmp;

        byte[] SQLData;

        SQL SQL;
        public SQLMem(byte[] Data) {
            SQLData = Data;
        }

        public void Dispose()
        {
            SQL?.Dispose();
            if (File.Exists(CurrentTmp))
                File.Delete(CurrentTmp);
        }

        public override string[] Import() {
            if (!string.IsNullOrEmpty(CurrentTmp) && File.Exists(CurrentTmp))
                Dispose();
            

            CurrentTmp = Path.GetTempFileName();
            File.WriteAllBytes(CurrentTmp, SQLData);

            SQL = new SQL(CurrentTmp);
            return SQL.Import();
        }

        public override byte[] Export(string[] Content) {
            SQL.Export(Content);
            SQL.Dispose();
            return File.ReadAllBytes(CurrentTmp);
        }
    }
}
