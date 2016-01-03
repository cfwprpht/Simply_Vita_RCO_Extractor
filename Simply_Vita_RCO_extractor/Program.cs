using System;
using System.Diagnostics;
using System.IO;
using ZLibNet;

namespace Simply_NextGen_RCO_Extractor
{
    class Program
    {
        #region vars
        private static byte[] gimMagic = new byte[16] { 0x4D, 0x49, 0x47, 0x2E, 0x30, 0x30, 0x2E, 0x31, 0x50, 0x53, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, };
        private static byte[] vagEnd = new byte[16] { 0x00, 0x07, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, };
        private static byte[] pngMagic = new byte[16] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, };
        private static byte[] ngrcoMagic = new byte[8] { 0x52, 0x43, 0x4F, 0x46, 0x10, 0x01, 0x00, 0x00, };
        private static byte[] ngCXML = new byte[8] { 0x52, 0x43, 0x53, 0x46, 0x10, 0x01, 0x00, 0x00, };
        private static byte[] vagMagic = new byte[8] { 0x56, 0x41, 0x47, 0x70, 0x00, 0x02, 0x00, 0x01, };
        private static byte[] ddsMagic = new byte[4] { 0x44, 0x44, 0x53, 0x20, };
        private static byte[] wavMagic = new byte[4] { 0x52, 0x49, 0x46, 0x46, };
        private static byte[] gtfMagic = new byte[4] { 0x02, 0x02, 0x00, 0xFF, };
        private static byte[] zlibMagic = new byte[3] { 0x00, 0x78, 0xDA, };
        private static byte[] singlZL = new byte[2] { 0x78, 0xDA, };
        private static byte[] _vag = new byte[0];
        private static byte[] _png = new byte[0];
        private static byte[] _cxml = new byte[0];
        private static byte[] _zlib = new byte[0];
        private static byte[] _wav = new byte[0];
        private static byte[] _gtf = new byte[0];
        private static byte[] _dds = new byte[0];
        private static byte[] zlib = new byte[2];
        private static byte[] vag = new byte[8];
        private static byte[] cxml = new byte[8];
        private static byte[] png = new byte[16];
        private static byte[] temp = new byte[1];
        private static byte[] dds = new byte[4];
        private static byte[] gtf = new byte[4];
        private static byte[] wav = new byte[4];
        private static int i = 0;
        private static int dumped = 0;
        private static int end = 0;
        private static int count = 0;
        private static int countVag = 0;
        private static int countCXML = 0;
        private static int countGim = 0;
        private static int countDDS = 0;
        private static int countPNG = 0;
        private static int countGTF = 0;
        private static int countWAV = 0;
        private static int countZLIB = 0;
        private static string GimConv = @"rsc\GimConv.exe";
        private static string Vag2Wav = @"rsc\vag2wav.exe";
        private static string Dds2Gtf = @"rsc\dds2gtf.exe";
        private static string baseDir = "";
        private static string convDir = "";
        private static string corExt = "";
        private static string move = "";
        private static string dest = "";
        #endregion vars

        /// <summary>
        /// Automatical set the folder ico on Start of app
        /// *broken need a fix*
        /// </summary>
        private static void SetFolderIco()
        {
            try
            {
                string str = Directory.GetCurrentDirectory() + @"\rsc\svre.ico,0";
                string _str = Directory.GetCurrentDirectory() + @"\desktop.ini";
                string[] setFolderIco = { "[.ShellClassInfo]",
                                          "IconResource=" + str,
                                          "[ViewState]",
                                          "Mode=",
                                          "Vid=",
                                          "FolderType=Generic"
                                        };

                if (!File.Exists(_str))
                    File.Create(_str).Close();

                FileInfo fileInfo = new FileInfo(_str);
                fileInfo.Attributes = FileAttributes.Normal;
                File.WriteAllLines(_str, setFolderIco);
                fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive;
            }
            catch (Exception e) { Console.WriteLine("ERROR:\n" + e.ToString()); }
        }

        // Show Version and such....
        private static void ShowVersion()
        {
            Console.WriteLine("\nSimply NextGen RCO Data-Table Extractor v1.20");
            Console.WriteLine("by cfwprophet\n\nGreetz goes out too:");
            Console.WriteLine(" flatz, CTurt, ZiNgA BuRgA for the very first psp RCO-Editor");
            Console.WriteLine("  eussNL, GregoryRasputin, Joonie, Sandungas, Ada,\n   The whole Wii U Scene - keep the good work up guys :)");
            Console.WriteLine("    Juanadie, grafchockolo, nwert, oh and GeoHot...Fuck You ;)\n     We are still here, writing History and you're gone to be History :D\n");
        }

        // Show Help Screen
        private static void ShowUsage()
        {
            Console.WriteLine("Usage: sngre.exe -f <input_file>");
        }

        // Check input if it is a file
        private static bool CheckInput(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Wrong input!");
                return false;
            }
            else if (args[0] == "-f" && File.Exists(args[1]))
            {
                Console.WriteLine("File found!");
                return true;
            }
            else if (args[0] == "-f" && !File.Exists(args[1]))
            {
                Console.WriteLine("Can not find/access file!");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Compare Byte by Byte or Array by Array
        /// </summary>
        /// <param name="bA1">Byte Array 1</param>
        /// <param name="bA2">Byte Array 2</param>
        /// <returns>True if both Byte Array's do match</returns>
        private static bool CompareBytes(byte[] bA1, byte[] bA2)
        {
            int s = 0;
            for (int z = 0; z < bA1.Length; z++)
            {
                if (bA1[z] != bA2[z])
                    s++;
            }

            if (s == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Decompress a zlib File
        /// </summary>
        /// <param name="fileToDeCompress">The file you want to Decompress</param>
        private static void ZLibDeCompress(string fileToDeCompress)
        {
            try 
            {
                Console.Write("Decompressing File...");

                using (Stream input = File.OpenRead(fileToDeCompress))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (Stream output = new ZLibStream(input, CompressionMode.Decompress))
                        {
                            using (var fileRenamed = File.Create(fileToDeCompress + ".decompressed"))
                            {
                                // this is a variable for the Buffer size. Play arround with it and maybe set a new size to get better result's
                                int workingBufferSize = 4096; // high
                                // int workingBufferSize = 2048; // middle
                                // int workingBufferSize = 1024; // default
                                // int workingBufferSizeE = 128;  // minimum

                                byte[] buffer = new byte[workingBufferSize];
                                int len;
                                while ((len = output.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    mem.Write(buffer, 0, len);
                                }
                                mem.WriteTo(fileRenamed);
                                fileRenamed.Close();
                            }
                            output.Close();
                        }
                        mem.Flush();
                    }
                    input.Close();
                }

                Console.Write("done!\nCleaning compressed dump...");
                File.Delete(fileToDeCompress);
                Console.Write("done!\n");
            }
            catch (Exception e) 
            { 
                Console.WriteLine("\nERROR: \n" + e.ToString());
                Environment.Exit(0);
            }
        }

        // Convert .GIM to .PNG
        private static void ConvertGIM(string file)
        {
            try 
            {
                Console.Write("Converting .GIM to .PNG...");
                ProcessStartInfo _GimConv = new ProcessStartInfo(GimConv);
                _GimConv.Arguments = file + " -o " + file.Replace(".gim", ".png");
                _GimConv.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(_GimConv);
                Console.Write("done!\n");
            }
            catch (Exception e) 
            { 
                Console.WriteLine("\nERROR:\n" + e.ToString());
                Environment.Exit(0);
            }
        }

        // Convert .VAG to .WAV
        private static void ConvertVAG(string file)
        {
            try 
            {
                Console.Write("Converting .VAG to .WAV...");
                ProcessStartInfo _Vag2Wav = new ProcessStartInfo(Vag2Wav);
                _Vag2Wav.Arguments = file + " " + file.Replace(".vag", ".wav");
                _Vag2Wav.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(_Vag2Wav);
                Console.Write("done!\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("\nERROR:\n" + e.ToString());
                Environment.Exit(0);
            }
        }

        // Convert .DDS to .GTF
        private static void ConvertDDS(string file)
        {
            try
            {
                Console.Write("Converting DDS to GTF...");
                ProcessStartInfo _Dds2Gtf = new ProcessStartInfo(Dds2Gtf);
                _Dds2Gtf.WindowStyle = ProcessWindowStyle.Hidden;
                _Dds2Gtf.Arguments = "-o " + file.Replace(".dds", ".gtf") + " " + file;
                Process.Start(_Dds2Gtf);
                Console.Write("done!\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("\nERROR:\n" + e.ToString());
                Environment.Exit(0);
            }

        }

        // Create a new Dir for our RCO to extract
        private static void MakeDIR(string file)
        {
            try
            {
                Console.Write("Creating Extraction Directory...");
                int found = 0;
                string fileToUse = file.Replace(".rco", "").Replace(".RCO", "");
                string fileToUse2 = file.Replace(".rco", "_").Replace(".RCO", "_");
                string[] toCheck = Directory.GetDirectories(Directory.GetCurrentDirectory());

                if (toCheck.Length > 0)
                {
                    foreach (string line in toCheck)
                    {
                        if (line.Contains(fileToUse))
                            found++;
                    }
                    if (found > 0)
                        baseDir = Directory.GetCurrentDirectory() + @"\" + fileToUse2 + (found +1);
                    else
                        baseDir = Directory.GetCurrentDirectory() + @"\" + fileToUse;
                }
                else
                    baseDir = Directory.GetCurrentDirectory() + @"\" + fileToUse;

                Directory.CreateDirectory(baseDir);
                Console.Write("done!\n");
                baseDir = baseDir + @"\";
                convDir = baseDir + @"converted\";
                Directory.CreateDirectory(convDir);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nERROR:\n" + e.ToString());
                Environment.Exit(0);
            }
        }

        // Extract Data
        private static void Extract(string file)
        {
            try
            {
                // Reading Header
                byte[] magic = new byte[8];
                byte[] offset = new byte[4];
                string outFile = "notDefined";
                using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
                {
                    // Check Magic
                    Console.Write("Checking Header....");
                    br.Read(magic, 0, 8);

                    if (!CompareBytes(magic, ngrcoMagic))
                    {
                        Console.WriteLine("ERROR: That is not a valid NextGen RCO!\nExiting now...");
                        Environment.Exit(0);
                    }

                    Console.Write("Magic OK!\nThat's a NextGen RCO :)\n");

                    // Get Data Table Offset and Length
                    Console.Write("Reading Offset and Length of Data Table...");
                    offset = new byte[4];
                    byte[] eof = new byte[4];
                    br.BaseStream.Seek(0x48, SeekOrigin.Begin);
                    br.Read(offset, 0, 4);
                    br.Read(eof, 0, 4);
                    Array.Reverse(offset);
                    Array.Reverse(eof);
                    Console.Write("done!\n");
                    Console.WriteLine("Readed Hex value of Offset: 0x" + BitConverter.ToString(offset).Replace("-", ""));
                    Console.WriteLine("Readed Hex value of Size: 0x" + BitConverter.ToString(eof).Replace("-", ""));

                    // Check for zlib Header '0x78DA' (compression level=9) or VAG & PNG files and write to file
                    
                    end = Convert.ToInt32(BitConverter.ToString(eof).Replace("-", ""), 16);
                    count = Convert.ToInt32(BitConverter.ToString(offset).Replace("-", ""), 16);
                    Console.WriteLine("Offset to start from: " + count + " bytes");
                    Console.WriteLine("Size to Dump: " + end + " bytes");
                    Console.WriteLine("Searching for ZLib Compressed (Vita) or Non-Compressed (PS4) Files...");
                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                    br.Read(zlib, 0, 2);

                    // main loop
                    if (!CompareBytes(zlib, singlZL))
                    {
                        temp = new byte[16];
                        while ((i = br.Read(temp, 0, 16)) != 0)
                        {
                            // In case of we now also have PS4 RCO's to work down and to not compromise the routine, we swapped the Extraction here 
                            // and placed the search for zlib files under the VAG and PNG file search
                            // For ZLib i removed the second routine that would read after the first Zlib compressed block, adding a 0 byte 0x00 on top of 0x78DA
                            // Instead of that, we simple counted +1 byte on end of dumping process and continue as usually

                            // Now we first fill the buffer's for the header's which we will compare after
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(vag, 0, 8);
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(cxml, 0, 8);
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(zlib, 0, 2);
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(png, 0, 16);
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(dds, 0, 4);
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(gtf, 0, 4);
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(wav, 0, 4);

                            #region vagExtract
                            if (CompareBytes(vag, vagMagic))
                            {
                                Console.Write("Found a VAG File will start to extract...");
                                outFile = baseDir + countVag + ".vag";
                                File.Create(outFile).Close();
                                byte[] toWrite = new byte[16];
                                _zlib = new byte[2];
                                _wav = new byte[4];
                                _gtf = new byte[4];
                                _dds = new byte[4];
                                _cxml = new byte[8];
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(toWrite, 0, 16);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    bw.Write(toWrite, 0, 16);
                                    dumped += 16;
                                    count += 16;
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_zlib, 0, 2);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_dds, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_gtf, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_wav, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_cxml, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(toWrite, 0, 16);

                                    while (true)
                                    {
                                        if (!CompareBytes(toWrite, vagEnd))
                                        {
                                            if (!CompareBytes(_cxml, ngCXML))
                                            {
                                                if (!CompareBytes(toWrite, pngMagic))
                                                {
                                                    if (!CompareBytes(_zlib, singlZL))
                                                    {
                                                        if (!CompareBytes(_wav, wavMagic))
                                                        {
                                                            if (!CompareBytes(_dds, ddsMagic))
                                                            {
                                                                if (!CompareBytes(_gtf, gtfMagic))
                                                                {
                                                                    bw.Write(toWrite, 0, 16);
                                                                    dumped += 16;
                                                                    count += 16;
                                                                    if (dumped != end)
                                                                    {
                                                                        br.Read(_zlib, 0, 2);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_dds, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_gtf, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_wav, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_cxml, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(toWrite, 0, 16);
                                                                    }
                                                                    else
                                                                        break;
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                        {
                                            // We reached the eof and loop was stopped. Now we need to write out the last 16 bytes which build the eof of a VAG file.
                                            bw.Write(toWrite, 0, 16);
                                            dumped += 16;
                                            count += 16;
                                            break;
                                        }
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");

                                // Convert VAG to WAV
                                ConvertVAG(outFile);
                                countVag++;
                            }
                            #endregion vagExtract
                            #region pngExtract
                            else if (CompareBytes(png, pngMagic))
                            {
                                Console.Write("Found a PNG File will start to extract...");
                                outFile = baseDir + countPNG + ".png";
                                File.Create(outFile).Close();
                                byte[] toWrite = new byte[16];
                                _zlib = new byte[2];
                                _wav = new byte[4];
                                _gtf = new byte[4];
                                _dds = new byte[4];
                                _vag = new byte[8];
                                _cxml = new byte[8];
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(toWrite, 0, 16);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    // Before we Jump into the Loop we need to write out the first readed 16 bytes which are the PNG Magic.
                                    // This is needed cause we need to compare for new Magic's / Header's to know if we reached the eof of current file.
                                    // Otherwise the routine would detect a PNG Magic and stop right after we jumped in, resulting in not extracting the PNG and loosing
                                    // the allready readed 16 bytes.
                                    bw.Write(toWrite, 0, 16);

                                    // count up the readed bytes and read next one before the loop start
                                    dumped += 16;
                                    count += 16;
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_zlib, 0, 2);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_dds, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_gtf, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_wav, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_vag, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_cxml, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(toWrite, 0, 16);

                                    // Now let's start the Loop and extract the PNG
                                    while (true)
                                    {
                                        // Have we reached EOF ?
                                        if (!CompareBytes(toWrite, pngMagic))
                                        {
                                            if (!CompareBytes(_cxml, ngCXML))
                                            {
                                                if (!CompareBytes(_zlib, singlZL))
                                                {
                                                    if (!CompareBytes(_vag, vagMagic))
                                                    {
                                                        if (!CompareBytes(_wav, wavMagic))
                                                        {
                                                            if (!CompareBytes(_dds, ddsMagic))
                                                            {
                                                                if (!CompareBytes(_gtf, gtfMagic))
                                                                {
                                                                    // Write out the readed data
                                                                    bw.Write(toWrite, 0, 16);
                                                                    dumped += 16;
                                                                    count += 16;
                                                                    if (dumped != end)
                                                                    {
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_zlib, 0, 2);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_dds, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_gtf, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_wav, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_vag, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_cxml, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(toWrite, 0, 16);
                                                                    }
                                                                    else
                                                                        break;
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");
                                countPNG++;
                            }
                            #endregion pngExtract
                            #region cxmlExtract
                            else if (CompareBytes(cxml, ngCXML))
                            {
                                Console.Write("Found a CXML File will start to extract...");
                                outFile = baseDir + countCXML + ".cxml";
                                File.Create(outFile).Close();
                                byte[] toWrite = new byte[16];
                                _vag = new byte[8];
                                _zlib = new byte[2];
                                _wav = new byte[4];
                                _gtf = new byte[4];
                                _dds = new byte[4];
                                _cxml = new byte[8];
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(toWrite, 0, 16);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    bw.Write(toWrite, 0, 16);
                                    dumped += 16;
                                    count += 16;
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_zlib, 0, 2);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_dds, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_gtf, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_wav, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_vag, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_cxml, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(toWrite, 0, 16);

                                    while (true)
                                    {
                                        if (!CompareBytes(_cxml, ngCXML))
                                        {
                                            if (!CompareBytes(toWrite, pngMagic))
                                            {
                                                if (!CompareBytes(_wav, wavMagic))
                                                {
                                                    if (!CompareBytes(_dds, ddsMagic))
                                                    {
                                                        if (!CompareBytes(_gtf, gtfMagic))
                                                        {
                                                            if (!CompareBytes(_zlib, singlZL))
                                                            {
                                                                if (!CompareBytes(_vag, vagMagic))
                                                                {
                                                                    if (dumped != end)
                                                                    {
                                                                        bw.Write(toWrite, 0, 16);
                                                                        dumped += 16;
                                                                        count += 16;
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_zlib, 0, 2);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_dds, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_gtf, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_wav, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_vag, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_cxml, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(toWrite, 0, 16);
                                                                    }
                                                                    else
                                                                        break;
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");
                                countCXML++;
                            }
                            #endregion cxmlExtract
                            #region ddsExtract
                            else if (CompareBytes(dds, ddsMagic))
                            {
                                Console.Write("Found a DDS File will start to extract...");
                                outFile = baseDir + countDDS + ".dds";
                                File.Create(outFile).Close();
                                byte[] toWrite = new byte[16];
                                _vag = new byte[8];
                                _zlib = new byte[2];
                                _wav = new byte[4];
                                _gtf = new byte[4];
                                _dds = new byte[4];
                                _cxml = new byte[8];
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(toWrite, 0, 16);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    bw.Write(toWrite, 0, 16);
                                    dumped += 16;
                                    count += 16;
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_zlib, 0, 2);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_dds, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_gtf, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_wav, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_vag, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_cxml, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(toWrite, 0, 16);

                                    while (true)
                                    {
                                        if (!CompareBytes(_cxml, ngCXML))
                                        {
                                            if (!CompareBytes(toWrite, pngMagic))
                                            {
                                                if (!CompareBytes(_wav, wavMagic))
                                                {
                                                    if (!CompareBytes(_dds, ddsMagic))
                                                    {
                                                        if (!CompareBytes(_gtf, gtfMagic))
                                                        {
                                                            if (!CompareBytes(_zlib, singlZL))
                                                            {
                                                                if (!CompareBytes(_vag, vagMagic))
                                                                {
                                                                    if (dumped != end)
                                                                    {
                                                                        bw.Write(toWrite, 0, 16);
                                                                        dumped += 16;
                                                                        count += 16;
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_zlib, 0, 2);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_dds, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_gtf, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_wav, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_vag, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_cxml, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(toWrite, 0, 16);
                                                                    }
                                                                    else
                                                                        break;
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");
                                countDDS++;
                            }
                            #endregion ddsExtract
                            #region gtfExtract
                            else if (CompareBytes(gtf, gtfMagic))
                            {
                                Console.Write("Found a GTF File will start to extract...");
                                outFile = baseDir + countGTF + ".gtf";
                                File.Create(outFile).Close();
                                byte[] toWrite = new byte[16];
                                _vag = new byte[8];
                                _zlib = new byte[2];
                                _wav = new byte[4];
                                _gtf = new byte[4];
                                _dds = new byte[4];
                                _cxml = new byte[8];
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(toWrite, 0, 16);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    bw.Write(toWrite, 0, 16);
                                    dumped += 16;
                                    count += 16;
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_zlib, 0, 2);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_dds, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_gtf, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_wav, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_vag, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_cxml, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(toWrite, 0, 16);

                                    while (true)
                                    {
                                        if (!CompareBytes(_cxml, ngCXML))
                                        {
                                            if (!CompareBytes(toWrite, pngMagic))
                                            {
                                                if (!CompareBytes(_wav, wavMagic))
                                                {
                                                    if (!CompareBytes(_dds, ddsMagic))
                                                    {
                                                        if (!CompareBytes(_gtf, gtfMagic))
                                                        {
                                                            if (!CompareBytes(_zlib, singlZL))
                                                            {
                                                                if (!CompareBytes(_vag, vagMagic))
                                                                {
                                                                    if (dumped != end)
                                                                    {
                                                                        bw.Write(toWrite, 0, 16);
                                                                        dumped += 16;
                                                                        count += 16;
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_zlib, 0, 2);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_dds, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_gtf, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_wav, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_vag, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_cxml, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(toWrite, 0, 16);
                                                                    }
                                                                    else
                                                                        break;
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");
                                countGTF++;
                            }
                            #endregion gtfExtract
                            #region wavExtract
                            else if (CompareBytes(wav, wavMagic))
                            {
                                Console.Write("Found a WAV File will start to extract...");
                                outFile = baseDir + countWAV + ".wav";
                                File.Create(outFile).Close();
                                byte[] toWrite = new byte[16];
                                _vag = new byte[8];
                                _zlib = new byte[2];
                                _wav = new byte[4];
                                _gtf = new byte[4];
                                _dds = new byte[4];
                                _cxml = new byte[8];
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(toWrite, 0, 16);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    bw.Write(toWrite, 0, 16);
                                    dumped += 16;
                                    count += 16;
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_zlib, 0, 2);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_dds, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_gtf, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_wav, 0, 4);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_vag, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(_cxml, 0, 8);
                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                    br.Read(toWrite, 0, 16);

                                    while (true)
                                    {
                                        if (!CompareBytes(_cxml, ngCXML))
                                        {
                                            if (!CompareBytes(toWrite, pngMagic))
                                            {
                                                if (!CompareBytes(_wav, wavMagic))
                                                {
                                                    if (!CompareBytes(_dds, ddsMagic))
                                                    {
                                                        if (!CompareBytes(_gtf, gtfMagic))
                                                        {
                                                            if (!CompareBytes(_zlib, singlZL))
                                                            {
                                                                if (!CompareBytes(_vag, vagMagic))
                                                                {
                                                                    if (dumped != end)
                                                                    {
                                                                        bw.Write(toWrite, 0, 16);
                                                                        dumped += 16;
                                                                        count += 16;
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_zlib, 0, 2);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_dds, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_gtf, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_wav, 0, 4);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_vag, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(_cxml, 0, 8);
                                                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                        br.Read(toWrite, 0, 16);
                                                                    }
                                                                    else
                                                                        break;
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");
                                countWAV++;
                            }
                            #endregion wavExtract
                            else
                            {
                                Console.WriteLine("\nFound a new File which i don't know what to do with !\nPlease contact the Developer @ www.playstationhax.it");
                                break;
                            }
                            if (dumped == end)
                                break;
                            else
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                        }
                    }
                    else if (CompareBytes(zlib, singlZL))
                    {
                        while ((i = br.Read(temp, 0, 1)) != 0)
                        {
                            #region zlibExtract
                            _zlib = new byte[3];
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(_zlib, 0, 3);
                            Console.Write("Found a ZLib Compressed File, starting to extract...");
                            byte[] toWrite = new byte[1];
                            outFile = baseDir + countZLIB + ".compressed";
                            corExt = "";
                            File.Create(outFile).Close();

                            using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                while (true)
                                {
                                    if (!CompareBytes(_zlib, zlibMagic))  // Next Byte is not the start of a Header from a other file ?
                                    {
                                        // write out data to file
                                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                                        br.Read(toWrite, 0, 1);
                                        bw.Write(toWrite, 0, 1);

                                        // Count up 1 and read the next byte(s) before the loop start again
                                        count++;
                                        dumped++;

                                        // Have we reached the end of data table?
                                        if (dumped != end)
                                        {
                                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                                            br.Read(_zlib, 0, 3);
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }

                                // In case of we need to compare zlibHeader with a additional 0x00 on top to know if it is really a zlibHeader and not just a 0x78DA data value
                                // we add that 0x00 on end of the extracted file and count dumped var +1
                                if (CompareBytes(_zlib, zlibMagic))
                                {
                                    toWrite = new byte[1];
                                    bw.Write(toWrite, 0, 1);
                                    dumped++;
                                    count++;
                                    countZLIB++;
                                }
                                bw.Close();
                            }
                            Console.Write("done!\n");

                            // Decompress dumped File
                            if (outFile == "notDefined")
                                Console.WriteLine("Found a Unknowen File!\nPlease contact the Developer on: www.playstationhax.it\n");
                            else
                                ZLibDeCompress(outFile);

                            // Check Header of Decompressed File and rename
                            Console.Write("Checking Header of Decompressed File...");
                            outFile = outFile + ".decompressed";
                            bool gHeader = false;
                            bool dHeader = false;
                            using (BinaryReader _br = new BinaryReader(new FileStream(outFile, FileMode.Open, FileAccess.Read)))
                            {
                                byte[] xmlHeader = new byte[8];
                                byte[] gimHeader = new byte[16];
                                byte[] ddsHeader = new byte[4];
                                _br.Read(xmlHeader, 0, 8);
                                _br.BaseStream.Seek(0, SeekOrigin.Begin);
                                _br.Read(gimHeader, 0, 16);
                                _br.BaseStream.Seek(0, SeekOrigin.Begin);
                                _br.Read(ddsHeader, 0, 4);

                                if (CompareBytes(xmlHeader, ngCXML))
                                {
                                    countCXML++;
                                    Console.Write("done!\nIt's a CXML Container.\n");
                                    corExt = outFile.Replace(".compressed.decompressed", ".cxml");

                                }
                                else if (CompareBytes(gimHeader, gimMagic))
                                {
                                    countGim++;
                                    gHeader = true;
                                    Console.Write("done!\nIt's a GIM File.\n");
                                    corExt = outFile.Replace(".compressed.decompressed", ".gim");
                                    move = corExt.Replace(".gim", ".png");
                                    dest = convDir + countGim + ".png";
                                }
                                else if (CompareBytes(ddsHeader, ddsMagic))
                                {
                                    countDDS++;
                                    dHeader = true;
                                    Console.Write("done!\nIt's a DDS Container.\n");
                                    corExt = outFile.Replace(".compressed.decompressed", ".dds");
                                    move = corExt.Replace(".dds", ".gtf");
                                    dest = convDir + countDDS + ".gtf";
                                }
                                else
                                    Console.Write("error!\nUnknown Header, please contact the developer...\n");
                                _br.Close();
                            }

                            // Finally Rename to the correct extension
                            Console.Write("Renaming " + "'" + outFile.Replace(baseDir, "") + "'" + " to " + "'" + corExt.Replace(baseDir, "") + "'...");
                            File.Move(outFile, corExt);
                            Console.Write("done!\n");

                            // Convert GIM to PNG or DDS to GTF
                            if (gHeader)
                            {
                                ConvertGIM(corExt);
                            }
                            else if (dHeader)
                            {
                                ConvertDDS(corExt);
                            }
                            #endregion ZlibExtract
                            // Have we dumped all data?
                            if (dumped == end)
                            {
                                Console.Write("Moving Converted Files to Extracted Folder...");
                                string fi = "";
                                string final = "";
                                string[] files = Directory.GetFiles(baseDir);

                                foreach (string s in files)
                                {
                                    if (s.Contains(".png") || s.Contains(".gtf"))
                                    {
                                        fi = s.Replace(baseDir, "");
                                        final = convDir + fi;
                                        File.Move(s, final);
                                    }
                                }
                                Console.Write("Done!\n");
                                break;
                            }
                            else
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                        }
                        br.Close();
                    }
                    else
                        Console.WriteLine("\nSomthing went wrong!\nPlease contact the developer @ www.playstationhax.it\nERROR: 1");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n\nERROR:\n" + e.ToString());
                Environment.Exit(0);
            }
        }

        // Main entry point
        static void Main(string[] args)
        {
            SetFolderIco();
            ShowVersion();
            if (!CheckInput(args))
            {
                ShowUsage();
                Environment.Exit(0);
            }
            MakeDIR(args[1]);
            Extract(args[1]);
            Console.WriteLine("\nExtracted: " + (countCXML + countGim + countVag + countDDS + countPNG + countWAV + countGTF).ToString() + " Files\nCXML Containers: " + countCXML.ToString() + "\nGIM Files: " + countGim.ToString() + "\nVAG Files: " + countVag.ToString() + "\nDDS Files: " + countDDS.ToString() + "\nPNG Files: " + countPNG.ToString() + "\nWAV Files: " + countWAV.ToString() + "\nGTF Files: " + countGTF.ToString() + "\n");
            Console.WriteLine("thx for using my Tool...\n..stay tuned for the incoming PlayStation CXML Tool ;)\nbyby <3");
            Environment.Exit(0);
        }
    }
}
