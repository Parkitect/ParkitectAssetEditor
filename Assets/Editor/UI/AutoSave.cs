using UnityEditor;

namespace ParkitectAssetEditor.Assets.Editor.UI
{
    public class AssetProcessor : UnityEditor.AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (ProjectManager.Project != null)
                ProjectManager.Save();

            return paths;
        }
    }
}
