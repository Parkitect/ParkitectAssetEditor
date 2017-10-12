using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace ParkitectAssetEditor.Assets.Editor.UI
{
    public class FileModificationWarning : SaveAssetsProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            ProjectManager.Save();

            return paths;
        }
    }
}
