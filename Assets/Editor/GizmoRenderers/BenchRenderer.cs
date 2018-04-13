using System.Globalization;
using System.Linq;
using Microsoft.CSharp;
using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
{
	/// <summary>
	/// Renders guests on a bench
	/// </summary>
	/// <seealso cref="IGizmoRenderer" />
	class BenchRenderer : IGizmoRenderer
	{
	    private Mesh npcMesh;
        
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
            return asset.Type == AssetType.Bench | asset.Type == AssetType.Flatride;
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Render(Asset asset)
        {
            if (npcMesh == null)
                npcMesh = Resources.Load<Mesh>("Reference Objects/reference_sitting_guest");
            var seats = asset.
                GameObject.
                GetComponentsInChildren<Transform>(true).
                Where(transform => transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

            foreach (var seat in seats)
            {
                Gizmos.DrawMesh(npcMesh,seat.position);
               
            }
        }
    }
}
