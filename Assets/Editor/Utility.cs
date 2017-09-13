using System;

namespace ParkitectAssetEditor
{
    /// <summary>
    /// Helper methods/properties
    /// </summary>
    public static class Utility
    {
        // TODO: support more than windows.
        public static string ParkitectModPath => $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\Parkitect\Mods";
    }
}
