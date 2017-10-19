using System;
using UnityEngine;
namespace ParkitectAssetEditor
{
    /// <summary>
    /// Helper methods/properties
    /// </summary>
    public static class Utility
    {
        // TODO: support more than windows.
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
