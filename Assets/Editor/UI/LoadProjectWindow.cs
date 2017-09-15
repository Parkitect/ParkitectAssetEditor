using UnityEditor;

namespace ParkitectAssetEditor.UI
{
    /// <summary>
    /// Window that handles the loading of projects.
    /// </summary>
    class LoadProjectWindow
    {
        /// <summary>
        /// Shows this load project window.
        /// </summary>
        public static void Show()
        {
            string path = EditorUtility.OpenFilePanel("Open Project", Utility.ParkitectModPath, "assetProject");

            if (path.Length != 0)
            {
                ProjectManager.Load(path);
            }
        }
    }
}
