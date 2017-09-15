using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor
{
    /// <summary>
    /// Represents an asset that will be loaded in Parkitect.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// The name
        /// </summary>
        private string _name;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name {
            get { return _name; }
            set
            {
                _name = value;
                if(GameObject != null) GameObject.name = _name;
            }
        }

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		[JsonConverter(typeof(StringEnumConverter))]
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

        #region deco
        /// <summary>
        /// Gets or sets a value indicating whether this asset builds on a grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if asset builds on a grid; otherwise, <c>false</c>.
        /// </value>
        public bool BuildOnGrid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this asset snaps to the center of a tile.
        /// </summary>
        /// <value>
        ///   <c>true</c> if snaps to center; otherwise, <c>false</c>.
        /// </value>
        public bool SnapCenter { get; set; }

        /// <summary>
        /// Gets or sets the grid size.
        /// </summary>
        /// <value>
        /// The grid size.
        /// </value>
        public float GridSize { get; set; }

        /// <summary>
        /// Gets or sets the height delta.
        /// </summary>
        /// <value>
        /// The height delta.
        /// </value>
        public float HeightDelta { get; set; }
        #endregion

        #region bench
        /// <summary>
        /// Gets or sets a value indicating whether the bench has a back rest.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bench has back a rest; otherwise, <c>false</c>.
        /// </value>
        public bool HasBackRest { get; set; } = true;
        #endregion

        #region fence
        /// <summary>
        /// Gets or sets the flat GO of a fence.
        /// </summary>
        /// <value>
        /// The flat GO.
        /// </value>
        public GameObject FenceFlat { get; set; }

        /// <summary>
        /// Gets or sets the fence post GO.
        /// </summary>
        /// <value>
        /// The fence post GO.
        /// </value>
        public GameObject FencePost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has a post.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a post; otherwise, <c>false</c>.
        /// </value>
        public bool HasPost { get; set; }
        #endregion

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
            get
            {
                if (_gameObject == null && GameObjectInstanceId != 0)
                {
                    _gameObject = EditorUtility.InstanceIDToObject(GameObjectInstanceId) as GameObject;
                }

                return _gameObject;
            }
            set
            {
                GameObjectInstanceId = value.GetInstanceID();
                _gameObject = value;
            }
        }
    }
}
