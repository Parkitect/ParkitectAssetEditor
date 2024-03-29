﻿using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class CustomAssetHandler : IAssetTypeHandler
    {
        public virtual void DrawDetailsSection(Asset selectedAsset)
        {
            if (selectedAsset.GameObject == null)
            {
                return;
            }

            GUILayout.Label("Custom asset settings", EditorStyles.boldLabel);
            selectedAsset.CustomType = EditorGUILayout.TextField("Type: ", selectedAsset.CustomType);

            GUILayout.Label("Data:");
            selectedAsset.CustomData = EditorGUILayout.TextArea(selectedAsset.CustomData);
        }
    }
}
