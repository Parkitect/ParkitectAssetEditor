using System;
using System.Globalization;
using System.IO;
using System.Linq;
using ParkitectAssetEditor.GizmoRenderers;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI
{
    /// <inheritdoc />
    /// <summary>
    /// The main asset editor window.
    /// </summary>
    /// <seealso cref="T:UnityEditor.EditorWindow" />
    public class AssetEditorWindow : EditorWindow
    {
        /// <summary>
        /// The window scroll position.
        /// </summary>
        private Vector2 _windowScrollPosition;

        /// <summary>
        /// The asset list scroll position.
        /// </summary>
        private Vector2 _assetListScrollPos;

        private Vector2 _descriptionTextScrollPosition;

        /// <summary>
        /// The selected asset.
        /// </summary>
        private Asset _selectedAsset;
        
        
        public Vector2 _waypointScrollView;

        private static readonly IGizmoRenderer[] _gizmoRenderers = {
            new SeatRenderer(),
            new WallRenderer(),
			new GridRenderer(), 
			new SignRenderer(), 
            new FootprintRenderer(),
            new WaypointRenderer(), 
        };

        [MenuItem("Window/Parkitect Asset Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(AssetEditorWindow));
        }

        void Awake()
        {
            // Make sure it doesn't autoload the project on unity start. This editor pref is just for when unity loses its state when it compiles.
            EditorPrefs.SetString("loadedProject", null);

            var files = Directory.GetFiles(Application.dataPath, "*.assetProject");
            
            if (files.Length > 0)
            {
                ProjectManager.Load(files[0]);
            }
        }
        
        public void Update()
        {
            // Unity loses its state when it compiles, with the editor pref we can load the opened project automatically.
            if (!ProjectManager.Initialized && !string.IsNullOrEmpty(EditorPrefs.GetString("loadedProject")))
            {
                ProjectManager.AutoLoad();

                _selectedAsset = ProjectManager.AssetPack.Assets.First();
            }

            if (ProjectManager.Initialized)
            {
                ProjectManager.AutoSave();

                // sync the selected game object in the scene with the corresponding asset
                if (_selectedAsset != null && Selection.activeGameObject != _selectedAsset.GameObject)
                {
                    var asset = ProjectManager.AssetPack.Assets.FirstOrDefault(a => a.GameObject == Selection.activeGameObject);

                    if (asset != null)
                    {
                        _selectedAsset = asset;

                        Repaint();
                    }
                }
            }
        }

        public void OnGUI()
        {
            if (!ProjectManager.Initialized)
            {
                EditorGUILayout.HelpBox("Create a project first", MessageType.Info);

                if (GUILayout.Button("New project"))
                {
                    NewProjectWindow.Show();
                }

                return;
            }

            // This draws the whole asset editor window, split up in sections.
            GUILayout.BeginVertical();
            _windowScrollPosition = GUILayout.BeginScrollView(_windowScrollPosition);
            DrawAssetPackSettingsSection();
            GUILayout.Space(15);
            DrawAssetListSection();
            GUILayout.Space(15);
            DrawAssetDetailSection();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

		[DrawGizmo(GizmoType.Selected)]
		static void DrawGizmoForMyScript(Transform scr, GizmoType gizmoType)
		{
			if (ProjectManager.AssetPack == null)
			{
				return;
			}

            foreach (var asset in ProjectManager.AssetPack.Assets.Where(a => a.GameObject != null && scr.GetComponentsInParent<Transform>().Contains(a.GameObject.transform)))
            {
                foreach (var gizmoRenderer in _gizmoRenderers)
                {
                    if (gizmoRenderer.CanRender(asset))
                    {
                        gizmoRenderer.Render(asset);
                    }
                }
            }
        }
        
        void OnFocus()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        void OnDestroy()
        {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            //Exporter.SaveToXML(ModManager);
            SceneView.onSceneGUIDelegate -= OnSceneGUI;

        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (ProjectManager.AssetPack == null)
            {
                return;
            }

            if (_selectedAsset == null)
            {
                return;
            }

            foreach (var gizmoRenderer in _gizmoRenderers)
            {
                if (gizmoRenderer.CanRender(_selectedAsset))
                {
                    var renderer = gizmoRenderer as IHandleRenderer;
                    if (renderer != null)
                    {
                        renderer.Handle(_selectedAsset);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the asset pack settings section.
        /// </summary>
        private void DrawAssetPackSettingsSection()
        {
            GUILayout.Label("Asset Pack Settings", "PreToolbar");

            ProjectManager.AssetPack.Name = EditorGUILayout.TextField("Name", ProjectManager.AssetPack.Name);
            GUILayout.Label("Pack description", EditorStyles.boldLabel);
            _descriptionTextScrollPosition = EditorGUILayout.BeginScrollView(_descriptionTextScrollPosition, GUILayout.Height(150));
			ProjectManager.AssetPack.Description = EditorGUILayout.TextArea(ProjectManager.AssetPack.Description, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Stores your raw model files with the asset pack in an archive. Recommended: on", MessageType.Info);
			ProjectManager.AssetPack.ArchiveAssets = EditorGUILayout.Toggle("Archive assets", ProjectManager.AssetPack.ArchiveAssets);

            GUILayout.Space(10);

            if (GUILayout.Button("Export Asset Pack"))
            {
                ProjectManager.Export(ProjectManager.AssetPack.ArchiveAssets);
            }
        }

        /// <summary>
        /// Draws the asset list section.
        /// </summary>
        private void DrawAssetListSection()
        {
            GUILayout.Label("Assets", "PreToolbar");

            EditorGUILayout.HelpBox("Drag your GameObject to this field to start editing.", MessageType.Info);

            // Game object drop box
            var newAsset = EditorGUILayout.ObjectField("Drop to add:", null, typeof(GameObject), true) as GameObject;
            if (newAsset != null && newAsset.scene.name != null) // scene name is null for prefabs, yay for unity for checking it this way
            {
                SelectAsset(ProjectManager.AssetPack.Add(newAsset));
            }

            // Asset list
            _assetListScrollPos = EditorGUILayout.BeginScrollView(_assetListScrollPos, "GroupBox", GUILayout.Height(200));
            foreach (var asset in ProjectManager.AssetPack.Assets)
            {
                // blue button for selected asset, black for non selected
                var style = new GUIStyle(GUI.skin.button)
                {
                    normal =
                    {
                        textColor = _selectedAsset != null && _selectedAsset.Guid == asset.Guid ? Color.blue : Color.black
                    }
                };

                if (GUILayout.Button(asset.Name, style))
                {
                    SelectAsset(asset);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the asset detail section of the selected asset.
        /// </summary>
        private void DrawAssetDetailSection()
        {
            if (_selectedAsset == null)
            {
                return;
            }

            GUILayout.Label(string.Format("{0} Settings", _selectedAsset.Name), "PreToolbar");

            // Name, type and price settings
            GUILayout.Label("General:", EditorStyles.boldLabel);
            _selectedAsset.Name = EditorGUILayout.TextField("Name", _selectedAsset.Name);
            _selectedAsset.Type = (AssetType)EditorGUILayout.Popup("Type", (int)_selectedAsset.Type, new[]
            {
                AssetType.Deco.ToString(),
                AssetType.Wall.ToString(),
                AssetType.Trashbin.ToString(),
                AssetType.Bench.ToString(),
                AssetType.Fence.ToString(),
                AssetType.Lamp.ToString(),
                AssetType.Sign.ToString(),
                AssetType.Tv.ToString(),
                AssetType.FlatRide.ToString()
            });
            _selectedAsset.Price = EditorGUILayout.FloatField("Price:", _selectedAsset.Price);

            GUILayout.Label("Color settings", EditorStyles.boldLabel);
            _selectedAsset.HasCustomColors = EditorGUILayout.Toggle("Has custom colors: ", _selectedAsset.HasCustomColors);
            if (_selectedAsset.HasCustomColors)
            {
                _selectedAsset.ColorCount = Mathf.RoundToInt(EditorGUILayout.Slider("Color Count: ", _selectedAsset.ColorCount, 1, 4));
                for (int i = 0; i < _selectedAsset.ColorCount; i++)
                {
                    _selectedAsset.Colors[i] = EditorGUILayout.ColorField("Color " + (i + 1), _selectedAsset.Colors[i]);

                }
            }

            GUILayout.Label("Light settings", EditorStyles.boldLabel);
			_selectedAsset.LightsTurnOnAtNight = EditorGUILayout.Toggle("Turn on at night: ", _selectedAsset.LightsTurnOnAtNight);
            if (_selectedAsset.LightsTurnOnAtNight && _selectedAsset.HasCustomColors) {
                _selectedAsset.LightsUseCustomColors = EditorGUILayout.Toggle("Use custom colors: ", _selectedAsset.LightsUseCustomColors);
                if (_selectedAsset.LightsUseCustomColors) {
                    _selectedAsset.LightsCustomColorSlot = (int)(CustomColorSlot)EditorGUILayout.EnumPopup("Custom color slot:", (CustomColorSlot)_selectedAsset.LightsCustomColorSlot);
                }
            }

            // Type specific settings
            switch (_selectedAsset.Type)
            {
                case AssetType.Wall:
                    DrawAssetWallDetailSection();
                    goto case AssetType.Deco;
                case AssetType.Deco:
                    DrawAssetDecoDetailSection();
                    break;
                case AssetType.Bench:
                    DrawAssetSeatingDetailSection();
                    break;
                case AssetType.Fence:
                    DrawAssetFenceDetailSection();
                    break;
                case AssetType.Sign:
                    DrawAssetSignDetailSection();
                    break;
                case AssetType.Tv:
                    DrawAssetTvDetailSection();
                    break;
                case AssetType.FlatRide:
                    DrawAssetFlatRideDetailSection();
                    break;
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Remove From Asset Pack"))
            {
                RemoveAsset(_selectedAsset);
            }
        }

        /// <summary>
        /// Draws the asset deco detail section.
        /// </summary>
        private void DrawAssetDecoDetailSection()
        {
            // Category settings
            GUILayout.Label("Category in the deco window:", EditorStyles.boldLabel);
            _selectedAsset.Category = EditorGUILayout.TextField("Category:", _selectedAsset.Category);
            _selectedAsset.SubCategory = EditorGUILayout.TextField("Sub category:", _selectedAsset.SubCategory);

            // Placement settings
            GUILayout.Label("Placement settings", EditorStyles.boldLabel);
	        if (_selectedAsset.Type != AssetType.Wall)
			{
				_selectedAsset.BuildOnGrid = EditorGUILayout.Toggle("Force build on grid: ", _selectedAsset.BuildOnGrid);
				_selectedAsset.SnapCenter = EditorGUILayout.Toggle("Snaps to center: ", _selectedAsset.SnapCenter);
				_selectedAsset.GridSubdivision = Mathf.RoundToInt(EditorGUILayout.Slider("Grid subdivision: ", _selectedAsset.GridSubdivision, 1, 9));
			}
            _selectedAsset.HeightDelta = Mathf.RoundToInt(EditorGUILayout.Slider("Height delta: ", _selectedAsset.HeightDelta, 0.05f, 1) * 200f) / 200f;
            
	        GUILayout.Label("Size settings", EditorStyles.boldLabel);
			_selectedAsset.IsResizable = EditorGUILayout.Toggle("Is resizable: ", _selectedAsset.IsResizable);

	        if (_selectedAsset.IsResizable)
			{
				_selectedAsset.MinSize = Mathf.RoundToInt(EditorGUILayout.Slider("Min size: ", _selectedAsset.MinSize, 0.1f, _selectedAsset.MaxSize) * 10f) / 10f;
				_selectedAsset.MaxSize = Mathf.RoundToInt(EditorGUILayout.Slider("Max size: ", _selectedAsset.MaxSize, _selectedAsset.MinSize, 10) * 10f) / 10f;
			}

	        GUILayout.Label("Visibility settings", EditorStyles.boldLabel);
			_selectedAsset.CanSeeThrough = EditorGUILayout.Toggle("Can see through: ", _selectedAsset.CanSeeThrough);
			_selectedAsset.BlocksRain = EditorGUILayout.Toggle("Blocks rain: ", _selectedAsset.BlocksRain);
        }


        /// <summary>
        /// Draws the asset flatride detail section.
        /// </summary>
        private void DrawAssetFlatRideDetailSection()
        {
            //shows the rating of the ride
            GUILayout.Label("Rating", EditorStyles.boldLabel);
            _selectedAsset.Excitement = EditorGUILayout.Slider("Excitement", _selectedAsset.Excitement * 100, 0, 100)/100f;
            _selectedAsset.Intensity =EditorGUILayout.Slider("Intensity",_selectedAsset.Intensity* 100, 0, 100)/100f;
            _selectedAsset.Nausea = EditorGUILayout.Slider("Nausea",_selectedAsset.Nausea* 100, 0, 100)/100f;

            //the footprint that the ride covers
            GUILayout.Label("Ride Footprint", EditorStyles.boldLabel);
            _selectedAsset.FootprintX = EditorGUILayout.IntField("X", _selectedAsset.FootprintX);
            _selectedAsset.FootprintZ = EditorGUILayout.IntField("Z", _selectedAsset.FootprintZ);


            //category of the ride
            GUILayout.Label("Category", EditorStyles.boldLabel);
            _selectedAsset.FlatRideCategory = AttractionType.CategoryTag[
                EditorGUILayout.Popup("category",
                    Array.IndexOf(AttractionType.CategoryTag, _selectedAsset.FlatRideCategory),
                    AttractionType.CategoryDisplayName)];

            //waypoint tool for NPC pathing
            GUILayout.Label("Waypoints", EditorStyles.boldLabel);
            _selectedAsset.EnableEditing =
                GUILayout.Toggle(_selectedAsset.EnableEditing, "Enable Editing Waypoints", "Button");

            if (_selectedAsset.EnableEditing)
            {

                GUILayout.Label("S - Snap to axis of connected waypoints");
                _selectedAsset.HelperPlaneY = EditorGUILayout.FloatField("Helper Plane Y", _selectedAsset.HelperPlaneY);

                //generates an initial gride of waypoints around the outer squares
                if (GUILayout.Button("Generate outer grid"))
                {

                    float minX = -_selectedAsset.FootprintX / 2;
                    float maxX = _selectedAsset.FootprintX / 2;
                    float minZ = -_selectedAsset.FootprintZ / 2;
                    float maxZ = _selectedAsset.FootprintZ / 2;
                    for (int xi = 0; xi < Mathf.RoundToInt(maxX - minX); xi++)
                    {
                        for (int zi = 0; zi < Mathf.RoundToInt(maxZ - minZ); zi++)
                        {
                            float x = minX + xi;
                            float z = minZ + zi;
                            if (!(x == minX || x == maxX - 1) && !(z == minZ || z == maxZ - 1))
                            {
                                continue;
                            }

                            Waypoint newWaypoint = new Waypoint();
                            newWaypoint.Position = new Vector3(x + 0.5f, 0, z + 0.5f);
                            newWaypoint.IsOuter = true;
                            _selectedAsset.Waypoints.Add(newWaypoint);
                        }
                    }
                }

                //adds a waypoint at (0,0,0) relative to the unity object
                if (GUILayout.Button("Add Waypoint"))
                {
                    Waypoint.addWaypoint(_selectedAsset, _selectedAsset.GameObject.transform.position);
                }

                //clears all the waypoint
                if (GUILayout.Button("Clear all"))
                {
                    _selectedAsset.Waypoints.Clear();
                    _selectedAsset.SelectedWaypoint = null;
                }
            }



            //provies a list of all the waypoints
            for (int i = 0; i < _selectedAsset.Waypoints.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("#" + i);
                _selectedAsset.Waypoints[i].IsOuter =
                    GUILayout.Toggle(_selectedAsset.Waypoints[i].IsOuter, "IsOuter", "Button");
                _selectedAsset.Waypoints[i].IsRabbitHoleGoal =
                    GUILayout.Toggle(_selectedAsset.Waypoints[i].IsRabbitHoleGoal, "IsRabbitHoleGoal", "Button");
                if (GUILayout.Button("Delete"))
                {
                    Waypoint.DeletePoint(_selectedAsset, _selectedAsset.SelectedWaypoint);
                }

                EditorGUILayout.EndHorizontal();

                _selectedAsset.Waypoints[i].Position =
                    EditorGUILayout.Vector3Field("Position", _selectedAsset.Waypoints[i].Position);
            }


            var seats = _selectedAsset.
                GameObject.
                GetComponentsInChildren<Transform>(true).
                Where(transform => transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

            // Bench settings
            GUILayout.Label("Bench settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Seats found", seats.Count().ToString());

            foreach (Transform seat in seats)
            {
                if (GUILayout.Button(seat.name))
                {
                    EditorGUIUtility.PingObject(seat.gameObject.GetInstanceID());
                    Selection.activeGameObject = seat.gameObject;
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Add seat"))
            {
                var seat = new GameObject("Seat");

                seat.transform.SetParent(_selectedAsset.GameObject.transform);

                seat.transform.localPosition = Vector3.zero;

                Selection.activeGameObject = seat.gameObject;
            }

        }


        /// <summary>
        /// Draws the asset deco detail section.
        /// </summary>
        private void DrawAssetWallDetailSection()
        {
            // Category settings
            GUILayout.Label("Wall settings:", EditorStyles.boldLabel);
            var north = EditorGUILayout.Toggle("Block North", Convert.ToBoolean(_selectedAsset.WallSettings & (int) WallBlock.Forward));
            var east = EditorGUILayout.Toggle("Block East", Convert.ToBoolean(_selectedAsset.WallSettings & (int) WallBlock.Right));
            var south = EditorGUILayout.Toggle("Block South", Convert.ToBoolean(_selectedAsset.WallSettings & (int) WallBlock.Back));
            var west = EditorGUILayout.Toggle("Block West", Convert.ToBoolean(_selectedAsset.WallSettings & (int) WallBlock.Left));

            var wallSettings = 0;

            if (north)
                wallSettings |= (int) WallBlock.Forward;
            if (east)
                wallSettings |= (int) WallBlock.Right;
            if (south)
                wallSettings |= (int) WallBlock.Back;
            if (west)
                wallSettings |= (int) WallBlock.Left;

            _selectedAsset.WallSettings = wallSettings;
        }

        /// <summary>
        /// Draws the asset seating detail section.
        /// </summary>
        private void DrawAssetSeatingDetailSection()
        {
	        if (_selectedAsset.GameObject == null)
	        {
		        return;
	        }

	        var seats = _selectedAsset.
				GameObject.
				GetComponentsInChildren<Transform>(true).
				Where(transform => transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

            // Bench settings
	        GUILayout.Label("Bench settings", EditorStyles.boldLabel);
	        _selectedAsset.HasBackRest = EditorGUILayout.Toggle("Has back rest: ", _selectedAsset.HasBackRest);

			EditorGUILayout.LabelField("Seats found", seats.Count().ToString());

	        foreach (Transform seat in seats)
			{
				if (GUILayout.Button(seat.name))
				{
					EditorGUIUtility.PingObject(seat.gameObject.GetInstanceID());
				    Selection.activeGameObject = seat.gameObject;
				}
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Add seat"))
            {
                var seat = new GameObject("Seat");

                seat.transform.SetParent(_selectedAsset.GameObject.transform);

                seat.transform.localPosition = Vector3.zero;

                Selection.activeGameObject = seat.gameObject;
            }
        }
		
        /// <summary>
        /// Draws the asset fence detail section.
        /// </summary>
        private void DrawAssetFenceDetailSection()
        {
			// Game object drop box
			GUILayout.Label("Category in the deco window:", EditorStyles.boldLabel);
	        _selectedAsset.Category = EditorGUILayout.TextField("Category:", _selectedAsset.Category);
	        _selectedAsset.SubCategory = EditorGUILayout.TextField("Sub category:", _selectedAsset.SubCategory);

	        var post = EditorGUILayout.ObjectField("Post:", _selectedAsset.FencePost, typeof(GameObject), true) as GameObject;

	        if (post != _selectedAsset.FencePost)
			{
				post.transform.SetParent(_selectedAsset.GameObject.transform);
				post.transform.localPosition = new Vector3(0.5f, 0, 0);

				if (_selectedAsset.FencePost != null)
				{
					DestroyImmediate(_selectedAsset.FencePost);
				}

				_selectedAsset.FencePost = post;
				post.name = "Post";
			}
	        _selectedAsset.HasMidPost = EditorGUILayout.Toggle("Has mid post: ", _selectedAsset.HasMidPost);
		}
		
        /// <summary>
        /// Draws the asset sign detail section.
        /// </summary>
        private void DrawAssetSignDetailSection()
        {
            GUILayout.Label("Sign settings:", EditorStyles.boldLabel);
            var text = EditorGUILayout.ObjectField("Text object:", _selectedAsset.Text, typeof(GameObject), true) as GameObject;

	        if (text != _selectedAsset.Text)
			{
				_selectedAsset.Text = text;
				text.name = "Text";
			}
		}
		
        /// <summary>
        /// Draws the asset tv detail section.
        /// </summary>
        private void DrawAssetTvDetailSection()
        {
            GUILayout.Label("Tv settings:", EditorStyles.boldLabel);
            var screen = EditorGUILayout.ObjectField("Screen object:", _selectedAsset.Screen, typeof(GameObject), true) as GameObject;

	        if (screen != _selectedAsset.Text)
			{
				_selectedAsset.Screen = screen;
				screen.name = "Screen";
			}
		}

        /// <summary>
        /// Selects an asset to draw its settings
        /// </summary>
        /// <param name="asset">The asset.</param>
        private void SelectAsset(Asset asset)
        {
            _selectedAsset = asset;
            
            EditorGUIUtility.PingObject(_selectedAsset.GameObject.GetInstanceID());

            Selection.activeGameObject = _selectedAsset.GameObject;
        }

        /// <summary>
        /// Removes an asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        private void RemoveAsset(Asset asset)
        {
            if (asset == _selectedAsset)
            {
                _selectedAsset = null;
            }

            ProjectManager.AssetPack.Remove(asset);
        }
    }

}