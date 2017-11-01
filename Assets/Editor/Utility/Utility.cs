using System;

namespace ParkitectAssetEditor.Utility {
    /// <summary>
    /// Helper methods/properties
    /// </summary>
    public static class ModPath
    {
		/// <summary>
		/// Gets the path to the parkitect mod folder.
		/// </summary>
        public static string ParkitectModPath
        {
            get
            {
#if UNITY_STANDALONE_OSX
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/../Library/Application Support/Parkitect/", "Mods"));
#elif UNITY_STANDALONE_WIN
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect/", "Mods"));
#else
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath + "/..", "Mods"));
#endif
            }
        } 
    }
}
