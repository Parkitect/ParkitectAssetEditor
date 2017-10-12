using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
{
    /// <summary>
    /// Renders an arrow for sign direction
    /// </summary>
    /// <seealso cref="IGizmoRenderer" />
    class SignRenderer : IGizmoRenderer
    {
        /// <summary>
        /// Determines whether this instance can render the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>
        ///   <c>true</c> if this instance can render the specified asset; otherwise, <c>false</c>.
        /// </returns>
        public bool CanRender(Asset asset)
        {
            return asset.Type == AssetType.Sign;
        }

        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Render(Asset asset)
        {
            Gizmos.DrawLine(asset.GameObject.transform.position - Vector3.forward / 2, asset.GameObject.transform.position + Vector3.forward / 2);
            Gizmos.DrawLine(asset.GameObject.transform.position - Vector3.forward / 2 + new Vector3(-0.3f, 0, 0.7f), asset.GameObject.transform.position + Vector3.forward / 2);
            Gizmos.DrawLine(asset.GameObject.transform.position - Vector3.forward / 2 + new Vector3(0.3f, 0, 0.7f), asset.GameObject.transform.position + Vector3.forward / 2);
        }
    }
}
