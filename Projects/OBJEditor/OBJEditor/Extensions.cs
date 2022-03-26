using System;
using System.IO;
using System.Text;

namespace OBJEditor {
    internal static class Extensions {
        static Encoding Encoding = Encoding.GetEncoding("UTF-16LE");
        internal static int GetInt32(this byte[] Arr, int At) => BitConverter.ToInt32(Arr, At);
        internal static int GetInt16(this byte[] Arr, int At) => BitConverter.ToInt16(Arr, At);

        internal static string GetString(this byte[] Arr, int At) {
            int StrLen = Arr.GetInt32(At);
            At += 4;

            byte[] Buffer = new byte[StrLen * 2];
            for (int i = 0; i < StrLen; i++) {
                Buffer[(i * 2)] = Arr[(i * 2) + At];
                Buffer[(i * 2) + 1] = Arr[(i * 2) + At + 1];
            }
            return Encoding.GetString(Buffer);
        }

        internal static void WriteTo(this string String, Stream Output) {
            byte[] Data = Encoding.GetBytes(String);
            BitConverter.GetBytes(Data.Length / 2).CopyTo(Output, 0, 4);
            Output.Write(Data, 0, Data.Length);
        }

        internal static void CopyTo(this byte[] Arr, Stream Buffer, int ReadIndex, int Length) => Buffer.Write(Arr, ReadIndex, Length);
    }
}
