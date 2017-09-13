using Newtonsoft.Json;
using UnityEngine;

namespace ParkitectAssetEditor
{
    /// <summary>
    /// Represents an asset that will be loaded in Parkitect.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public AssetType Type { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public float Price { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the sub category.
        /// </summary>
        /// <value>
        /// The sub category.
        /// </value>
        public string SubCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this asset snaps to grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if snaps to grid; otherwise, <c>false</c>.
        /// </value>
        public bool GridSnap { get; set; }

        /// <summary>
        /// Gets or sets the grid subdivision.
        /// </summary>
        /// <value>
        /// The grid subdivision.
        /// </value>
        public int GridSubdivision { get; set; }

        private MaterialType _material;

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>
        /// The shader.
        /// </value>
        public MaterialType Material
        {
            get
            {
                return _material;
            }
            set
            {
                if (_material != value)
                {
                    _material = value;
                    ChangeMaterial(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the game object instance identifier.
        /// </summary>
        /// <value>
        /// The game object instance identifier.
        /// </value>
        public int GameObjectInstanceId { get; set; }

        /// <summary>
        /// The game object
        /// </summary>
        private GameObject _gameObject;

        /// <summary>
        /// Gets or sets the game object.
        /// </summary>
        /// <value>
        /// The game object.
        /// </value>
        [JsonIgnore]
        public GameObject GameObject
        {
            get { return _gameObject; }
            set
            {
                GameObjectInstanceId = value.GetInstanceID();
                _gameObject = value;
            }
        }

        private void ChangeMaterial(MaterialType value)
        {
            var material = Resources.Load<Material>(value == MaterialType.Diffuse ? "Diffuse" : "Specular");

            GameObject.GetComponent<Renderer>().material = material;
        }
    }
}
