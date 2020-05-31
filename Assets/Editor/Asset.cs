using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ParkitectAssetEditor.GizmoRenderers;
using ParkitectAssetEditor.Utility;
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
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				if (GameObject != null) GameObject.name = _name;
			}
		}

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
		[JsonIgnore] public Color[] Colors = new Color[4];

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

		/// <summary>
		/// Gets or sets a value indicating whether this object blocks visibility.
		/// </summary>
		/// <value>
		///   <c>true</c> if [see through]; otherwise, <c>false</c>.
		/// </value>
		public bool CanSeeThrough { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this object blocks rain.
		/// </summary>
		/// <value>
		///   <c>true</c> if [blocks rain]; otherwise, <c>false</c>.
		/// </value>
		public bool BlocksRain { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this object has lights that should turn on at night.
		/// </summary>
		/// <value>
		///   <c>true</c> if [lights should turn on at night]; otherwise, <c>false</c>.
		/// </value>
		public bool LightsTurnOnAtNight { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether lights on this object should use custom colors.
		/// </summary>
		/// <value>
		///   <c>true</c> if [lights should use custom colors]; otherwise, <c>false</c>.
		/// </value>
		public bool LightsUseCustomColors { get; set; }

		/// <summary>
		/// Gets or sets the custom color slot to use by this light.
		/// </summary>
		/// <value>
		/// The custom color slot
		/// </value>
		public int LightsCustomColorSlot { get; set; }

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
		public GameObject FencePost
		{
			get { return GameObjectHashMap.Instance.Get(Guid + ".post"); }
			set { GameObjectHashMap.Instance.Set(Guid + ".post", value); }
		}

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

        public float Height;
        #endregion

        #region shop

        public List<ShopProduct> Products = new List<ShopProduct>();

	    #endregion

        #region sign
        /// <summary>
        /// Gets or sets the text game object.
        /// </summary>
        /// <value>
        /// The text game object.
        /// </value>
        [JsonIgnore]
        public GameObject Text
        {
            get { return GameObjectHashMap.Instance.Get(Guid + ".text"); }
            set { GameObjectHashMap.Instance.Set(Guid + ".text", value); }
        }
        #endregion

		#region flatride

		public string Description;

		/// <summary>
		/// Get or sets a value indicating how protected against rain a ride is bounded between 0 and 1
		/// </summary>
		public float RainProtection;

		/// <summary>
		/// Get or sets a value indicating how exciting a ride is bounded between 0 and 1
		/// </summary>
		public float Excitement;

		/// <summary>
		/// Get or sets a value indicating how intense a ride is bounded between 0 and 1
		/// </summary>
		public float Intensity;

		/// <summary>
		/// Get or sets a value indicating how nausea a ride is bounded between 0 and 1
		/// </summary>
		public float Nausea;

		/// <summary>
		/// Determines the x footprint size
		/// </summary>
		public int FootprintX = 1;

		/// <summary>
		/// Determines the y footprint size
		/// </summary>
		public int FootprintZ = 1;

		/// <summary>
		/// Category of flatride
		/// </summary>
		public string FlatRideCategory = AttractionType.CategoryTag[0];

		#endregion

		#region trains

		public string TrackedRideName;

		public int DefaultTrainLength = 1;
		public int MinTrainLength = 1;
		public int MaxTrainLength = 1;

		public CoasterCar LeadCar;
		public CoasterCar Car;
		public CoasterCar RearCar;

		#endregion

		#region BoundingBoxes

		[JsonIgnore]
		public bool EnableBoundingBoxEditing;


		/// <summary>
		/// determins if the bounding box can snap
		/// </summary>
		[JsonIgnore]
		public bool BoundingBoxSnap;

		/// <summary>
		/// the active bounded box
		/// </summary>
		[JsonIgnore]
		public BoundingBox SelectedBoundingBox = null;

		/// <summary>
		/// Bounded box regions
		/// </summary>
		public List<BoundingBox> BoundingBoxes = new List<BoundingBox>();

		#endregion

		#region waypoints

		/// <summary>
		/// list of waypoints to help with NPC traversial
		/// </summary>
		public List<Waypoint> Waypoints = new List<Waypoint>();

		[JsonIgnore] public bool EnableWaypointEditing;

		/// <summary>
		/// Helper Y Plane, used to place waypoints
		/// </summary>
		[JsonIgnore] public float HelperPlaneY;

		/// <summary>
		/// The currently selected waypoint
		/// </summary>
		[JsonIgnore] public Waypoint SelectedWaypoint;

		/// <summary>
		/// The state of the waypoint editor
		/// </summary>
		[JsonIgnore] public WaypointRenderer.WaypointState WaypointState;

		#endregion

        #region sign

		/// <summary>
		/// Gets or sets the screen game object.
		/// </summary>
		/// <value>
		/// The screen game object.
		/// </value>
		[JsonIgnore]
		public GameObject Screen
		{
			get { return GameObjectHashMap.Instance.Get(Guid + ".screen"); }
			set { GameObjectHashMap.Instance.Set(Guid + ".screen", value); }
		}

		#endregion

		public AspectRatio AspectRatio { get; set; }

		/// <summary>
		/// Gets or sets the game object.
		/// </summary>
		/// <value>
		/// The game object.
		/// </value>
		[JsonIgnore]
		public GameObject GameObject
		{
			get { return GameObjectHashMap.Instance.Get(Guid); }
			set { GameObjectHashMap.Instance.Set(Guid, value); }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Asset"/> class.
		/// </summary>
		public Asset()
		{
			Guid = GUID.Generate().ToString(); // don't need the object, just make it a string immediately
			MinSize = 1;
			MaxSize = 1;
            Height = 1;
			CanSeeThrough = true;
		}
	}
}
