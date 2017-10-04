using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParkitectAssetEditor.Assets.Editor.GizmoRenderers
{
    /// <summary>
    /// Renders the blocking sides of a wall.
    /// </summary>
    /// <seealso cref="IGizmoRenderer" />
    class WallRenderer : IGizmoRenderer
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
            return asset.Type == AssetType.Wall;
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Render(Asset asset)
        {
            if(Convert.ToBoolean(asset.WallSettings & (int)WallBlock.North))
                Gizmos.DrawLine(asset.GameObject.transform.position, asset.GameObject.transform.position + Vector3.forward);
            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.South))
                Gizmos.DrawLine(asset.GameObject.transform.position, asset.GameObject.transform.position + Vector3.back);
            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.West))
                Gizmos.DrawLine(asset.GameObject.transform.position, asset.GameObject.transform.position + Vector3.left);
            if (Convert.ToBoolean(asset.WallSettings & (int)WallBlock.East))
                Gizmos.DrawLine(asset.GameObject.transform.position, asset.GameObject.transform.position + Vector3.right);
        }
    }
}
