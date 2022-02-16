using System.Collections.Generic;
using System.IO;
using System.Linq;
using ParkitectAssetEditor.GizmoRenderers;
using ParkitectAssetEditor.UI.AssetHandlers;
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


        private static readonly IGizmoRenderer[] _gizmoRenderers = {
            new SeatRenderer(),
            new WallRenderer(),
            new GridRenderer(),
            new SignRenderer(),
            new FootprintRenderer(),
            new WaypointRenderer(),
            new BoundingBoxRenderer(),
            new TrainRenderer(),
        };

        private static readonly Dictionary<AssetType, IAssetTypeHandler> assetHandlers = new Dictionary<AssetType, IAssetTypeHandler>
        {
            { AssetType.Bench, new BenchAssetHandler() },
            { AssetType.FlatRide, new FlatRideAssetHandler() },
            { AssetType.Train, new TrainAssetHandler() },
            { AssetType.Deco, new DecoAssetHandler() },
            { AssetType.Wall, new WallAssetHandler() },
            { AssetType.ImageSign, new ImageSignAssetHandler() },
            { AssetType.Fence, new FenceAssetHandler() },
            { AssetType.Sign, new SignAssetHandler() },
            { AssetType.Tv, new TVAssetHandler() },
            { AssetType.Trashbin, new DefaultAssetHandler(AssetType.Trashbin) },
            { AssetType.Lamp, new DefaultAssetHandler(AssetType.Lamp) },
            { AssetType.ParticleEffect, new ParticleEffectAssetHandler() },
            { AssetType.Custom, new CustomAssetHandler() },
        };

        [MenuItem("Window/Parkitect Asset Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(AssetEditorWindow));
        }

        private void Awake()
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
        private static void DrawGizmoForMyScript(Transform scr, GizmoType gizmoType)
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

        private void OnFocus()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.duringSceneGui -= OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDestroy()
        {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            //Exporter.SaveToXML(ModManager);
            SceneView.duringSceneGui -= OnSceneGUI;

        }

        private void OnSceneGUI(SceneView sceneView)
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
            ProjectManager.AssetPack.OrderPriority = EditorGUILayout.IntField("Load Order Priority", ProjectManager.AssetPack.OrderPriority);

            GUILayout.Label("Assemblies", EditorStyles.boldLabel);
            //adds a waypoint at (0,0,0) relative to the unity object
            if (GUILayout.Button("Add Assembly"))
            {
                ProjectManager.AssetPack.Assemblies.Add("");
            }

            //provides a list of all the waypoints
            for (int i = 0; i < ProjectManager.AssetPack.Assemblies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("#" + i);

                ProjectManager.AssetPack.Assemblies[i] = EditorGUILayout.TextField(ProjectManager.AssetPack.Assemblies[i]);

                if (GUILayout.Button("Delete"))
                {
                    ProjectManager.AssetPack.Assemblies.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Stores your raw model files with the asset pack in an archive. Recommended: on", MessageType.Info);
            ProjectManager.AssetPack.ArchiveAssets = EditorGUILayout.Toggle("Archive assets", ProjectManager.AssetPack.ArchiveAssets);

            ProjectManager.AssetPack.VersionNumber = EditorGUILayout.TextField("Version number", ProjectManager.AssetPack.VersionNumber);

            GUILayout.Space(10);

            if (GUILayout.Button("Export Asset Pack"))
            {
                if (ProjectManager.Export(ProjectManager.AssetPack.ArchiveAssets))
                {
                    GUIUtility.ExitGUI();
                }
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
            Asset removedAsset = null;

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

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(asset.Name, style))
                {
                    SelectAsset(asset);
                }

                style = new GUIStyle(GUI.skin.button)
                {
                    stretchWidth = false
                };

                if (GUILayout.Button("Remove", style))
                {
                    removedAsset = asset;
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (removedAsset != null)
            {
                RemoveAsset(removedAsset);
            }
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
            _selectedAsset.Type = (AssetType)EditorGUILayout.EnumPopup("Type", _selectedAsset.Type);
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
            if (_selectedAsset.LightsTurnOnAtNight && _selectedAsset.HasCustomColors)
            {
                _selectedAsset.LightsUseCustomColors = EditorGUILayout.Toggle("Use custom colors: ", _selectedAsset.LightsUseCustomColors);
                if (_selectedAsset.LightsUseCustomColors)
                {
                    _selectedAsset.LightsCustomColorSlot = (int)(CustomColorSlot)EditorGUILayout.EnumPopup("Custom color slot:", (CustomColorSlot)_selectedAsset.LightsCustomColorSlot);
                }
            }

            if (assetHandlers.TryGetValue(_selectedAsset.Type, out var handler))
            {
                handler.DrawDetailsSection(_selectedAsset);
            }
            else
            {
                Debug.LogWarning($"No asset handler has been defined for {_selectedAsset.Type}");
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Remove From Asset Pack"))
            {
                RemoveAsset(_selectedAsset);
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