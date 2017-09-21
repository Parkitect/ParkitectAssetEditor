using System.Globalization;
using System.Linq;
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

        /// <summary>
        /// The selected asset.
        /// </summary>
        private Asset _selectedAsset;

        [MenuItem("Window/Parkitect Asset Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(AssetEditorWindow));
        }

        void Awake()
        {
            // Make sure it doesn't autoload the project on unity start. This editor pref is just for when unity loses its state when it compiles.
            EditorPrefs.SetString("loadedProject", null);
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
                if (Selection.activeGameObject != _selectedAsset?.GameObject)
                {
                    var asset = ProjectManager.AssetPack?.Assets?.FirstOrDefault(a => a.GameObject == Selection.activeGameObject);

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
                EditorGUILayout.HelpBox("Load a project first", MessageType.Info);

                if (GUILayout.Button("Open project"))
                {
                    LoadProjectWindow.Show();
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

        /// <summary>
        /// Draws the asset pack settings section.
        /// </summary>
        private void DrawAssetPackSettingsSection()
        {
            GUILayout.Label("Asset Pack Settings", "PreToolbar");

            ProjectManager.AssetPack.Name = EditorGUILayout.TextField("Name", ProjectManager.AssetPack.Name);

            GUILayout.Space(10);

            if (GUILayout.Button("Export Asset Pack"))
            {
                ProjectManager.Save();
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
            if (newAsset != null)
            {
                SelectAsset(ProjectManager.AssetPack.Add(newAsset));
            }

            // Asset list
            _assetListScrollPos = EditorGUILayout.BeginScrollView(_assetListScrollPos, "GroupBox", GUILayout.Height(200));
            foreach (var asset in ProjectManager.AssetPack.Assets)
            {
                if (GUILayout.Button(asset.Name))
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

            GUILayout.Label($"{_selectedAsset.Name} Settings", "PreToolbar");

            // Name, type and price settings
            GUILayout.Label("General:", EditorStyles.boldLabel);
            _selectedAsset.Name = EditorGUILayout.TextField("Name", _selectedAsset.Name);
            _selectedAsset.Type = (AssetType)EditorGUILayout.Popup("Type", (int)_selectedAsset.Type, new[]
            {
                AssetType.Deco.ToString(),
                AssetType.Trashbin.ToString(),
                AssetType.Bench.ToString(),
                AssetType.Fence.ToString(),
                AssetType.Lamp.ToString(),
            });
            _selectedAsset.Price = EditorGUILayout.FloatField("Price:", _selectedAsset.Price);
            
            // Type specific settings
            switch (_selectedAsset.Type)
            {
                case AssetType.Deco:
                    DrawAssetDecoDetailSection();
                    break;
                case AssetType.Bench:
                    DrawAssetSeatingDetailSection();
                    break;
                case AssetType.Fence:
                    DrawAssetFenceDetailSection();
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
            _selectedAsset.BuildOnGrid = EditorGUILayout.Toggle("Build on grid: ", _selectedAsset.BuildOnGrid);
            _selectedAsset.SnapCenter = EditorGUILayout.Toggle("Snaps to center: ", _selectedAsset.SnapCenter);
            _selectedAsset.GridSize = Mathf.RoundToInt(EditorGUILayout.Slider("Grid Size: ", _selectedAsset.GridSize, 0, 5) * 20f) / 20f;
            _selectedAsset.HeightDelta = Mathf.RoundToInt(EditorGUILayout.Slider("Height delta: ", _selectedAsset.HeightDelta, 0, 1) * 20f) / 20f;
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
                seat.AddComponent<SeatHelper>();
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
        /// Selects an asset to draw its settings
        /// </summary>
        /// <param name="asset">The asset.</param>
        private void SelectAsset(Asset asset)
        {
            _selectedAsset = asset;
            
            EditorGUIUtility.PingObject(_selectedAsset.GameObjectInstanceId);

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