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

			CoasterCar previousCar = asset.LeadCar;
			if (asset.Car != null && asset.Car.GameObject != null) {
				if (previousCar != null && previousCar.GameObject != null) {
					GameObject previousCarObject = previousCar.GameObject;
					GameObject car = asset.Car.GameObject;

					float lengthAxis = 0;
					Transform backAxisMarker = previousCarObject.transform.Find("backAxis");
					if (backAxisMarker != null) {
						lengthAxis = Mathf.Abs(backAxisMarker.localPosition.z);
					}

					car.transform.position = previousCarObject.transform.position - previousCarObject.transform.forward * (lengthAxis + previousCar.OffsetBack + asset.Car.OffsetFront);
				}

				previousCar = asset.Car;
			}

			if (asset.RearCar != null && asset.RearCar.GameObject != null) {
				if (previousCar != null && previousCar.GameObject != null) {
					GameObject previousCarObject = previousCar.GameObject;
					GameObject car = asset.RearCar.GameObject;

					float lengthAxis = 0;
					Transform backAxisMarker = previousCarObject.transform.Find("backAxis");
					if (backAxisMarker != null) {
						lengthAxis = Mathf.Abs(backAxisMarker.localPosition.z);
					}

					car.transform.position = previousCarObject.transform.position - previousCarObject.transform.forward * (lengthAxis + previousCar.OffsetBack + asset.RearCar.OffsetFront);
				}
			}

			DrawCar(asset.LeadCar);
			DrawCar(asset.Car);
			DrawCar(asset.RearCar);
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
			if (backAxis != null) {
				Vector3 backPosition = backAxis.position;
				backPosition -= carGO.transform.forward * car.OffsetBack;

				Gizmos.DrawLine(backPosition, backPosition + Vector3.up * 0.5f);
				Handles.Label(backPosition - Vector3.up * 0.1f - carGO.transform.forward * 0.1f, "Back");
			}

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
