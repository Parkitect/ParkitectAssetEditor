﻿using System.Linq;
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
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string Guid { get; set; }

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
        public float GridSubdivision { get; set; }

        /// <summary>
        /// Gets or sets the height delta.
        /// </summary>
        /// <value>
        /// The height delta.
        /// </value>
        public float HeightDelta { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance has custom colors.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance has custom colors; otherwise, <c>false</c>.
		/// </value>
		public bool HasCustomColors { get; set; }

		/// <summary>
		/// The colors
		/// </summary>
		[JsonIgnore]
		public Color[] Colors = new Color[4];

		/// <summary>
		/// Property to support serializing for Unity's color struct
		/// </summary>
		public CustomColor[] CustomColors
	    {
		    get { return Colors.Select(c => new CustomColor {Red = c.r, Green = c.g, Blue = c.b, Alpha = c.a}).ToArray(); }
		    set { Colors = value.Select(c => new Color(c.Red, c.Green, c.Blue, c.Alpha)).ToArray(); }
	    }

		/// <summary>
		/// Gets or sets the amount of custom colors this asset has.
		/// </summary>
		/// <value>
		/// The color count.
		/// </value>
		public int ColorCount { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is resizable.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is resizable; otherwise, <c>false</c>.
		/// </value>
		public bool IsResizable { get; set; }

		/// <summary>
		/// Gets or sets the minimum size.
		/// </summary>
		/// <value>
		/// The minimum size.
		/// </value>
		public float MinSize { get; set; }

		/// <summary>
		/// Gets or sets the maximum size.
		/// </summary>
		/// <value>
		/// The maximum size.
		/// </value>
		public float MaxSize { get; set; }
		#endregion

		#region bench
		/// <summary>
		/// Gets or sets a value indicating whether the bench has a back rest.
		/// </summary>
		/// <value>
		///   <c>true</c> if the bench has back a rest; otherwise, <c>false</c>.
		/// </value>
		public bool HasBackRest { get; set; }
        #endregion

        #region fence

		/// <summary>
		/// Gets or sets the fence post GO.
		/// </summary>
		/// <value>
		/// The fence post GO.
		/// </value>
		[JsonIgnore]
		public GameObject FencePost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has a post.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a post; otherwise, <c>false</c>.
        /// </value>
        public bool HasMidPost { get; set; }
        #endregion

        #region wall

        public int WallSettings;
        #endregion

        /// <summary>
        /// Gets or sets the game object instance identifier.
        /// </summary>
        /// <value>
        /// The game object instance identifier.
        /// </value>
        public int GameObjectInstanceId { get; set; }

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
                return EditorUtility.InstanceIDToObject(GameObjectInstanceId) as GameObject;
            }
            set
            {
                GameObjectInstanceId = value != null ? value.GetInstanceID() : 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        public Asset()
        {
            Guid = GUID.Generate().ToString(); // don't need the object, just make it a string immediately
        }
    }
}
