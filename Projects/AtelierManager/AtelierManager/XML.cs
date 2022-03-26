using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AtelierManager
{
    class XML : PluginBase
    {
        byte[] Script;
        public XML(byte[] Script)
        {
            this.Script = Script;
        }

        Dictionary<string, int> ElmMaxLen = new Dictionary<string, int>();

        string[] TagsAttribs = new string[] { "text", "title", "message", "desc", "name", "note" };

        HtmlDocument Doc;
        public override string[] Import() {
            using (var Strm = new MemoryStream(Script))
            {
                Doc = new HtmlDocument();
                Doc.OptionOutputOriginalCase = true;
                Doc.OptionWriteEmptyNodes = true;

                Doc.Load(Strm);

                var Declartions = Doc.DocumentNode.SelectNodes("//comment()");
                if (Declartions != null)
                {
                    var Declartion = Declartions.First();
                    if (Declartion != null)
                    {
                        var Tag = Declartion.OuterHtml?.ToLowerInvariant();
                        if (Tag.Contains("encoding"))
                        {
                            var Enco = Tag.Substring(Tag.IndexOf("encoding=")).Split('=')[1].Trim();
                            var Quote = Enco.First();
                            if (Quote == '"' || Quote == '\'')
                            {
                                Enco = Enco.Split(Quote)[1].Trim();
                            }
                            var Declared = Encoding.GetEncoding(Enco);

                            Strm.Position = 0;
                            Doc.Load(Strm, Declared);
                        }
                    }
                }

                List<string> Strs = new List<string>();
                foreach (var Tag in TagsAttribs)
                {
                    var FoundElms = Doc.DocumentNode.SelectNodes("//*[@*]");
                    if (FoundElms == null)
                        continue;
                    foreach (var FoundElm in FoundElms) {

                        if (ElmMaxLen.ContainsKey(FoundElm.Name))
                        {
                            if (FoundElm.InnerLength > ElmMaxLen[FoundElm.Name])
                                ElmMaxLen[FoundElm.Name] = FoundElm.InnerLength;
                        }
                        else if (FoundElm.InnerLength == 0)
                        {
                            ElmMaxLen[FoundElm.Name] = FoundElm.InnerLength;
                            HtmlNode.ElementsFlags[FoundElm.Name] = HtmlElementFlag.Empty;
                        }

                        foreach (var Attribute in FoundElm.Attributes) {
                            if (!Attribute.Name.ToLower().EndsWith(Tag) && !Attribute.Name.ToLower().StartsWith(Tag))
                                continue;
                            Strs.Add(HttpUtility.HtmlDecode(Attribute.Value));
                        }
                    }
                }
                return Strs.ToArray();
            }
        }

        public override byte[] Export(string[] Content) {

            int ID = 0;
            foreach (var Tag in TagsAttribs)
            {
                var FoundElms = Doc.DocumentNode.SelectNodes("//*[@*]");
                if (FoundElms == null)
                    continue;
                foreach (var FoundElm in FoundElms)
                {

                    foreach (var Attribute in FoundElm.Attributes)
                    {
                        if (!Attribute.Name.ToLower().EndsWith(Tag) && !Attribute.Name.ToLower().StartsWith(Tag))
                            continue;
                        Attribute.Value = HttpUtility.HtmlEncode(Content[ID++]);
                    }
                }
            }

            using (MemoryStream Stream = new MemoryStream())
            {
                Doc.Save(Stream, Encoding.UTF8);
                return Stream.ToArray();
            }
        }
    }
}
