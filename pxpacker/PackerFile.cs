using System;
namespace pxpacker
{
	public class PackerFile
	{
		public string Name { get; set; }
		public string NameForArchives { get; set; }
		public string MainExe { get; set; }
		public string Icon { get; set; }
		public bool EmbedMono { get; set; }
	}
}
