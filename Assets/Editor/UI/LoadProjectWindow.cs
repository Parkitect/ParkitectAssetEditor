using System;
using UnityEditor;

namespace ParkitectAssetEditor.UI
{
    class LoadProjectWindow
    {
        public static void Show()
        {
            string path = EditorUtility.OpenFilePanel("Open Project", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect/Mods", "assetProject");
            if (path.Length != 0)
            {
                ProjectManager.Load(path);
            }
        }
    }
}
