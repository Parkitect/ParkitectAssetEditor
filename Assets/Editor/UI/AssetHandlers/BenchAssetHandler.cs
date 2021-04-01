using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class BenchAssetHandler : SeatAssetHandler
    {
        public override void DrawDetailsSection(Asset selectedAsset)
        {
            if (selectedAsset.GameObject == null)
            {
                return;
            }

            // Bench settings
            GUILayout.Label("Bench settings", EditorStyles.boldLabel);
            selectedAsset.HasBackRest = EditorGUILayout.Toggle("Has back rest: ", selectedAsset.HasBackRest);

            GUILayout.Space(15);

            base.DrawDetailsSection(selectedAsset);
        }
    }
}
