using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class TVAssetHandler : IAssetTypeHandler
    {
        public void DrawDetailsSection(Asset selectedAsset)
        {
            GUILayout.Label("Tv settings:", EditorStyles.boldLabel);
            var screen = EditorGUILayout.ObjectField("Screen object:", selectedAsset.Screen, typeof(GameObject), true) as GameObject;

            if (screen != selectedAsset.Screen)
            {
                selectedAsset.Screen = screen;
                screen.name = "Screen";
            }
        }
    }
}
