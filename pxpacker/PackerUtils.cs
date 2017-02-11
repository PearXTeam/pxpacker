using System.Collections.Generic;
using System.IO;
using PearXLib;

namespace pxpacker
{
	public static class PackerUtils
	{
		public static PackerFile[] GetFiles(string path, OS os, Architecture arch)
		{
			List<PackerFile> files = new List<PackerFile>();
			string[] dirsToCheck = {
				Path.Combine(path, os.ToString().ToLower(), arch.ToString().ToLower()),
				Path.Combine(path, "all", arch.ToString().ToLower()),
				Path.Combine(path, "all", "all"),
				Path.Combine(path, os.ToString().ToLower(), "all")};
			foreach (var p in dirsToCheck)
			{
				if (Directory.Exists(p))
				{
					var fls = FileUtils.GetFilesInDir(p);
					foreach (var f in fls)
					{
						files.Add(new PackerFile
						{
							Full = f,
							File = f.Substring(p.Length + 1)
						});
					}
				}
			}
			return files.ToArray();
		}

		public static string ParseFile(string path, string file)
		{
			return file.Substring(path.Length + 1);
		}
	}
}
