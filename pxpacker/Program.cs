using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Newtonsoft.Json;
using PearXLib;

namespace pxpacker
{
	public class Program
	{
		public static string Usage => "Usage: pxpacker <root> [--nopacman] [--nowindows] [--nozip]";

		public static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				string path = Path.GetFullPath(args[0]);
				string pathFiles = Path.Combine(path, "files");
				string pathOut = Path.Combine(path, "output");
				string pathTemp = Path.Combine(path, "temp");
				var cfg = JsonConvert.DeserializeObject<PackerConfig>(File.ReadAllText(Path.Combine(path, "config.json")));

				if (!args.Contains("--nozip"))
				{
					foreach (OS os in Enum.GetValues(typeof(OS)))
					{
						foreach (Architecture arch in Enum.GetValues(typeof(Architecture)))
						{
							Console.WriteLine($"Packing .ZIP archive for {os} {arch}.");
							var files = PackerUtils.GetFiles(pathFiles, os, arch);
							using (ZipFile zip = new ZipFile(Path.Combine(pathOut, cfg.CodeName + "_" + cfg.Version + "_" + os + "_" + arch + ".zip")))
							{
								foreach (var v in files)
									zip.AddFile(v.Full, Path.GetDirectoryName(v.File));
								zip.Save();
							}
						}
					}
				}
				if (!args.Contains("--nopacman"))
				{
					foreach (Architecture arch in Enum.GetValues(typeof(Architecture)))
					{
						string pathPacman = Path.Combine(pathTemp, "pacman_" + arch.ToString());
						Directory.CreateDirectory(pathPacman);
						StringBuilder pb = new StringBuilder();
						pb.AppendLine("# Maintainer:  " + cfg.MaintainerEmail);
						pb.AppendLine($"pkgname='{cfg.CodeName}'");
						pb.AppendLine($"pkgver='{cfg.Version}'");
						pb.AppendLine($"pkgrel=1");
						pb.AppendLine($"pkgdesc='{cfg.Description}'");
						pb.AppendLine($"url='{cfg.URL}'");
						pb.AppendLine($"arch=('any')");
						pb.AppendLine($"license=( '{cfg.License}' )");
						pb.AppendLine("depends=(");
						foreach (string s in cfg.PacmanDependencies)
							pb.Append($" '{s}' ");
						pb.AppendLine(")");
						pb.AppendLine("package() {  ");
						foreach (var f in PackerUtils.GetFiles(pathFiles, OS.Linux, arch))
						{
							pb.AppendLine("mkdir -p ${pkgdir}'/usr/share/" + cfg.CodeName + "/" + Path.GetDirectoryName(f.File) + "'");
							pb.AppendLine("cp '" + f.Full + "' ${pkgdir}'/usr/share/" + cfg.CodeName + "/" + f.File + "'");
						}
						pb.AppendLine("mkdir -p ${pkgdir}'/usr/share/applications'");
						foreach (var f in cfg.Shortcuts)
						{
							var sc = Path.Combine(pathPacman, f.DisplayName + ".desktop");
							pb.AppendLine("cp '" + sc + "' ${pkgdir}'/usr/share/applications/" + f.DisplayName + ".desktop" + "'");
							PXL.CreateShortcut($"/usr/share/{cfg.CodeName}/" + f.Executable, f.Arguments, pathPacman, f.DisplayName, f.GenericName, $"/usr/share/{cfg.CodeName}/" + f.LinuxIcon);
						}
						pb.Append("}");
						File.WriteAllText(Path.Combine(pathPacman, "PKGBUILD"), pb.ToString());
						ProcessStartInfo inf = new ProcessStartInfo("makepkg");
						inf.WorkingDirectory = pathPacman;
						Process proc = Process.Start(inf);
						proc.WaitForExit();
						string name = cfg.CodeName + "-" + cfg.Version + "-1-any.pkg.tar.xz";
						File.Move(Path.Combine(pathPacman, name), Path.Combine(pathOut, cfg.CodeName + "_" + cfg.Version + "_" + arch + ".pkg.tar.xz"));
					}
				}
				Directory.Delete(pathTemp, true);
				Console.WriteLine("Done!");
				return;
			}
			Console.WriteLine(Usage);
		}
	}
}
