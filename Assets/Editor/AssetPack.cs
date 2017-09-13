using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public List<Asset> Assets { get; set; } = new List<Asset>();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetPack"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AssetPack(string name)
        {
            Name = name;
        }

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
            if (Assets.Any(a => a.GameObjectInstanceId == gameObject.GetInstanceID()))
            {
                Debug.LogWarning($"GameObject {gameObject.name} is already added as an asset, can't add twice.");

                return Assets.First(a => a.GameObjectInstanceId == gameObject.GetInstanceID());
            }

            var asset = new Asset
            {
                Name = gameObject.name,
                GameObject = gameObject,
                Category = "Asset Pack",
                SubCategory = gameObject.name,
                GridSnap = true,
                GridSubdivision = 4
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
                Debug.LogWarning($"Asset {asset.Name} was already added as an asset, can't add twice.");
            }

            Assets.Add(asset);

            asset.GameObject.SetActive(true);

            LayOutAssets();
        }
        
        /// <summary>
        /// Removes the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Remove(Asset asset)
        {
            Assets.Remove(asset);

            asset.GameObject?.SetActive(false);

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
                asset.GameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
                asset.GameObject.SetActive(true);
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

                asset.GameObject.transform.position = new Vector3(x, 0, z);
            }
        }
    }
}
