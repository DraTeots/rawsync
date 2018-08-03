using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RawSync;

class Program
{
    private static readonly KeyValuePair<long, string>[] Thresholds =
    {
        // new KeyValuePair<long, string>(0, " Bytes"), // Don't devide by Zero!
        new KeyValuePair<long, string>(1, " Byte"),
        new KeyValuePair<long, string>(2, " Bytes"),
        new KeyValuePair<long, string>(1024, " KB"),
        new KeyValuePair<long, string>(1048576, " MB"), // Note: 1024 ^ 2 = 1026 (xor operator)
        new KeyValuePair<long, string>(1073741824, " GB"),
        new KeyValuePair<long, string>(1099511627776, " TB"),
        new KeyValuePair<long, string>(1125899906842620, " PB"),
        new KeyValuePair<long, string>(1152921504606850000, " EB"),

        // These don't fit into a int64
        // new KeyValuePair<long, string>(1180591620717410000000, " ZB"), 
        // new KeyValuePair<long, string>(1208925819614630000000000, " YB") 
    };

    /// <summary>
    /// Returns x Bytes, kB, Mb, etc... 
    /// </summary>
    public static string ToByteSize(long value)
    {
        if (value == 0) return "0 Bytes"; // zero is plural
        for (int t = Thresholds.Length - 1; t > 0; t--)
            if (value >= Thresholds[t].Key) return ((double)value / Thresholds[t].Key).ToString("0.00") + Thresholds[t].Value;
        return "-" + ToByteSize(-value); // negative bytes (common case optimised to the end of this routine)
    }

    static void Main(string[] args)
    {
        // DirectoryInfo diTop = new DirectoryInfo(@"e:\");
        var sf = new SafeFileEnumerator(@"e:\Sync\", @"*.NEF");

        long sum = 0;
        try
        {
            foreach (var path  in sf)
            {
                var nefFileInfo = new FileInfo(path);
                var directoryInfo = nefFileInfo.Directory;
                if (directoryInfo == null) continue;
                //directoryInfo.WriteLine(directoryInfo.FullName);
                if (directoryInfo.Name.ToLower() == "raw" && directoryInfo.Parent!=null)
                {
                    var jpgPath = Path.Combine(directoryInfo.Parent.FullName,
                        Path.GetFileNameWithoutExtension(path) + ".JPG");
                    if (!File.Exists(jpgPath))
                    { 
                        Console.WriteLine(nefFileInfo.FullName);
                        sum += nefFileInfo.Length;
                    }
                }
            }
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            string formatted = (sum/1000.0).ToString("#,0.00", nfi); // "1 234 897.11"

            Console.WriteLine(ToByteSize(sum));
//            foreach (var di in diTop.EnumerateDirectories("?aw", SearchOption.AllDirectories))
//            {
//                try
//                {
//                    Console.WriteLine($"{di.FullName}");
//                    Console.WriteLine($"{di.Parent?.FullName}");
//                }
//                catch (UnauthorizedAccessException unAuthTop)
//                {
//                    Console.WriteLine("{0}", unAuthTop.Message);
//                }
//            }

//            foreach (var di in diTop.EnumerateDirectories("*"))
//            {
//                try
//                {
//                    foreach (var fi in di.EnumerateFiles("*", SearchOption.AllDirectories))
//                    {
//                        try
//                        {
//                            // Display each file over 10 MB; 
//                            if (fi.Length > 10000000)
//                            {
//                                Console.WriteLine("{0}\t\t{1}", fi.FullName, fi.Length.ToString("N0"));
//                            }
//                        }
//                        catch (UnauthorizedAccessException UnAuthFile)
//                        {
//                            Console.WriteLine("UnAuthFile: {0}", UnAuthFile.Message);
//                        }
//                    }
//                }
//                catch (UnauthorizedAccessException UnAuthSubDir)
//                {
//                    Console.WriteLine("UnAuthSubDir: {0}", UnAuthSubDir.Message);
//                }
//            }
        }
        catch (DirectoryNotFoundException DirNotFound)
        {
            Console.WriteLine("{0}", DirNotFound.Message);
        }
        catch (UnauthorizedAccessException UnAuthDir)
        {
            Console.WriteLine("UnAuthDir: {0}", UnAuthDir.Message);
        }
        catch (PathTooLongException LongPath)
        {
            Console.WriteLine("{0}", LongPath.Message);
        }
    }
}