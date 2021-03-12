using UnityEditor;

namespace ParkitectAssetEditor.Assets.Editor.UI
{
    public class AssetProcessor : SaveAssetsProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            ProjectManager.Save();

            return paths;
        }
    }
}
