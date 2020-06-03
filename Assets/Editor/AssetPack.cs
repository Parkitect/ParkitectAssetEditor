using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParkitectAssetEditor
{
    /// <summary>
    /// The main asset pack class that combines all data about the assets.
    /// </summary>
    public class AssetPack
    {
        /// <summary>
        /// Gets or sets the assets.
        /// </summary>
        /// <value>
        /// The assets.
        /// </value>
        public List<Asset> Assets { get; set; }

        /// <summary>
        /// Paths to the assemblies that this mod will load
        /// </summary>
		public List<string> Assemblies = new List<string>();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <value>
        /// Defines the order in which this mod will get loaded
        /// </value>
        public int OrderPriority { get; set; }

        /// <summary>
        /// Gets or sets the asset archive setting.
        /// </summary>
        /// <value>
        /// The archive setting. If true, assets should be archived with the mod to be uploaded to the workshop.
        /// </value>
        public bool ArchiveAssets { get; set; }
        
        /// <summary>
        /// Adds the specified game object as an asset.
        /// </summary>
        /// <remarks>
        /// If you add a game object that already is an asset it will return that instance instead.
        /// </remarks>
        /// <param name="gameObject">The game object.</param>
        /// <returns>The asset that was created from the game object.</returns>
        public Asset Add(GameObject gameObject)
        {
            if (Assets.Any(a => a.GameObject == gameObject))
            {
                Debug.LogWarning(string.Format("GameObject {0} is already added as an asset, can't add twice.", gameObject.name));

                return Assets.First(a => a.GameObject == gameObject);
            }

            var asset = new Asset
            {
                GameObject = gameObject,
                Name = gameObject.name,
                Category = ProjectManager.Project.Value.ProjectName,
                SubCategory = "",
                SnapCenter = true,
                GridSubdivision = 1,
				HeightDelta = 0.25f
            };

            Add(asset);

            return asset;
        }

        /// <summary>
        /// Adds the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Add(Asset asset)
        {
            if (Assets.Contains(asset))
            {
                Debug.LogWarning(string.Format("Asset {0} was already added as an asset, can't add twice.", asset.Name));
            }

            Assets.Add(asset);

            asset.GameObject.SetActive(true);

            GameObjectHashMap.Instance.Set(asset.Guid, asset.GameObject);

            LayOutAssets();
        }
        
        /// <summary>
        /// Removes the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Remove(Asset asset)
        {
            Assets.Remove(asset);

            if (asset.GameObject != null)
            {
                asset.GameObject.SetActive(false);
            }

            GameObjectHashMap.Instance.Remove(asset.Guid);

            LayOutAssets();
        }

        /// <summary>
        /// Removes the assets from scene.
        /// </summary>
        public void RemoveAssetsFromScene()
        {
            foreach (var asset in Assets)
            {
                Object.DestroyImmediate(asset.GameObject);
            }
        }

        /// <summary>
        /// Setups the assets in the scene.
        /// </summary>
        public void InitAssetsInScene()
        {
            foreach (var asset in Assets)
            {
                if (asset.GameObject != null) asset.GameObject.SetActive(true);
            }

            LayOutAssets();
        }

        /// <summary>
        /// Lay out the assets in a grid in the scene.
        /// </summary>
        public void LayOutAssets()
        {
            var gridSize = Mathf.FloorToInt(Mathf.Log(Assets.Count)) + 1;
            const int margin = 5;

            for (var i = 0; i < Assets.Count; i++)
            {
                var asset = Assets[i];

                var z = i % gridSize * margin;
                var x = i / gridSize * margin;

                if (asset.GameObject != null)
                {
                    asset.GameObject.transform.position = new Vector3(x, 0, z);
                    asset.GameObject.transform.rotation = Quaternion.identity;
                }
            }
        }

        /// <summary>
        /// Finds an asset for game object.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <returns>The asset if found, null if not</returns>
        public Asset FindForGameObject(GameObject gameObject)
        {
            return Assets.FirstOrDefault(a => a.GameObject == gameObject);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetPack"/> class.
        /// </summary>
        public AssetPack()
        {
            Assets = new List<Asset>();
            ArchiveAssets = true;
        }
    }
}
