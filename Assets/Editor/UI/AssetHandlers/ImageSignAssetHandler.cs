using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class ImageSignAssetHandler : DecoAssetHandler
    {
        public override void DrawDetailsSection(Asset _selectedAsset)
        {
            GUILayout.Label("Image Sign settings:", EditorStyles.boldLabel);
            var sign = EditorGUILayout.ObjectField("Sign object:", _selectedAsset.Screen, typeof(GameObject), true) as GameObject;

            if (sign != _selectedAsset.Screen)
            {
                _selectedAsset.Screen = sign;
                sign.name = "Sign";
            }

            _selectedAsset.AspectRatio = (AspectRatio)EditorGUILayout.Popup("Aspect ratio", (int)_selectedAsset.AspectRatio, AspectRatioUtility.aspectRatioNames);
            base.DrawDetailsSection(_selectedAsset);
        }
    }
}
