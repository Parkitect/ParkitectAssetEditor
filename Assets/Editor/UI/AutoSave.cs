using UnityEditor;

namespace ParkitectAssetEditor.Assets.Editor.UI
{
    public class AssetProcessor : SaveAssetsProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            ProjectManager.Save();

            return paths;
        }
    }
}
