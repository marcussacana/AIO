using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//Source code lost, Decompiled code.

namespace Json
{
    // Token: 0x02000002 RID: 2
    public class Helper
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002057 File Offset: 0x00000257
		public Helper(byte[] JSON)
		{
			this.JSON = Encoding.UTF8.GetString(JSON);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002074 File Offset: 0x00000274
		public string[] Import()
		{
			JObject obj = JsonConvert.DeserializeObject<JObject>(this.JSON);
			List<string> list = new List<string>();
			foreach (JValue jvalue in this.EnumValues(obj))
			{
				bool flag = jvalue.Type == (JTokenType)8;
				if (flag)
				{
					list.Add((string)jvalue.Value);
				}
			}
			return list.ToArray();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002104 File Offset: 0x00000304
		public byte[] Export(string[] Strings)
		{
			JObject jobject = JsonConvert.DeserializeObject<JObject>(this.JSON);
			int num = 0;
			foreach (JValue jvalue in this.EnumValues(jobject))
			{
				bool flag = jvalue.Type == (JTokenType)8;
				if (flag)
				{
					jvalue.Value = Strings[num++];
				}
			}
			return Encoding.UTF8.GetBytes(jobject.ToString());
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002194 File Offset: 0x00000394
		public IEnumerable<JValue> EnumValues(object Obj)
		{
			bool flag = Obj is JValue;
			if (flag)
			{
				yield return (JValue)Obj;
			}
			bool flag2 = Obj is JArray;
			if (flag2)
			{
				JArray Arr = Obj as JArray;
				foreach (JToken Value in Extensions.Values(Arr))
				{
					bool flag3 = Value is JObject;
					if (flag3)
					{
						foreach (JValue SubEntry in this.EnumValues(Value))
						{
							yield return SubEntry;
						}
						IEnumerator<JValue> enumerator2 = null;
					}
					bool flag4 = Value is JValue;
					if (flag4)
					{
						yield return Value as JValue;
					}
				}
				IEnumerator<JToken> enumerator = null;
				Arr = null;
			}
			foreach (object Entry in ((IEnumerable<object>)Obj))
			{
				bool flag5 = Entry is JProperty;
				if (flag5)
				{
					JProperty Prop = Entry as JProperty;
					bool flag6 = Prop.Type == (JTokenType)4;
					if (flag6)
					{
						foreach (JValue SubEntry2 in this.EnumValues(Prop.Value))
						{
							yield return SubEntry2;
						}
						IEnumerator<JValue> enumerator4 = null;
						continue;
					}
					Prop = null;
				}
				bool flag7 = Entry is JObject;
				if (flag7)
				{
					foreach (JValue SubEntry3 in this.EnumValues(Entry))
					{
						yield return SubEntry3;
					}
					IEnumerator<JValue> enumerator5 = null;
				}
			}
			IEnumerator<object> enumerator3 = null;
			yield break;
			yield break;
		}

		// Token: 0x04000001 RID: 1
		private string JSON;
	}
}
