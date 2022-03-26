using System.Text;

namespace STR
{
    public class Processor : PluginBase {
        BinaryReader Script;
		
		
        public Processor(byte[] Script) {
			this.Script = new BinaryReader(new MemoryStream(Script), Encoding.UTF8);
        }

        public override string[] Import() {
            string[] Arr = new string[Script.ReadUInt32()];
			for (uint i = 0; i < Arr.LongLength; i++){
				Arr[i] = Script.ReadString();
			}
			
			return Arr;
        }		

        public override byte[] Export(string[] Text) {
			MemoryStream TMP = new MemoryStream();
			BinaryWriter Writer = new BinaryWriter(TMP, Encoding.UTF8);
			Writer.Write((uint)Text.LongLength);
			
			for (uint i = 0; i < Text.LongLength; i++)
				Writer.Write(Text[i]);
			
			Writer.Flush();
			
			byte[] CNT = TMP.ToArray();
			TMP.Close();
			
			return CNT;
        }
    }
}
