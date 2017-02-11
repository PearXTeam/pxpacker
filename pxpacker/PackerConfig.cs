using System;
namespace pxpacker
{
	public class PackerConfig
	{
		public string CodeName { get; set; }
		public string FriendlyName { get; set; }
		public string Version { get; set; }
		public string Description { get; set; }
		public string MaintainerEmail { get; set; }
		public string URL { get; set; }
		public string License { get; set; }
		public string[] PacmanDependencies { get; set; }
		public PackerShortcut[] Shortcuts { get; set; }
	}
}
