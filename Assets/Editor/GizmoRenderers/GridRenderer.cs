using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ParkitectAssetEditor.GizmoRenderers
{
	/// <summary>
	/// Renders a grid around decos and walls.
	/// </summary>
	/// <seealso cref="IGizmoRenderer" />
	class GridRenderer : IGizmoRenderer
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
			return asset.Type == AssetType.Deco || asset.Type == AssetType.Wall;
		}

		/// <summary>
		/// Renders the specified asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		public void Render(Asset asset)
		{
			var min = asset.SnapCenter ?  - 2.5f : -3f;
			var max = asset.SnapCenter ? 2.5f : 3f;

			Handles.zTest = CompareFunction.LessEqual;

			for (float x = min; x <= max; x += 1 / asset.GridSubdivision)
			{
				Handles.DrawLine(asset.GameObject.transform.position + new Vector3(x, 0, min), asset.GameObject.transform.position + new Vector3(x, 0, max));
			}
			for (float z = min; z <= max; z += 1 / asset.GridSubdivision)
			{
				Handles.DrawLine(asset.GameObject.transform.position + new Vector3(min, 0, z), asset.GameObject.transform.position + new Vector3(max, 0, z));
			}

			Handles.zTest = CompareFunction.Always;
		}
	}
}
