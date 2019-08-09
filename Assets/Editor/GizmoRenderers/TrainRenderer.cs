using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ParkitectAssetEditor.GizmoRenderers
{
	/// <summary>
	/// Renders a grid around decos and walls.
	/// </summary>
	/// <seealso cref="IGizmoRenderer" />
	class TrainRenderer : IGizmoRenderer
	{
		private Material sceneViewMaterial;

		/// <summary>
		/// Determines whether this instance can render the specified asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <returns>
		///   <c>true</c> if this instance can render the specified asset; otherwise, <c>false</c>.
		/// </returns>
		public bool CanRender(Asset asset)
		{
			return asset.Type == AssetType.Train;
		}

		/// <summary>
		/// Renders the specified asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		public void Render(Asset asset)
		{
			if (sceneViewMaterial == null)
			{
				sceneViewMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Editor/SceneViewGhostMaterial.mat", typeof(Material));
			}

			if (asset.LeadCar != null && asset.LeadCar.GameObject != null && asset.Car != null && asset.Car.GameObject != null) {
				GameObject car = asset.Car.GameObject;
				GameObject leadCar = asset.LeadCar.GameObject;

				float lengthAxis = 0;
				Transform backAxisMarker = leadCar.transform.Find("backAxis");
				if (backAxisMarker != null) {
					lengthAxis = Mathf.Abs(backAxisMarker.localPosition.z);
				}

				car.transform.position = leadCar.transform.position - leadCar.transform.forward * (lengthAxis + asset.LeadCar.OffsetBack + asset.Car.OffsetFront);
			}

			DrawCar(asset.LeadCar);
			DrawCar(asset.Car);
		}

		private void DrawCar(CoasterCar car) {
			if (car == null || car.GameObject == null)
			{
				return;
			}

			GameObject carGO = car.GameObject;

			Utility.Utility.renderSeatGizmo(carGO);

			Color gizmoColor = Gizmos.color;
			Gizmos.color = Color.red;
			var seats = carGO.
				GetComponentsInChildren<Transform>(true).Where(transform => transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

			foreach (Transform seatTransform in seats)
			{
				Vector3 position = seatTransform.position + seatTransform.forward * car.SeatWaypointOffset - Vector3.up * 0.06f;
				Gizmos.DrawSphere(position, 0.01f);
			}
			Gizmos.color = gizmoColor;

			Gizmos.color = Color.white;
			Vector3 frontPosition = carGO.transform.position + carGO.transform.forward * car.OffsetFront;
			Gizmos.DrawLine(frontPosition, frontPosition + Vector3.up * 0.5f);
			Handles.Label(frontPosition + Vector3.up * 0.5f + carGO.transform.forward * 0.1f, "Front");

			Transform backAxis = carGO.transform.Find("backAxis");
			Vector3 backPosition = backAxis.position;
			backPosition -= carGO.transform.forward * car.OffsetBack;

			EditorGUI.BeginChangeCheck();
			Gizmos.DrawLine(backPosition, backPosition + Vector3.up * 0.5f);
			Handles.Label(backPosition - Vector3.up * 0.1f - carGO.transform.forward * 0.1f, "Back");

			foreach (CoasterRestraints restraints in car.Restraints)
			{
				var restraintTransforms = carGO.
					GetComponentsInChildren<Transform>(true).Where(transform => transform.name.StartsWith(restraints.TransformName, true, CultureInfo.InvariantCulture));

				foreach (Transform restraintTransform in restraintTransforms)
				{
					MeshFilter meshFilter = restraintTransform.GetComponent<MeshFilter>();
					if (meshFilter != null)
					{
						sceneViewMaterial.SetPass(0);
						Graphics.DrawMeshNow(meshFilter.sharedMesh, restraintTransform.position, restraintTransform.rotation * Quaternion.Euler(restraints.ClosedAngle, 0, 0));
						sceneViewMaterial.SetPass(1);
						Graphics.DrawMeshNow(meshFilter.sharedMesh, restraintTransform.position, restraintTransform.rotation * Quaternion.Euler(restraints.ClosedAngle, 0, 0));
					}
				}
			}
		}
	}
}
