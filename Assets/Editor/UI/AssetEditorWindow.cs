using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI
{
    public class AssetEditorWindow : EditorWindow
    {
        private GameObject newAsset;

        private Vector2 assetListScrollPos;

        private Asset _selectedAsset;

        [MenuItem("Window/Parkitect Asset Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(AssetEditorWindow));
        }

        void Update()
        {
            if (!ProjectManager.Initialized && !string.IsNullOrEmpty(EditorPrefs.GetString("assetPackPath")))
            {
                var path = EditorPrefs.GetString("assetPackPath");

                EditorPrefs.SetString("assetPackPath", "");

                Debug.Log($"Auto loading path {path}");

                ProjectManager.Load(path);

                _selectedAsset = ProjectManager.AssetPack.Assets.First();
            }
        }

        void OnGUI()
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

            GUILayout.BeginVertical();

            DrawAssetPackSection();
            GUILayout.Space(15);
            DrawAssetListSection();
            GUILayout.Space(15);
            DrawAssetDetailSection();

            GUILayout.EndVertical();
        }

        void DrawAssetPackSection()
        {
            GUILayout.Label("Asset Pack Settings", "PreToolbar");

            ProjectManager.AssetPack.Name = EditorGUILayout.TextField("Name", ProjectManager.AssetPack.Name);

            if (GUILayout.Button("Save Asset Pack"))
            {
                ProjectManager.Save();
            }
        }

        void DrawAssetListSection()
        {
            GUILayout.Label("Assets", "PreToolbar");

            EditorGUILayout.HelpBox("Drag your GameObject to this field to start editing.", MessageType.Info);

            newAsset = EditorGUILayout.ObjectField("Drop to add:", newAsset, typeof(GameObject), true) as GameObject;

            if (newAsset != null)
            {
                var asset = AddObjectAsAsset(newAsset);

                SelectAsset(asset);

                newAsset = null;
            }

            assetListScrollPos = EditorGUILayout.BeginScrollView(assetListScrollPos, "GroupBox", GUILayout.Height(200));
            foreach (Asset asset in ProjectManager.AssetPack.Assets)
            {
                if (GUILayout.Button(asset.Name))
                {
                    SelectAsset(asset);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawAssetDetailSection()
        {
            if (_selectedAsset == null)
            {
                return;
            }

            GUILayout.Label($"{_selectedAsset.Name} Settings", "PreToolbar");

            GUILayout.Label("General:", EditorStyles.boldLabel);
            _selectedAsset.Name = EditorGUILayout.TextField("Name", _selectedAsset.Name);
            _selectedAsset.Type = (AssetType)EditorGUILayout.Popup("Type", (int)_selectedAsset.Type, new[]
            {
            AssetType.Deco.ToString(),
            AssetType.Trashbin.ToString(),
            AssetType.Seating.ToString(),
            AssetType.Fence.ToString(),
            AssetType.Lamp.ToString(),
        });
            _selectedAsset.Price = EditorGUILayout.FloatField("Price:", _selectedAsset.Price);

            GUILayout.Label("Category in the deco window:", EditorStyles.boldLabel);
            _selectedAsset.Category = EditorGUILayout.TextField("Category:", _selectedAsset.Category);
            _selectedAsset.SubCategory = EditorGUILayout.TextField("Sub category:", _selectedAsset.SubCategory);

            GUILayout.Label("Placement settings", EditorStyles.boldLabel);
            _selectedAsset.GridSnap = EditorGUILayout.Toggle("Snaps to grid: ", _selectedAsset.GridSnap);
            _selectedAsset.GridSubdivision = Mathf.RoundToInt(EditorGUILayout.Slider("Grid subdivision: ", _selectedAsset.GridSubdivision, 1, 8));

            if (GUILayout.Button("Remove From Asset Pack"))
            {
                RemoveAsset(_selectedAsset);
            }
        }

        private void SelectAsset(Asset asset)
        {
            _selectedAsset = asset;

            EditorGUIUtility.PingObject(_selectedAsset.GameObjectInstanceId);

            Selection.activeGameObject = _selectedAsset.GameObject;
        }

        private void RemoveAsset(Asset asset)
        {
            _selectedAsset = null;

            ProjectManager.AssetPack.Remove(asset);
        }

        private Asset AddObjectAsAsset(GameObject gameObject)
        {
            return ProjectManager.AssetPack.Add(gameObject);
        }
    }

}