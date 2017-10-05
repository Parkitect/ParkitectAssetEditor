using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
{
	/// <summary>
	/// Renders guests on a bench
	/// </summary>
	/// <seealso cref="IGizmoRenderer" />
	class BenchRenderer : IGizmoRenderer
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
            return asset.Type == AssetType.Bench;
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Render(Asset asset)
        {
            var seats = asset.
                GameObject.
                GetComponentsInChildren<Transform>(true).
                Where(transform => transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

            foreach (var seat in seats)
            {
                Gizmos.DrawSphere(seat.position, 0.05f);
                var leftKnee = seat.position - seat.up * 0.02f + seat.forward * 0.078f - seat.right * 0.045f;
                Gizmos.DrawSphere(leftKnee, 0.03f);

                var rightKnee = seat.position - seat.up * 0.02f + seat.forward * 0.078f + seat.right * 0.045f;
                Gizmos.DrawSphere(rightKnee, 0.03f);

                var head = seat.position + seat.up * 0.305f + seat.forward * 0.03f;
                Gizmos.DrawSphere(head, 0.1f);

                var leftFoot = leftKnee + seat.forward * 0.015f - seat.up * 0.07f;
                Gizmos.DrawSphere(leftFoot, 0.02f);

                var rightFoot = rightKnee + seat.forward * 0.015f - seat.up * 0.07f;
                Gizmos.DrawSphere(rightFoot, 0.02f);
            }
        }
    }
}
