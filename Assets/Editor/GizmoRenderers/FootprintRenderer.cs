using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ParkitectAssetEditor.GizmoRenderers
{
    public class FootprintRenderer : IGizmoRenderer
    {
        public bool CanRender(Asset asset)
        {
            return asset.Type == AssetType.FlatRide;
        }

        public void Render(Asset asset)
        {
            Vector3 topLeft = new Vector3(-((float)asset.FootprintX) / 2.0f, 0, (float)asset.FootprintZ / 2.0f) + asset.GameObject.transform.position;
            Vector3 topRight = new Vector3(((float)asset.FootprintX) / 2.0f, 0, (float)asset.FootprintZ / 2.0f) + asset.GameObject.transform.position;
            Vector3 bottomLeft = new Vector3(-((float)asset.FootprintX) / 2.0f, 0, -(float)asset.FootprintZ / 2.0f) + asset.GameObject.transform.position;
            Vector3 bottomRight = new Vector3(((float)asset.FootprintX) / 2.0f, 0, -(float)asset.FootprintZ / 2.0f) + asset.GameObject.transform.position;

            Color fill = Color.white;
            fill.a = 0.1f;
            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(new[] {topLeft, topRight, bottomRight, bottomLeft}, fill, Color.black);
        }

    }
}
