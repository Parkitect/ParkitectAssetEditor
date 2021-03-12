using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace ParkitectAssetEditor.Assets.Editor.UI
{
    public class AssetProcessor : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            if (ProjectManager.Project != null)
                ProjectManager.Save();

            return paths;
        }
    }
}
