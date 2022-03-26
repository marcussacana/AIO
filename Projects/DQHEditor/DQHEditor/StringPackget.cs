using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DQHEditor {
    public struct File {
        public int Index;
        public  byte[] Content;
    }
    public class StringPackage {

        struct Entry{
            internal int Offset;
            internal int Length;
        }
        public static File[] Open(byte[] Package) {
            StructReader Reader = new StructReader(new System.IO.MemoryStream(Package));
            int Count = Reader.ReadInt32();
            File[] Files = new File[Count];
            for (int i = 0; i < Count; i++) {
                Entry Entry = new Entry();
                Reader.ReadStruct(ref Entry);
                Files[i] = new File() {
                    Index = i,
                    Content = GetSubArray(Package, Entry.Offset, Entry.Length)
                };
            }
            return Files;
        }

        public static byte[] Repack(File[] Files) {
            System.IO.MemoryStream Output = new System.IO.MemoryStream();
            StructWriter Header = new StructWriter(Output);
            System.IO.MemoryStream Content = new System.IO.MemoryStream();
            Header.Write(Files.Length);
            for (int i = 0; i < Files.Length; i++) {
                Entry Entry = new Entry() {
                    Offset = (int)Content.Length + 4 + (8 * Files.Length),//Header Size
                    Length = Files[i].Content.Length
                };
                Header.WriteStruct(ref Entry);
                Content.Write(Files[i].Content, 0, Files[i].Content.Length);
                while (Content.Length % 4 != 0)
                    Content.WriteByte(0x00);
            }
            Content.Position = 0;
            int Readed = 0;
            byte[] Buffer = new byte[1024];
            do {
                Readed = Content.Read(Buffer, 0, Buffer.Length);
                Header.Write(Buffer, 0, Readed);
            } while (Readed > 0);
            Content.Close();
            return Output.ToArray();
        }

        private static byte[] GetSubArray(byte[] data, int offset, int length) {
            byte[] Rst = new byte[length];
            Array.Copy(data, offset, Rst, 0, length);
            return Rst;
        }
    }
}
