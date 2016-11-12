using System;
using System.IO;
using Newtonsoft.Json;
using PearXLib;
using Ionic.Zip;
using System.Linq;

namespace pxpacker
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			string usage = "Usage: 'pxpacker pack <pxpacker root>' or 'pxpacker createroot'";
			if (args.Length < 1)
			{
				Console.WriteLine(usage);
				Environment.Exit(-1);
			}
			if (args[0] == "pack")
			{
				if (args.Length < 2)
				{
					Console.WriteLine(usage);
					Environment.Exit(-1);
				}
				string path = args[1];
				string pathJson = Path.Combine(path, "pxpacker.json");
				string pathOut = Path.Combine(path, "out");
				string pathFiles = Path.Combine(path, "files");
				string pathTemp = Path.Combine(path, "temp");
				if (Directory.Exists(path))
				{
					if (File.Exists(pathJson) && Directory.Exists(pathOut) && Directory.Exists(pathFiles))
					{
						PackerFile f = JsonConvert.DeserializeObject<PackerFile>(File.ReadAllText(pathJson));

						Directory.CreateDirectory(pathTemp);
						if (!args.Contains("--nozip"))
						{
							Console.WriteLine("Creating .ZIP archive...");
							string pathZip = Path.Combine(pathOut, f.NameForArchives + ".zip");
							File.Delete(pathZip);
							using (ZipFile zip = new ZipFile(pathZip))
							{
								foreach (string file in FileUtils.GetFilesInDir(pathFiles))
								{
									Console.WriteLine($"Adding {file} file...");
									string rel = TextUtils.GetRelativeString(file, pathFiles, true);
									string dir = Path.GetDirectoryName(rel);
									zip.AddFile(file, dir);
								}
								zip.Save();
							}
						}
					}
					else
					{
						Console.WriteLine("Root directory not setted up!");
						Environment.Exit(-1);
					}
				}
				else
				{
					Console.WriteLine("Directory not found.");
					Environment.Exit(-1);
				}
			}
			else if (args[1] == "createroot")
			{
				string dir = "pxpacker_example_root";
				Directory.CreateDirectory(dir + "/out");
				Directory.CreateDirectory(dir + "/files");
				File.WriteAllText(dir + "/pxpacker.json", JsonConvert.SerializeObject(new PackerFile
				{
					Name = "My Program",
					NameForArchives = "MyProgram",
					MainExe = "program.exe",
					Icon = "icon.png",
					EmbedMono = false
				}, Formatting.Indented));
				Console.WriteLine("OK!");
			}
		}
	}
}
