using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor
{
    public static class AssetPackSerializer
    {
        /// <summary>
        /// Saves the specified asset pack.
        /// </summary>
        /// <param name="assetPack">The asset pack.</param>
        /// <returns></returns>
        public static bool CreateAssetBundle(this AssetPack assetPack)
        {
            if (assetPack.Assets.Any(a => a.GameObject == null))
            {
                foreach (var asset in assetPack.Assets.Where(a => a.GameObject == null))
                {
                    Debug.LogError($"Could not save asset pack because GameObject of asset {asset.Name} is missing.");

                    return false;
                }
            }

            Directory.CreateDirectory(Application.dataPath + "/Tmp");

            List<string> prefabPaths = new List<string>();

            foreach (Asset asset in assetPack.Assets)
            {
                var path = $"Assets/Tmp/{asset.GameObjectInstanceId}.prefab";

                PrefabUtility.CreatePrefab(path, asset.GameObject);

                prefabPaths.Add(path);
            }

            AssetBundleBuild[] descriptor = {
                new AssetBundleBuild()
                {
                    assetBundleName = "assetPack",
                    assetNames      = prefabPaths.ToArray()
                }
            };

            BuildPipeline.BuildAssetBundles(ProjectManager.Project.Value.ProjectDirectory, descriptor, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

            return true;
        }
        
        /// <summary>
        /// Fills asset pack from asset bundle.
        /// </summary>
        /// <param name="assetPack">The asset pack.</param>
        public static void LoadAssetBundle(this AssetPack assetPack)
        {
            var assetBundlePath = Path.Combine(ProjectManager.Project.Value.ProjectDirectory, "assetPack");

            var assetBundle = AssetBundle.LoadFromFile(assetBundlePath);

            for (var i = assetPack.Assets.Count - 1; i >= 0; i--)
            {
                var asset = assetPack.Assets[i];

                var objectInScene = EditorUtility.InstanceIDToObject(asset.GameObjectInstanceId);
                if (objectInScene != null)
                {
                    asset.GameObject = objectInScene as GameObject;
                }
                else
                {
                    // if one object fails to load, don't make it fail the rest
                    try
                    {
                        var go = assetBundle.LoadAsset<GameObject>(asset.GameObjectInstanceId.ToString());

                        var instantiadedGo = Object.Instantiate(go);

                        asset.GameObject = instantiadedGo;
                        asset.GameObject.name = asset.Name;
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError($"Could not find GameObject in AssetBundle for asset {asset.Name}, skipped loading of asset");

                        assetPack.Assets.Remove(asset);
                    }
                }
            }

            assetBundle.Unload(true);
        }
    }
}
