using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
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
	        if (Convert.ToBoolean(asset.WallSettings & (int) WallBlock.Forward))
	        {
		        Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, 0, 0.5f), asset.GameObject.transform.position + new Vector3(0.5f, 1, 0.5f));
		        Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, 1, 0.5f), asset.GameObject.transform.position + new Vector3(0.5f, 0, 0.5f));
	        }

	        if (Convert.ToBoolean(asset.WallSettings & (int) WallBlock.Back))
			{
				Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, 0, -0.5f), asset.GameObject.transform.position + new Vector3(-0.5f, 1, -0.5f));
				Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, 1, -0.5f), asset.GameObject.transform.position + new Vector3(-0.5f, 0, -0.5f));
			}

	        if (Convert.ToBoolean(asset.WallSettings & (int) WallBlock.Left))
	        {
		        Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, 0, -0.5f), asset.GameObject.transform.position + new Vector3(-0.5f, 1, 0.5f));
		        Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(-0.5f, 1, -0.5f), asset.GameObject.transform.position + new Vector3(-0.5f, 0, 0.5f));
	        }

	        if (Convert.ToBoolean(asset.WallSettings & (int) WallBlock.Right))
			{
				Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, 0, 0.5f), asset.GameObject.transform.position + new Vector3(0.5f, 1, -0.5f));
				Gizmos.DrawLine(asset.GameObject.transform.position + new Vector3(0.5f, 1, 0.5f), asset.GameObject.transform.position + new Vector3(0.5f, 0, -0.5f));
			}
        }
    }
}
