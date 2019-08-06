using System.Globalization;
using System.Linq;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
{
	/// <summary>
	/// Renders guests on a bench
	/// </summary>
	/// <seealso cref="IGizmoRenderer" />
	class SeatRenderer : IGizmoRenderer
	{
		private Mesh npcMesh;
		private Material sceneViewMaterial;
		
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
			return asset.Type == AssetType.Bench | asset.Type == AssetType.FlatRide;
		}

		/// <inheritdoc />
		/// <summary>
		/// Renders the specified asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		public void Render(Asset asset)
		{
			if (npcMesh == null)
			{
				npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting");
			}

			if (sceneViewMaterial == null)
			{
				sceneViewMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Editor/SceneViewGhostMaterial.mat", typeof(Material));
			}

			var seats = asset.
				GameObject.
				GetComponentsInChildren<Transform>(true).
				Where(transform => transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

			foreach (var seat in seats)
			{
				sceneViewMaterial.SetPass(0);
				Graphics.DrawMeshNow(npcMesh, seat.position, seat.rotation); 
				sceneViewMaterial.SetPass(1);
				Graphics.DrawMeshNow(npcMesh, seat.position, seat.rotation);
			}
		}
	}
}
