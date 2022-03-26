using System;
using System.IO;
using System.Security.Cryptography;
using AdvancedBinary;
using System.Text;
using System.Linq;
using ComponentAce.Compression.Libs.ZLib;
using ComponentAce;

namespace SpriteTranslator {
    public class SpriteListEditor {

        byte[] Script;

        SL2Header Header = new SL2Header();
        VLContent Content;

        public Encoding Encoding = Encoding.GetEncoding(932);

        public bool Compress = false;        
        public bool Encrypt = false;
        public bool UpdateModifyDate = true;
        private bool NoUnk14 = true;
        public SpriteListEditor(byte[] Script) {
            this.Script = Script;
            Tools.ReadStruct(Script, ref Header, true, Encoding);
        }

        public string Import() {
            if (Header.IsBinary || Header.Entries != 1)
                throw new Exception("Not Suported.");

            if (Header.IsEncrypted)
                Decrypt();
            if (Header.IsCompressed)
                Decompress();

            Content = new VLContent();
            StructReader Reader = new StructReader(new MemoryStream(Script), true, Encoding);
            Reader.Seek(0x50, 0);

            if (Reader.PeekUInt() == 3557938853) {//English Script from KoiChoco
                Reader.Seek(0x4C, 0);
                Header.Unk = 0;
                NoUnk14 = true;
            }

            Reader.ReadStruct(ref Content);

            return Content.Content;
        }

        public byte[] Export(string Content) {
            dynamic NewVLC = new VLContent() {
                Dummy = this.Content.Dummy,
                Content = Content

            };

            if (NoUnk14 && (Encrypt || Compress)) {
                //The English Script of koichoco have a part of the header cutted off, so... 
                //if you ignore this he corrupt when you encrypt or compress the script
                throw new Exception("This Script is Corrupted, You can't Export it with Compression or Encryption.");
            } 

            SL2Header Header = new SL2Header();
            Tools.CopyStruct(this.Header, ref Header);
            Header.IsCompressed = Header.IsEncrypted = false;
            Header.DecLen = 0;


            byte[] Output = Tools.BuildStruct(ref NewVLC, true, Encoding);

            if (Compress) {
                Output = CompressData(Output);
                Header.IsCompressed = true;
                Header.DecLen = (uint)Encoding.GetByteCount(Content) + 0x18;//???
            }

            if (Encrypt) {
                Output = EncryptData(Output);
                Header.IsEncrypted = true;
            }


            Header.Length = (uint)(Output.LongLength + (NoUnk14 ? 0x4C : 0x50));

            if (UpdateModifyDate) {
                Header.Day = (byte)DateTime.Now.Day;
                Header.Month = (byte)DateTime.Now.Month;
                Header.Year = (ushort)DateTime.Now.Year;
            }

            MemoryStream Data = new MemoryStream();
            StructWriter Result = new StructWriter(Data, true, Encoding);
            Result.WriteStruct(ref Header);

            if (NoUnk14)
                Result.Seek(-4, SeekOrigin.Current);

            Result.Write(Output, 0, Output.Length);
            Output = Data.ToArray();
            Result.Close();
            Data.Close();

            return Output;
        }


        private byte[] CompressData(byte[] Output) {
            MemoryStream Stream = new MemoryStream();
            ZOutputStream Compressor = new ZOutputStream(Stream, 9);

            MemoryStream Data = new MemoryStream(Output);
            CopyStream(Data, Compressor);

            Data.Close();
            Compressor.Finish();

            byte[] Content = Stream.ToArray();

            Compressor.Close();
            Stream?.Close();

            return Content;
        }

        private void Decompress() {
            MemoryStream Temp = new MemoryStream(Script);
            Temp.Seek(0x50, 0);
            MemoryStream ZLIBDATA = new MemoryStream();
            CopyStream(Temp, ZLIBDATA);
            ZLIBDATA.Position = 0;

            Temp.Close();
            Temp = new MemoryStream();
            ZInputStream Zlib = new ZInputStream(ZLIBDATA);
            CopyStream(Zlib, Temp);

            UpdateContent(Temp.ToArray());

            Header.IsCompressed = false;
        }

        #region StreamWorker
        private void CopyStream(Stream Input, Stream Output) {
            int Readed = 0;
            byte[] Buffer = new byte[16];
            do {
                Readed = Input.Read(Buffer, 0, Buffer.Length);
                Output.Write(Buffer, 0, Readed);
            } while (Readed > 0);
        }

        private void UnsafeCopyStream(Stream Input, Stream Output) {
            try {
                while (true) {
                    int b = Input.ReadByte();
                    if (b == -1)
                        break;
                    Output.WriteByte((byte)b);
                }
            }
            catch {
                //looks some scripts have not flushed the final encryption block... so this is required
                //maybe this is the reason of crazy values in the header.
            }
        }
        #endregion

        byte[] Key = new byte[] { 0x04, 0x38, 0x04, 0x31, 0x2D, 0x32, 0x0C, 0x30, 0x43, 0x2E, 0x08, 0x04, 0x16, 0x30, 0x22, 0x0C };

        private byte[] EncryptData(byte[] Data) {
            RijndaelManaged Encrypt = new RijndaelManaged() {
                KeySize = 128,
                BlockSize = 128
            };

            MemoryStream Output = new MemoryStream();
            MemoryStream Content = new MemoryStream(Data);

            CryptoStream Cryptor = new CryptoStream(Output, Encrypt.CreateEncryptor(Key, new byte[16]), CryptoStreamMode.Write);

            CopyStream(Content, Cryptor);

            Cryptor.FlushFinalBlock();


            byte[] Result = Output.ToArray();
            try {
                Cryptor.Close();
            }
            catch { }
            Content.Close();
            Output.Close();

            return Result;
        }
        private void Decrypt() {
            RijndaelManaged Decrypt = new RijndaelManaged() {
                KeySize = 128,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            MemoryStream Reader = new MemoryStream(Script);
            Reader.Seek(0x50, 0);
            CryptoStream Cryptor = new CryptoStream(Reader, Decrypt.CreateDecryptor(Key, new byte[16]), CryptoStreamMode.Read);

            MemoryStream Out = new MemoryStream();
            UnsafeCopyStream(Cryptor, Out);

            Reader.Close();

            UpdateContent(Out.ToArray());

            Out.Close();

            Header.IsEncrypted = false;
        }

        private void UpdateContent(byte[] Content) {
            Script = new byte[0x50];
            Tools.BuildStruct(ref Header, true, Encoding).CopyTo(Script, 0);
            Script = Script.Concat(Content).ToArray();
        }

#pragma warning disable 169, 649
        private struct VLContent {
            [FArray(0x2C)]
            public byte[] Dummy;

            [PString(PrefixType = Const.UINT32)]
            public string Content;
        }
        private struct VL2Content {
            [FArray(0x28)]
            public byte[] Dummy;

            [PString(PrefixType = Const.UINT32)]
            public string Content;
        }

        // Based on BinaryFail Struct, Thank you!
        private struct SL2Header {
            public uint Version;
            public uint EngineVer;
            uint unk1; //0x3ADE68B1 Signature?
            public uint Entries;

            public uint DecLen; //uncompressed size?
            public uint Length;
            uint unk2; //0x07
            uint unk3; //0x20000

            uint unk4;
            byte unk5;
            byte isEncrypted;
            byte isBinary;
            byte isCompressed;
            uint unk6;
            uint unk7;

            uint unk8; //0x5C
            uint unk9; //0x68
            uint unk10;
            uint unk11; //0x4C

            //Modify Date
            public byte Day;
            public byte Month;
            public ushort Year;

            uint unk12;
            uint unk13;
            public uint Unk;

            [Ignore]
            public bool IsEncrypted { get { return isEncrypted != 0; } set { isEncrypted = (byte)(value ? 1 : 0); } }
            [Ignore]
            public bool IsBinary { get { return isBinary != 0; } set { isBinary = (byte)(value ? 1 : 0); } }
            [Ignore]
		public bool IsCompressed { get { return isCompressed != 0; } set { isCompressed = (byte)(value ? 1 : 0); } }
        }
    }

#pragma warning restore 169, 649
}