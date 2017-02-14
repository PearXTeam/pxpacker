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
						Console.WriteLine($"Creating pacman package for {arch}.");
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
				if (!args.Contains("--nowindows"))
				{
					foreach (Architecture arch in Enum.GetValues(typeof(Architecture)))
					{
						Console.WriteLine($"Creating Windows installer for {arch}...");
						string pathNsis = Path.Combine(pathTemp, "windows_" + arch.ToString());
						Directory.CreateDirectory(pathNsis);
						string nsis = Encoding.UTF8.GetString(ResourceUtils.GetFromResources("pxpacker.script.nsi"));
						long sz = 0;
						string fIcon = "";
						foreach (var f in PackerUtils.GetFiles(pathFiles, OS.Windows, arch))
						{
							if (f.File == cfg.IconFile)
								fIcon = f.Full;
							sz += new FileInfo(f.Full).Length / 1024;
						}
						StringBuilder fls = new StringBuilder();
						foreach (var f in PackerUtils.GetFiles(pathFiles, OS.Windows, arch))
						{
							fls.AppendLine(@"SetOutPath ""$INSTDIR\" + Path.GetDirectoryName(f.File).Replace('/', '\\') + @"""");
							fls.AppendLine(@"File """ + f.Full + @"""");
						}
						StringBuilder scs = new StringBuilder();
						foreach (var sc in cfg.Shortcuts)
						{
							scs.AppendLine(@"CreateShortCut ""$DESKTOP\" + sc.DisplayName + @".lnk"" ""$INSTDIR\" + sc.Executable.Replace('/', '\\') + @""" """ + sc.Arguments + @""" ""$INSTDIR\"  + sc.Executable.Replace('/', '\\') + @""" 0");
						}

						File.WriteAllText(Path.Combine(pathNsis, "script.nsis"), nsis
										  .Replace("$pxpName", cfg.FriendlyName)
										  .Replace("$pxpSize", sz.ToString())
										  .Replace("$pxpIcon", cfg.IconFile)
										  .Replace("$pxpCompany", cfg.Company)
										  .Replace("$pxpUrl", cfg.URL)
										  .Replace("$pxpVer", cfg.Version)
										  .Replace("$pxpOut", Path.Combine(pathOut, cfg.CodeName + "_" + cfg.Version + "_" + arch.ToString() + ".exe"))
										  .Replace("$pxpFiles", fls.ToString())
										  .Replace("$pxpShortcuts", scs.ToString())
										  .Replace("$pxpFullIcon", fIcon)
										 );
						ProcessStartInfo inf = new ProcessStartInfo("makensis", "script.nsis");
						inf.WorkingDirectory = pathNsis;
						Process proc = Process.Start(inf);
						proc.WaitForExit();
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
