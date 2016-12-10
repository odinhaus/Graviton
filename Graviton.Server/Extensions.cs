using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public static class NumbersEx
    {
        public static double Clamp(this double value, double min, double max)
        {
            if (value <= max && value >= min) return value;
            if (value > max) return max;
            return min;
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value <= max && value >= min) return value;
            if (value > max) return max;
            return min;
        }
    }

    public static class ByteEx
    {
        static readonly IFastCopier _copier;

        static AssemblyName _asmName = new AssemblyName() { Name = "FastCopier" };
        static ModuleBuilder _modBuilder;
        static AssemblyBuilder _asmBuilder;

        static ByteEx()
        {
            _asmBuilder = Thread.GetDomain().DefineDynamicAssembly(_asmName, AssemblyBuilderAccess.RunAndSave);
            _modBuilder = _asmBuilder.DefineDynamicModule(_asmName.Name, _asmName.Name + ".dll", false);

            var typeBuilder = _modBuilder.DefineType("FastCopier",
                       TypeAttributes.Public
                       | TypeAttributes.AutoClass
                       | TypeAttributes.AnsiClass
                       | TypeAttributes.Class
                       | TypeAttributes.Serializable
                       | TypeAttributes.BeforeFieldInit);
            typeBuilder.AddInterfaceImplementation(typeof(IFastCopier));
            var copyMethod = typeBuilder.DefineMethod("Copy",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                new Type[] { typeof(byte[]), typeof(byte[]), typeof(int), typeof(uint) });
            var code = copyMethod.GetILGenerator();

            code.Emit(OpCodes.Ldarg_2);
            code.Emit(OpCodes.Ldc_I4_0);
            code.Emit(OpCodes.Ldelema, typeof(byte));
            code.Emit(OpCodes.Ldarg_1);
            code.Emit(OpCodes.Ldarg_3);
            code.Emit(OpCodes.Ldelema, typeof(byte));
            code.Emit(OpCodes.Ldarg, 4);
            code.Emit(OpCodes.Cpblk);
            code.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(copyMethod, typeof(IFastCopier).GetMethod("Copy"));

            var copierType = typeBuilder.CreateType();
            _copier = (IFastCopier)Activator.CreateInstance(copierType);
        }


        public static void Copy(this byte[] src, int srcOffset, byte[] dst, uint count)
        {
            if (src == null || srcOffset < 0 ||
                dst == null || count < 0 || srcOffset > src.Length
                || count > dst.Length || count > src.Length)
            {
                throw new System.ArgumentException();
            }

            _copier.Copy(src, dst, srcOffset, count);
        }

        public static string ToBase16(this byte[] bytes)
        {
            return ToBase16(bytes, -1, -1);
        }

        public static string ToBase16(this byte[] bytes, int maxLength)
        {
            return ToBase16(bytes, maxLength, -1);
        }

        /// <summary>
        /// Coverts the provided byte array to a base16 string of the given max length.  If the converted string exceeds maxLength in length,
        /// the remain characters are truncated from the returned string.  Specify -1 to avoid truncation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string ToBase16(this byte[] bytes, int maxLength, int dashPosition)
        {
            string hash = Convert.ToBase64String(bytes);
            return hash.ToBase16(maxLength, dashPosition);
        }
    }

    public interface IFastCopier
    {
        void Copy(byte[] source, byte[] dest, int offset, uint count);
    }


    public static class StringEx
    {
        private static Regex _dtRegex = new Regex(@"(?<Year>\d{4})-(?<Month>\d{2})-(?<Day>\d{2}) +(?<Hour>\d{2}):(?<Minute>\d{2}):(?<Second>\d{2}).?(?<Millisecond>\d{0,3})");
        public static string StripSpecial(this string source, string replacement)
        {
            Regex r = new Regex(@"\W");
            return r.Replace(source, replacement);
        }

        public static string ToArgs(this string[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in args)
            {
                sb.Append(s);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public static string GetArgValue(this string delimitedString, string name)
        {
            delimitedString = delimitedString.Right('?');
            string[] nvps = delimitedString.Split('&');
            for (int i = 0; i < nvps.Length; i++)
            {
                string[] nvp = nvps[i].Split('=');
                if (nvp[0].Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return nvp[1];
            }
            return string.Empty;
        }

        //public static string GetArgValue(this string[] nvps, string name)
        //{
        //    string nvp = nvps.Where(s => s.Split(':')[0].Replace("-", "").Trim().Equals(name.Trim(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        //    if (string.IsNullOrEmpty(nvp))
        //    {
        //        var val = AppContext.GetEnvironmentVariable<string>(name, null);
        //        if (val == null) return null;
        //        else return val.ToString();
        //    }
        //    string[] parts = nvp.Split(':');
        //    if (parts.Length >= 2)
        //    {
        //        string val = "";
        //        for (int i = 1; i < parts.Length; i++)
        //        {
        //            if (i > 1) val += ":";
        //            val += parts[i].Trim();
        //        }
        //        return val;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        //public static string GetArgValue(this string[] nvps, string name, string defaultValue)
        //{
        //    string ret = GetArgValue(nvps, name);
        //    if (string.IsNullOrEmpty(ret)) return defaultValue;
        //    else return ret;
        //}

        //public static bool HasArgValue(this string[] nvps, string name)
        //{
        //    return GetArgValue(nvps, name) != null;
        //}

        public static string GetArgName(this string[] nvps, int index)
        {
            return nvps[index].Split(':')[0].Replace("-", "").Trim();
        }

        public static string GetArgValue(this string[] nvps, int index)
        {
            return nvps[index].Split(':')[1].Trim();
        }

        public static string Right(this string searchString, char searchChar)
        {
            int idx = searchString.LastIndexOf(searchChar);
            string retString = searchString;
            if (idx >= 0)
            {
                retString = searchString.Substring(idx + 1, searchString.Length - idx - 1);
            }
            return retString;
        }

        public static string Right(this string searchString, string searchChars)
        {
            int idx = searchString.LastIndexOf(searchChars);
            string retString = searchString;
            if (idx >= 0)
            {
                idx += searchChars.Length - 1;
                retString = searchString.Substring(idx + 1, searchString.Length - idx - 1);
            }
            return retString;
        }

        public static string Right(this string searchString, int length)
        {
            return searchString.Substring(searchString.Length - length);
        }

        public static string Left(this string searchString, string searchChars)
        {
            int idx = searchString.LastIndexOf(searchChars);
            string retString = searchString;
            if (idx >= 0)
            {
                retString = searchString.Substring(0, idx);
            }
            else
            {
                retString = string.Empty;
            }
            return retString;
        }

        public static string Left(this string searchString, int length)
        {
            return searchString.Substring(0, length);
        }

        public static System.DateTime FromANSI(this string ansiDateTime)
        {
            Match m = _dtRegex.Match(ansiDateTime);
            if (m.Success)
            {
                return new System.DateTime(int.Parse(m.Groups["Year"].Value),
                    int.Parse(m.Groups["Month"].Value),
                    int.Parse(m.Groups["Day"].Value),
                    int.Parse(m.Groups["Hour"].Value),
                    int.Parse(m.Groups["Minute"].Value),
                    int.Parse(m.Groups["Second"].Value),
                    int.Parse(string.IsNullOrEmpty(m.Groups["Millisecond"].Value) ? "0" : m.Groups["Millisecond"].Value)).ToLocalTime();
            }
            else
            {
                throw new InvalidOperationException("DateTime did not match ANSI format.");
            }
        }

        public static string ToBase16(this string hash)
        {
            return ToBase16(hash, -1, -1);
        }

        public static string ToBase16(this string hash, int maxLength)
        {
            return ToBase16(hash, maxLength, -1);
        }

        public static string ToBase16(this string hash, int maxLength, int dashPosition)
        {
            byte[] base16 = new byte[hash.Length * 8];
            for (int i = 0; i < hash.Length; i++)
            {
                char c = hash[i];
                ToBase16(c).CopyTo(base16, i * 8);
            }
            return ASCIIEncoding.ASCII.GetString(RLE(base16, maxLength, dashPosition));
        }

        private static byte[] RLE(byte[] base16, int maxLength, int dashPosition)
        {
            List<Byte> rle = new List<byte>();
            if (maxLength < 0) maxLength = int.MaxValue;
            byte lastByte = 0;
            int lastCount = 0;
            int charCount = 0;
            for (int i = 0; i < base16.Length; i++)
            {
                if (base16[i] == lastByte)
                {
                    lastCount++;
                }
                else
                {
                    if (lastByte != 0)
                    {
                        if (lastCount > 1)
                        {
                            rle.Add(ASCIIEncoding.ASCII.GetBytes(lastCount.ToString())[0]);
                            charCount++;
                            if (charCount >= maxLength) break;
                            if (dashPosition > 0 && charCount % dashPosition == 0)
                                rle.Add(45);
                        }

                        rle.Add(lastByte);
                        charCount++;
                        if (charCount >= maxLength) break;
                        if (dashPosition > 0 && charCount % dashPosition == 0)
                            rle.Add(45);
                    }
                    lastByte = base16[i];
                    lastCount = 1;
                }
            }

            if (rle.Last() == 45)
                rle.RemoveAt(rle.Count - 1);

            return rle.ToArray();
        }

        private static byte[] ToBase16(char c)
        {
            byte[] base16 = new byte[] { 48, 48, 48, 48, 48, 48, 48, 48 };
            int ptr = 0;
            int rem = c;
            while (rem > 0)
            {
                byte b = (byte)(rem > 15 ? 15 : rem);
                rem -= (char)(b + 1);
                if (b > 9)
                {
                    switch (b)
                    {
                        case 10:
                            {
                                b = ASCIIEncoding.ASCII.GetBytes("A")[0];
                                break;
                            }
                        case 11:
                            {
                                b = ASCIIEncoding.ASCII.GetBytes("B")[0];
                                break;
                            }
                        case 12:
                            {
                                b = ASCIIEncoding.ASCII.GetBytes("C")[0];
                                break;
                            }
                        case 13:
                            {
                                b = ASCIIEncoding.ASCII.GetBytes("D")[0];
                                break;
                            }
                        case 14:
                            {
                                b = ASCIIEncoding.ASCII.GetBytes("E")[0];
                                break;
                            }
                        case 15:
                            {
                                b = ASCIIEncoding.ASCII.GetBytes("F")[0];
                                break;
                            }
                    }
                }
                else
                {
                    b = ASCIIEncoding.ASCII.GetBytes(b.ToString())[0];
                }

                base16[ptr] = b;
                ptr++;
            }
            return base16;
        }

        public static int IndexOfPrevious(this string source, int start, string searchText, params string[] stopWords)
        {
            var searchLength = searchText.Length;
            var s = start - searchText.Length;
            while (s >= 0)
            {
                if (source.Substring(s, searchLength) == searchText)
                {
                    return s;
                }
                else if (stopWords.Any(sw => source.Substring(s, Math.Min(source.Length - s, sw.Length)).Equals(sw, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return -1;
                }
                s--;
            }

            return s;
        }

        public static int CountOfPrevious(this string source, int start, string searchText, params string[] stopWords)
        {
            int index = start;
            int count = 0;
            do
            {

                index = IndexOfPrevious(source, index, searchText, stopWords);
                if (index >= 0)
                    count++;

            } while (index >= 0);
            return count;
        }
    }

    public static class HashingEx
    {
        public static string ToBase64SHA1(this string value)
        {
            var hasher = System.Security.Cryptography.SHA1.Create();
            return System.Convert.ToBase64String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
        }

        public static string ToBase64MD5(this string value)
        {
            var hasher = System.Security.Cryptography.MD5.Create();
            return System.Convert.ToBase64String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
        }

        public static string ToBase64SHA256(this string value)
        {
            var hasher = new System.Security.Cryptography.SHA256CryptoServiceProvider();
            return System.Convert.ToBase64String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
        }

        public static string ToBase64SHA1(this byte[] value)
        {
            var hasher = System.Security.Cryptography.SHA1.Create();
            return System.Convert.ToBase64String(hasher.ComputeHash(value));
        }

        public static string ToBase64MD5(this byte[] value)
        {
            var hasher = System.Security.Cryptography.MD5.Create();
            return System.Convert.ToBase64String(hasher.ComputeHash(value));
        }

        public static string ToBase64SHA256(this byte[] value)
        {
            var hasher = new System.Security.Cryptography.SHA256CryptoServiceProvider();
            return System.Convert.ToBase64String(hasher.ComputeHash(value));
        }
    }


    public unsafe static class Extensions
    {
        public static string ToISO8601(this DateTime datetime)
        {
            return datetime.ToUniversalTime().ToString("o");
        }
        public static bool TryCast<T>(this object value, out T cast)
        {
            cast = default(T);
            if (value == null) return true;

            try
            {
                if (value is string && typeof(T) == typeof(DateTime))
                {
                    cast = (T)(object)DateTime.Parse(value.ToString());
                    return true;
                }

                var p = Expression.Parameter(value.GetType());
                var conv = Expression.Convert(p, typeof(T));
                var func = typeof(Func<,>).MakeGenericType(value.GetType(), typeof(T));
                var lambda = Expression.Lambda(func, conv, p).Compile();
                cast = (T)lambda.DynamicInvoke(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static byte[] GetBytes(this IComparable value)
        {
            if (value is bool)
                return GetBytes((bool)value);
            else if (value is byte)
                return GetBytes((byte)value);
            else if (value is char)
                return GetBytes((char)value);
            else if (value is ushort)
                return GetBytes((ushort)value);
            else if (value is short)
                return GetBytes((short)value);
            else if (value is uint)
                return GetBytes((uint)value);
            else if (value is int)
                return GetBytes((int)value);
            else if (value is float)
                return GetBytes((float)value);
            else if (value is ulong)
                return GetBytes((ulong)value);
            else if (value is long)
                return GetBytes((long)value);
            else if (value is double)
                return GetBytes((double)value);
            else if (value is decimal)
                return GetBytes((decimal)value);
            else if (value is string)
                return GetBytes((string)value);
            else if (value is DateTime)
                return GetBytes((DateTime)value);
            else
                throw (new InvalidOperationException("Type not supported"));
        }

        public static byte[] GetBytes(this bool value)
        {
            byte[] bytes = new byte[1];
            fixed (byte* ptr = bytes)
            {
                *(bool*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this byte value)
        {
            byte[] bytes = new byte[1];
            fixed (byte* ptr = bytes)
            {
                *(byte*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this char value)
        {
            byte[] bytes = new byte[2];
            fixed (byte* ptr = bytes)
            {
                *(char*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this short value)
        {
            byte[] bytes = new byte[2];
            fixed (byte* ptr = bytes)
            {
                *(short*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this ushort value)
        {
            byte[] bytes = new byte[2];
            fixed (byte* ptr = bytes)
            {
                *(ushort*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this int value)
        {
            byte[] bytes = new byte[4];
            fixed (byte* ptr = bytes)
            {
                *(int*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this uint value)
        {
            byte[] bytes = new byte[4];
            fixed (byte* ptr = bytes)
            {
                *(uint*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this float value)
        {
            byte[] bytes = new byte[4];
            fixed (byte* ptr = bytes)
            {
                *(float*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this long value)
        {
            byte[] bytes = new byte[8];
            fixed (byte* ptr = bytes)
            {
                *(long*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this ulong value)
        {
            byte[] bytes = new byte[8];
            fixed (byte* ptr = bytes)
            {
                *(ulong*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this double value)
        {
            byte[] bytes = new byte[8];
            fixed (byte* ptr = bytes)
            {
                *(double*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this decimal value)
        {
            byte[] bytes = new byte[8];
            fixed (byte* ptr = bytes)
            {
                *(decimal*)ptr = value;
                return bytes;
            }
        }

        public static byte[] GetBytes(this DateTime value)
        {
            return value.ToBinary().GetBytes();
        }

        public static byte[] GetBytes(this string value)
        {
            int count = Encoding.Unicode.GetByteCount(value);
            byte[] text = new byte[4 + count];
            count.GetBytes(ref text);
            Encoding.Unicode.GetBytes(value).CopyTo(text, 4);
            return text;
        }

        //========================


        public static void GetBytes(this bool value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(bool*)ptr = value;
            }
        }

        public static void GetBytes(this byte value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(byte*)ptr = value;
            }
        }

        public static void GetBytes(this char value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(char*)ptr = value;
            }
        }

        public static void GetBytes(this short value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(short*)ptr = value;
            }
        }

        public static void GetBytes(this ushort value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(ushort*)ptr = value;
            }
        }

        public static void GetBytes(this int value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(int*)ptr = value;
            }
        }

        public static void GetBytes(this uint value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(uint*)ptr = value;
            }
        }

        public static void GetBytes(this float value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(float*)ptr = value;
            }
        }

        public static void GetBytes(this long value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(long*)ptr = value;
            }
        }

        public static void GetBytes(this ulong value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(ulong*)ptr = value;
            }
        }

        public static void GetBytes(this double value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(double*)ptr = value;
            }
        }

        public static void GetBytes(this decimal value, ref byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                *(decimal*)ptr = value;
            }
        }

        public static void GetBytes(this DateTime value, ref byte[] bytes)
        {
            value.ToBinary().GetBytes(ref bytes);
        }


        //=======================

        public static char ToChar(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((char*)ptr2));
            }
        }

        public static ushort ToUInt16(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((ushort*)ptr2));
            }
        }

        public static short ToInt16(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((short*)ptr2));
            }
        }

        public static float ToSingle(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((float*)ptr2));
            }
        }

        public static double ToDouble(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((double*)ptr2));
            }
        }


        public static uint ToUInt32(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((uint*)ptr2));
            }
        }

        public static int ToInt32(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((int*)ptr2));
            }
        }

        public static ulong ToUInt64(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((ulong*)ptr2));
            }
        }

        public static long ToInt64(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((long*)ptr2));
            }
        }

        public static decimal ToDecimal(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((decimal*)ptr2));
            }
        }

        public static bool ToBoolean(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return *(((bool*)ptr2));
            }
        }

        public static DateTime ToDateTime(this byte[] bytes, int offset = 0)
        {
            fixed (byte* ptr = bytes)
            {
                byte* ptr2 = ptr;
                ptr2 += offset;
                return DateTime.FromBinary(*(((long*)ptr2)));
            }
        }

        public static bool IsNumeric(this object value)
        {
            if (value == null) return false;
            if (value is byte
                || value is ushort
                || value is short
                || value is uint
                || value is int
                || value is ulong
                || value is long
                || value is float
                || value is double
                || value is decimal)
                return true;
            return false;
        }

        public static bool IsNumeric(this Type value)
        {
            if (value == null) return false;
            if (value == typeof(byte)
                || value == typeof(ushort)
                || value == typeof(short)
                || value == typeof(uint)
                || value == typeof(int)
                || value == typeof(ulong)
                || value == typeof(long)
                || value == typeof(float)
                || value == typeof(double)
                || value == typeof(decimal))
                return true;
            return false;
        }

        public static bool IsDateTime(this Type value)
        {
            return value.Equals(typeof(DateTime));
        }

        public static bool IsZero(this object value)
        {
            if (value is byte)
                return (byte)value == (byte)0;
            else if (value is ushort)
                return (ushort)value == (ushort)0;
            else if (value is short)
                return (short)value == (short)0;
            else if (value is uint)
                return (uint)value == (uint)0;
            else if (value is int)
                return (int)value == (int)0;
            else if (value is ulong)
                return (ulong)value == (ulong)0;
            else if (value is long)
                return (long)value == (long)0;
            else if (value is float)
                return (float)value == (float)0;
            else if (value is double)
                return (double)value == (double)0;
            else if (value is decimal)
                return (decimal)value == (decimal)0;
            else if (value is DateTime)
                return ((DateTime)value).Ticks == 0;
            else
                return value == null;
        }
    }
}
