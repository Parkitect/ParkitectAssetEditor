﻿using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class ParticleEffectAssetHandler : IAssetTypeHandler
    {
        public virtual void DrawDetailsSection(Asset selectedAsset)
        {
            // Category settings
            GUILayout.Label("Category in the deco window:", EditorStyles.boldLabel);
            selectedAsset.Category = EditorGUILayout.TextField("Category:", selectedAsset.Category);
            selectedAsset.SubCategory = EditorGUILayout.TextField("Sub category:", selectedAsset.SubCategory);

            // Placement settings
            GUILayout.Label("Placement settings", EditorStyles.boldLabel);
            selectedAsset.BuildOnGrid = EditorGUILayout.Toggle("Force build on grid: ", selectedAsset.BuildOnGrid);
            selectedAsset.SnapCenter = EditorGUILayout.Toggle("Snaps to center: ", selectedAsset.SnapCenter);
            selectedAsset.GridSubdivision = Mathf.RoundToInt(EditorGUILayout.Slider("Grid subdivision: ", selectedAsset.GridSubdivision, 1, 9));
            selectedAsset.HeightDelta = Mathf.RoundToInt(EditorGUILayout.Slider("Height delta: ", selectedAsset.HeightDelta, 0.05f, 1) * 200f) / 200f;

            GUILayout.Label("Size settings", EditorStyles.boldLabel);
            selectedAsset.IsResizable = EditorGUILayout.Toggle("Is resizable: ", selectedAsset.IsResizable);

            if (selectedAsset.IsResizable)
            {
                selectedAsset.MinSize = Mathf.RoundToInt(EditorGUILayout.Slider("Min size: ", selectedAsset.MinSize, 0.1f, selectedAsset.MaxSize) * 10f) / 10f;
                selectedAsset.MaxSize = Mathf.RoundToInt(EditorGUILayout.Slider("Max size: ", selectedAsset.MaxSize, selectedAsset.MinSize, 10) * 10f) / 10f;
                
                selectedAsset.ParticleEffectSizeModifiesScale =
                    EditorGUILayout.Toggle("Size modifies particle scale: ", selectedAsset.ParticleEffectSizeModifiesScale);
                
                selectedAsset.ParticleEffectSizeModifiesVelocity =
                    EditorGUILayout.Toggle("Size modifies particle velocity: ", selectedAsset.ParticleEffectSizeModifiesVelocity);
                
                selectedAsset.ParticleEffectSizeModifiesLifetime =
                    EditorGUILayout.Toggle("Size modifies particle lifetime: ", selectedAsset.ParticleEffectSizeModifiesLifetime);
            }

            GUILayout.Label("Effect trigger", EditorStyles.boldLabel);
            selectedAsset.EffectsTriggerEnabled =
                EditorGUILayout.Toggle("Can be triggered: ", selectedAsset.EffectsTriggerEnabled);
        }
    }
}