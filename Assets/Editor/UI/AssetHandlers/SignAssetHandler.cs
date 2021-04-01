using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class SignAssetHandler : IAssetTypeHandler
    {
        public void DrawDetailsSection(Asset selectedAsset)
        {
            GUILayout.Label("Sign settings:", EditorStyles.boldLabel);
            var text = EditorGUILayout.ObjectField("Text object:", selectedAsset.Text, typeof(GameObject), true) as GameObject;

            if (text != selectedAsset.Text)
            {
                selectedAsset.Text = text;
                text.name = "Text";
            }
        }
    }
}
