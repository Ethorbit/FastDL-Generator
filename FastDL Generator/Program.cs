﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.BZip2;
using System.Diagnostics;

namespace FastDL_Generator
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Program only accepts 1 directory, 
            // run it for every additional directory passed
            if (args.Length > 1)
            {
                string currentProgramPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                for (int i = 0; i < args.Length; i++)
                {
                    var newProc = new Process();
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = currentProgramPath;
                    startInfo.Arguments = $"\"{args[i]}\"";
                    newProc.StartInfo = startInfo;
                    newProc.Start();
                }
                
                return;
			}

            int RunningTasks = 0;
            int MaxTasks = 2;
            int HardLimit = 4;

            // Define Filetypes
            string[] SearchFor = new string[] {
                "materials/*.vmt",
                "materials/*.vtf",
                "materials/*.png",
                "materials/*.gif",
                "materials/*.jpg",
                "sound/*.wav",
                "sound/*.mp3",
                "sound/*.ogg",
                "maps/*.bsp",
                "maps/graphs/*.ain",
                "models/*.mdl",
                "models/*.vtx",
                "models/*.dx80.vtx",
                "models/*.dx90.vtx",
                "models/*.dx90.vtf",
                "models/*.xbox.vtx",
                "models/*.sw.vtx",
                "models/*.vvd",
                "models/*.phy",
                "resource/*.ttf",
                "particles/*.pcf"
            };

            // Setting mainpath if arg[0] (first argument given to programm) is set
            string MainPath;
            try
            {
                MainPath = args[0];
            }
            catch (Exception)
            {
                Console.WriteLine("Please add a Path as first arg");
                return;
            }

            // Same as above but instead quitting just set default path as the target path
            string copyPath = MainPath + $"/../fastdl-{Path.GetFileName(MainPath).ToLower()}";

            // if it exists Clear it because we need a clear folder
            if (Directory.Exists(copyPath))
            {
                Directory.Delete(copyPath, true);
            }

            if (!Directory.Exists(MainPath))
            {
                Console.WriteLine("The given path doesnt exists: {0}", MainPath);
                return;
            }


            List<string> IndexedFiles = new List<string>();

            // Indexing files 
            foreach (var Type in SearchFor)
            {
                string[] Data = TreeScan(MainPath, Type);
                foreach (var item in Data)
                {
                    IndexedFiles.Add(item.Substring(MainPath.Length + 1).Replace('\\', '/'));
                }
            }

            if(IndexedFiles.Count > 2000) 
            {
                MaxTasks = IndexedFiles.Count / 1000;
                if(MaxTasks > HardLimit)
                {
                    MaxTasks = 4;
                }
            }

            // Define first 2 lines for fastdl.lua
            string FileData = "// fastdl.lua generated by FastDL Generator.\n" +
                                "if (SERVER) then\n";

            // While copy files to target folder add line to File
            foreach (var item in IndexedFiles)
            {
                Console.WriteLine("Copy > " + item);
                FileData = FileData + " resource.AddFile(\"" + item + "\")\n";
                CopyFile(item, MainPath, copyPath);
            }

            // and the end
            FileData = FileData + "end";


            // Bzip2 any file in the Index (in the target folder)
            Console.Title = "Bzipping...";

            // Because the threads should NOT try to edit the same files at the same time ever
            List<string> ChangedFiles = new List<string> { };
            List<Thread> CurrentThreads = new List<Thread> { };

            for (int i = 0; i < MaxTasks; i++)
            {
                Thread temp = new Thread(new ThreadStart(ThreadedCompressing));
                temp.Start();
                Console.WriteLine("Thread #{0} Started", i);
                CurrentThreads.Add(temp);
                RunningTasks++;
            }

            void ThreadedCompressing()
            {
                long Edited = 0;
                foreach (string item in IndexedFiles)
                {
                    if (!ChangedFiles.Contains(item))
                    {
                        ChangedFiles.Add(item);
                        Edited++;
                    } else
                    {
                        // if (ChangedFiles.Contains(item)) Console.WriteLine("        >>>Item: <" + item + "> Already changed..");
                        continue;
                    }
                    try
                    {
                        string Path = copyPath + "/" + item;


                        if (File.Exists(Path) )
                        {
                            Console.WriteLine("compressing > " + item);
                            BzipFile(Path);

                            Console.Title = " Compressed "+ChangedFiles.Count+" / "+IndexedFiles.Count+" Files, Running Threads: "+CurrentThreads.Count;
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                RunningTasks--;
                Console.WriteLine("Thread Killed with {0} compressed files. {1} Threads remaining please be patient...", Edited,RunningTasks);
                if(RunningTasks <= 0)
                {
                    Console.WriteLine("All Threads Killed, Generator closed");

                    // Save the fastdl.lua in the target folder
                    File.WriteAllText(copyPath + $"/fastdl-{Path.GetFileName(MainPath).ToLower()}.lua", FileData);
                }
            }

        }
       
        private static void CopyFile(string Filee,string oldFolder, string NewFolder)
        {
            string oldFile = oldFolder+"/"+ Filee;
            string newFile = NewFolder+"/"+ Filee;

            try
            {
                Directory.CreateDirectory(newFile);
            } 
            catch (Exception) { }
            
            try
            {
                Directory.Delete(newFile); // hacky way
            } 
            catch (Exception) { }
            
            try
            {
                File.Copy(oldFile, newFile, true);
            }
            catch (Exception)
            {
                Console.WriteLine("Error at Copy:\n" + oldFile + " >>> " + newFile);
            }
        }
        
        private static string[] TreeScan(string mainDir, string search)
        {
            try
            {
                return Directory.GetFiles(mainDir, search, SearchOption.AllDirectories);
            }
            catch (Exception)
            {
                return new string[] { };
            }
        }

        private static bool BzipFile(string Path)
        {
            if(!File.Exists(Path))
            {
                return false;
            }
            FileInfo fileToBeZipped = new FileInfo(Path);
            FileInfo zipFileName = new FileInfo(string.Concat(fileToBeZipped.FullName, ".bz2"));
            using (FileStream fileToBeZippedAsStream = fileToBeZipped.OpenRead())
            {
                using (FileStream zipTargetAsStream = zipFileName.Create())
                {
                    try
                    {
                        BZip2.Compress(fileToBeZippedAsStream, zipTargetAsStream, true, 4096);
                        System.IO.File.Delete(Path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }
            return true;
        }
    }
} 