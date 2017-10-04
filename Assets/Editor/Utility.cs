using System;

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
                return string.Format(@"{0}\Parkitect\Mods", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
        } 
    }
}
