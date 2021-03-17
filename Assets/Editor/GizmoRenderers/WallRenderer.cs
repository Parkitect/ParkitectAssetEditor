using System;
using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
{
    /// <summary>
    /// Renders the blocking sides of a wall.
    /// </summary>
    /// <seealso cref="IGizmoRenderer" />
    internal class WallRenderer : IGizmoRenderer
    {
        /// <inheritdoc />
        /// <summary>
        /// Determines whether this instance can render the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>
        ///   <c>true</c> if this instance can render the specified asset; otherwise, <c>false</c>.
        /// </returns>
        public bool CanRender(Asset asset)
        {
            return asset.Type == AssetType.Wall || asset.Type == AssetType.Fence;
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Render(Asset asset)
        {
            Vector3 offset = Vector3.zero;
            if (asset.Type == AssetType.Fence)
            {
                offset = Vector3.forward / 2;
            }

            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.Forward))
            {
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, 0, 0.5f) + offset, asset.GameObject.transform.position + new Vector3(0.5f, asset.Height, 0.5f) + offset);
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, asset.Height, 0.5f) + offset, asset.GameObject.transform.position + new Vector3(0.5f, 0, 0.5f) + offset);
            }

            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.Back))
            {
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, 0, -0.5f) + offset, asset.GameObject.transform.position + new Vector3(-0.5f, asset.Height, -0.5f) + offset);
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, asset.Height, -0.5f) + offset, asset.GameObject.transform.position + new Vector3(-0.5f, 0, -0.5f) + offset);
            }

            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.Left))
            {
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, 0, -0.5f) + offset, asset.GameObject.transform.position + new Vector3(-0.5f, asset.Height, 0.5f) + offset);
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, asset.Height, -0.5f) + offset, asset.GameObject.transform.position + new Vector3(-0.5f, 0, 0.5f) + offset);
            }

            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.Right))
            {
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, 0, 0.5f) + offset, asset.GameObject.transform.position + new Vector3(0.5f, asset.Height, -0.5f) + offset);
                Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, asset.Height, 0.5f) + offset, asset.GameObject.transform.position + new Vector3(0.5f, 0, -0.5f) + offset);
            }
        }
    }
}
