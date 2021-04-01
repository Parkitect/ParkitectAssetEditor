using System;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class WallAssetHandler : DecoAssetHandler
    {
        public override void DrawDetailsSection(Asset _selectedAsset)
        {
            // Category settings
            GUILayout.Label("Wall settings:", EditorStyles.boldLabel);
            _selectedAsset.Height = EditorGUILayout.FloatField("Height:", _selectedAsset.Height);
            var north = EditorGUILayout.Toggle("Block North", Convert.ToBoolean(_selectedAsset.WallSettings & (int)WallBlock.Forward));
            var east = EditorGUILayout.Toggle("Block East", Convert.ToBoolean(_selectedAsset.WallSettings & (int)WallBlock.Right));
            var south = EditorGUILayout.Toggle("Block South", Convert.ToBoolean(_selectedAsset.WallSettings & (int)WallBlock.Back));
            var west = EditorGUILayout.Toggle("Block West", Convert.ToBoolean(_selectedAsset.WallSettings & (int)WallBlock.Left));

            var wallSettings = 0;

            if (north)
                wallSettings |= (int)WallBlock.Forward;
            if (east)
                wallSettings |= (int)WallBlock.Right;
            if (south)
                wallSettings |= (int)WallBlock.Back;
            if (west)
                wallSettings |= (int)WallBlock.Left;

            _selectedAsset.WallSettings = wallSettings;

            base.DrawDetailsSection(_selectedAsset);
        }
    }
}
