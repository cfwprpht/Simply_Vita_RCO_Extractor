using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLibNet;

namespace Simply_Vita_RCO_extractor
{
    class Program
    {
        #region vars
        private static byte[] gimMagic = new byte[16] { 0x4D, 0x49, 0x47, 0x2E, 0x30, 0x30, 0x2E, 0x31, 0x50, 0x53, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, };
        private static byte[] vagEnd = new byte[16] { 0x00, 0x07, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, };
        private static byte[] vrcoMagic = new byte[8] { 0x52, 0x43, 0x4F, 0x46, 0x10, 0x01, 0x00, 0x00, };
        private static byte[] vrcoXML = new byte[8] { 0x52, 0x43, 0x53, 0x46, 0x10, 0x01, 0x00, 0x00, };
        private static byte[] vagMagic = new byte[8] { 0x56, 0x41, 0x47, 0x70, 0x00, 0x02, 0x00, 0x01, };
        private static byte[] ddsMagic = new byte[4] { 0x44, 0x44, 0x53, 0x20, };
        private static byte[] zlibMagic = new byte[3] { 0x00, 0x78, 0xDA, };
        private static byte[] singlZL = new byte[2] { 0x78, 0xDA, };
        private static string GimConv = @"rsc\GimConv.exe";
        private static string Vag2Wav = @"rsc\vag2wav.exe";
        private static string Dds2Gtf = @"rsc\dds2gtf.exe";
        private static string baseDir = "";
        private static int countVag = 0;
        private static int countXml = 0;
        private static int countGim = 0;
        private static int countDds = 0;
        #endregion vars

        /// <summary>
        /// Automatical set the folder ico on Start of app
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
            Console.WriteLine("\nSimply Vita RCO extractor\nv1.00\nby cfwprophet\n\n");
        }

        // Show Help Screen
        private static void ShowUsage()
        {
            Console.WriteLine("Usage: svre.exe -f <input_file>");
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
                byte[] bufferMagic = new byte[8];
                string outFile = "notDefined";
                using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
                {
                    // Check Magic
                    Console.Write("Checking Header....");
                    br.Read(bufferMagic, 0, 8);

                    if (!CompareBytes(bufferMagic, vrcoMagic))
                    {
                        Console.WriteLine("ERROR: That is not a valid Vita RCO!\nExiting now...");
                        Environment.Exit(0);
                    }

                    Console.Write("Magic OK!\nThat's a Vita RCO :)\n");

                    // Get Data Table Offset and Length
                    Console.Write("Reading Offset and Length of Data Table...");
                    bufferMagic = new byte[4];
                    byte[] buffer = new byte[4];
                    br.BaseStream.Seek(0x48, SeekOrigin.Begin);
                    br.Read(bufferMagic, 0, 4);
                    br.Read(buffer, 0, 4);
                    Array.Reverse(bufferMagic);
                    Array.Reverse(buffer);
                    Console.Write("done!\n");
                    Console.WriteLine("Readed Hex value of Offset: 0x" + BitConverter.ToString(bufferMagic).Replace("-", ""));
                    Console.WriteLine("Readed Hex value of Size: 0x" + BitConverter.ToString(buffer).Replace("-", ""));

                    // Check for zlib Header '0x78DA' (compression level=9) and for VAG files and write to file
                    byte[] zlib = new byte[3];
                    byte[] vag = new byte[8];
                    byte[] temp = new byte[1];
                    int i, startVAG, startZLIB, dumped, count;
                    startVAG = 0;
                    startZLIB = 0;
                    dumped = 0;
                    int end = Convert.ToInt32(BitConverter.ToString(buffer).Replace("-", ""), 16);
                    count = Convert.ToInt32(BitConverter.ToString(bufferMagic).Replace("-", ""), 16);
                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                    Console.WriteLine("Offset to start from: " + count + " bytes");
                    Console.WriteLine("Size to Dump: " + end + " bytes");
                    Console.WriteLine("Searching for ZLib Compressed and VAG Files...");

                    // main loop
                    while ((i = br.Read(temp, 0, 1)) != 0)
                    {
                        #region zlibExtractSingle
                        if (count == Convert.ToInt32(BitConverter.ToString(bufferMagic).Replace("-", ""), 16))
                        {
                            zlib = new byte[2];
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(zlib, 0, 2);

                            if (CompareBytes(zlib, singlZL))
                            {
                                Console.Write("Found a ZLib Compressed File, starting to extract...");
                                byte[] toWrite = new byte[1];
                                zlib = new byte[3];
                                byte[] _vag = new byte[8];

                                outFile = baseDir + startZLIB++ + ".compressed";
                                File.Create(outFile).Close();
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(zlib, 0, 3);
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(_vag, 0, 8);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    while(true)
                                    {
                                        if (!CompareBytes(zlib, zlibMagic))
                                        {
                                            if (!CompareBytes(_vag, vagMagic))
                                            {
                                                // write out data to file
                                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                br.Read(toWrite, 0, 1);
                                                bw.Write(toWrite, 0, 1);

                                                // Count up 1 and read the next bytes before the loop start again
                                                count++;
                                                dumped++;

                                                // Have we reached the end of data table?
                                                if (dumped != end)
                                                {
                                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                    br.Read(zlib, 0, 3);
                                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                    br.Read(_vag, 0, 8);
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

                                    // In case of we need to compare zlibHeader with a additional 0x00 on top to know if it is really a zlibHeader and not just a 0x78DA data value
                                    // we add that 0x00 on end of the extracted file and count dumped var +1
                                    if (CompareBytes(zlib, zlibMagic))
                                    {
                                        toWrite = new byte[1];
                                        bw.Write(toWrite, 0, 1);
                                        dumped++;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");
                            }

                            // Decompress dumped File
                            ZLibDeCompress(outFile);

                            // Check Header of Decompressed File and rename
                            Console.Write("Checking Header of Decompressed File...");
                            outFile = outFile + ".decompressed";
                            string corExt = "";
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

                                if (CompareBytes(xmlHeader, vrcoXML))
                                {
                                    Console.Write("done!\nIt's a CXML Container.\n");
                                    corExt = outFile.Replace(".compressed.decompressed", ".cxml");
                                    countXml++;
                                }
                                else if (CompareBytes(gimHeader, gimMagic))
                                {
                                    Console.Write("done!\nIt's a GIM File.\n");
                                    corExt = outFile.Replace(".compressed.decompressed", ".gim");
                                    countGim++;
                                    gHeader = true;
                                }
                                else if (CompareBytes(ddsHeader, ddsMagic))
                                {
                                    Console.Write("done!\nIt's a DDS Container.\n");
                                    corExt = outFile.Replace(".compressed.decompressed", ".dds");
                                    countDds++;
                                    dHeader = true;
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
                                ConvertGIM(corExt);
                            else if (dHeader)
                                ConvertDDS(corExt);

                        }
                        #endregion ZlibExtractSingle
                        #region ZlibExtract
                        else
                        {
                            zlib = new byte[3];
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(zlib, 0, 3);
                            if (CompareBytes(zlib, zlibMagic))
                            {
                                Console.Write("Found a ZLib Compressed File, starting to extract...");
                                byte[] toWrite = new byte[1];
                                byte[] _vag = new byte[8];

                                outFile = baseDir + startZLIB++ + ".compressed";
                                File.Create(outFile).Close();
                                count++;
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(zlib, 0, 3);
                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                br.Read(_vag, 0, 8);

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                                {
                                    while(true)
                                    {
                                        if (!CompareBytes(zlib, zlibMagic))
                                        {
                                            if (!CompareBytes(_vag, vagMagic))
                                            {
                                                // Write out Data to file
                                                br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                br.Read(toWrite, 0, 1);
                                                bw.Write(toWrite, 0, 1);

                                                // Count up 3 and read the next bytes before the loop start again
                                                count++;
                                                dumped++;

                                                // Have we reached the end of data table?
                                                if (dumped != end)
                                                {
                                                    // if not set BaseStream pointer to next value and read next bytes...
                                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                    br.Read(zlib, 0, 3);
                                                    br.BaseStream.Seek(count, SeekOrigin.Begin);
                                                    br.Read(_vag, 0, 8);
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

                                    // In case of we need to compare zlibHeader with a additional 0x00 on top to know if it is really a zlibHeader and not just a 0x78DA data value
                                    // we add that 0x00 on end of the extracted file and count dumped var +1
                                    if (CompareBytes(zlib, zlibMagic))
                                    {
                                        toWrite = new byte[1];
                                        bw.Write(toWrite, 0, 1);
                                        dumped++;
                                    }
                                    bw.Close();
                                }
                                Console.Write("done!\n");

                                // Decompress dumped File
                                ZLibDeCompress(outFile);

                                // Check Header of Decompressed File and rename
                                Console.Write("Checking Header of Decompressed File...");
                                outFile = outFile + ".decompressed";
                                string corExt = "";
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

                                    if (CompareBytes(xmlHeader, vrcoXML))
                                    {
                                        Console.Write("done!\nIt's a CXML Container.\n");
                                        corExt = outFile.Replace(".compressed.decompressed", ".cxml");
                                        countXml++;
                                    }
                                    else if (CompareBytes(gimHeader, gimMagic))
                                    {
                                        Console.Write("done!\nIt's a GIM File.\n");
                                        corExt = outFile.Replace(".compressed.decompressed", ".gim");
                                        countGim++;
                                        gHeader = true;
                                    }
                                    else if (CompareBytes(ddsHeader, ddsMagic))
                                    {
                                        Console.Write("done!\nIt's a DDS Container.\n");
                                        corExt = outFile.Replace(".compressed.decompressed", ".dds");
                                        countDds++;
                                        dHeader = true;
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
                                    ConvertGIM(corExt);
                                else if (dHeader)
                                    ConvertDDS(corExt);

                            }
                        }
                        #endregion zlibExtract

                        #region vagExtract
                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                        br.Read(vag, 0, 8);
                        if (CompareBytes(vag, vagMagic))
                        {
                            Console.Write("Found a VAG File will start to extract...");
                            outFile = baseDir + startVAG++ + ".vag";
                            File.Create(outFile).Close();
                            byte[] toWrite = new byte[16];
                            br.BaseStream.Seek(count, SeekOrigin.Begin);
                            br.Read(toWrite, 0, 16);
                            using (BinaryWriter bw = new BinaryWriter(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                while(true)
                                {
                                    if (!CompareBytes(toWrite, vagEnd))
                                    {
                                        // Write out the readed data
                                        bw.Write(toWrite, 0, 16);

                                        // count up the readed bytes and read next one before the loop start again
                                        dumped += 16;
                                        count += 16;
                                        br.Read(toWrite, 0, 16);
                                    }
                                    else
                                        break;
                                }
                                bw.Write(toWrite, 0, 16);
                                dumped += 16;
                                count += 16;
                                bw.Close();
                            }
                            Console.Write("done!\n");

                            // Convert VAG to WAV
                            ConvertVAG(outFile);
                            countVag++;
                        }
                        #endregion vagExtract

                        // Have we dumped all data?
                        if (dumped == end)
                            break;
                        br.BaseStream.Seek(count, SeekOrigin.Begin);
                    }
                    br.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nERROR:\n" + e.ToString());
                Environment.Exit(0);
            }
        }

        // Main entry point
        static void Main(string[] args)
        {
            SetFolderIco();
            ShowVersion();
            if (CheckInput(args) == false)
            {
                ShowUsage();
                Environment.Exit(0);
            }
            MakeDIR(args[1]);
            Extract(args[1]);
            Console.WriteLine("\nExtracted: " + (countXml + countGim + countVag + countDds).ToString() + " Files\nCXML Containers: " + countXml.ToString() + "\nGIM Files: " + countGim.ToString() + "\nVAG Files: " + countVag.ToString()  + "\nDDS Files: " + countDds.ToString() + "\n\n");
            Console.WriteLine("thx for using my Tool...\n..stay tuned for the incoming PlayStation CXML Tool ;)\nby");
            Environment.Exit(0);
        }
    }
}
