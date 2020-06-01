using ParkitectAssetEditor.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ParkitectAssetEditor.GizmoRenderers
{
	public class BoundingBoxRenderer : IGizmoRenderer, IHandleRenderer
	{
		public bool CanRender(Asset asset)
		{
			return asset.Type == AssetType.FlatRide || asset.Type == AssetType.Shop;
		}

		public void Render(Asset asset)
		{

		}

		public void Handle(Asset asset)
		{
			DrawBoxes(asset);
		}

		private void drawPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color fill, Color outer, Asset asset)
		{
			Handles.DrawSolidRectangleWithOutline(
				new[]
				{
					asset.GameObject.transform.TransformPoint(p1), asset.GameObject.transform.TransformPoint(p2),
					asset.GameObject.transform.TransformPoint(p3), asset.GameObject.transform.TransformPoint(p4)
				}, fill, outer);
		}

		private void DrawBoxes(Asset asset)
		{
			foreach (BoundingBox box in asset.BoundingBoxes)
			{
				if (box == asset.SelectedBoundingBox) {
					// EditorUtility.SetDirty(PMM);
					Tools.current = Tool.None;

					int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
					switch (Event.current.type)
					{
						case EventType.Layout:
							HandleUtility.AddDefaultControl(controlID);
							break;
						case EventType.KeyDown:
							if (Event.current.keyCode == KeyCode.S)
							{
								asset.BoundingBoxSnap = true;
							}

							break;
						case EventType.KeyUp:
							if (Event.current.keyCode == KeyCode.S)
							{
								asset.BoundingBoxSnap = false;
							}

							break;
					}
				}

				Vector3 diff = box.Bounds.max - box.Bounds.min;
				Vector3 diffX = new Vector3(diff.x, 0, 0);
				Vector3 diffY = new Vector3(0, diff.y, 0);
				Vector3 diffZ = new Vector3(0, 0, diff.z);

				Color fill = Color.white;
				fill.a = 0.005f;
				Color outer = Color.gray;
				if (box == asset.SelectedBoundingBox)
				{
					fill = Color.magenta;
					fill.a = 0.05f;
					outer = Color.black;
				}

				Handles.zTest = CompareFunction.Less;

				// left
				drawPlane(box.Bounds.min, box.Bounds.min + diffZ, box.Bounds.min + diffZ + diffY,
					box.Bounds.min + diffY, fill, outer, asset);

				//back
				drawPlane(box.Bounds.min, box.Bounds.min + diffX, box.Bounds.min + diffX + diffY,
					box.Bounds.min + diffY, fill, outer, asset);

				//right
				drawPlane(box.Bounds.max, box.Bounds.max - diffY, box.Bounds.max - diffY - diffZ,
					box.Bounds.max - diffZ, fill, outer, asset);

				//forward
				drawPlane(box.Bounds.max, box.Bounds.max - diffY, box.Bounds.max - diffY - diffX,
					box.Bounds.max - diffX, fill, outer, asset);

				//up
				drawPlane(box.Bounds.max, box.Bounds.max - diffX, box.Bounds.max - diffX - diffZ,
					box.Bounds.max - diffZ, fill, outer, asset);

				//down
				drawPlane(box.Bounds.min, box.Bounds.min + diffX, box.Bounds.min + diffX + diffZ,
					box.Bounds.min + diffZ, fill, outer, asset);

				Handles.zTest = CompareFunction.Always;

				if (box == asset.SelectedBoundingBox)
				{
					box.Bounds.min = handleModifyValue(box.Bounds.min,
						asset.GameObject.transform.InverseTransformPoint(Handles.PositionHandle(
							asset.GameObject.transform.TransformPoint(box.Bounds.min),
							Quaternion.LookRotation(Vector3.left, Vector3.down))), asset.BoundingBoxSnap);

					box.Bounds.max = handleModifyValue(box.Bounds.max,
						asset.GameObject.transform.InverseTransformPoint(Handles.PositionHandle(
							asset.GameObject.transform.TransformPoint(box.Bounds.max),
							Quaternion.LookRotation(Vector3.forward))), asset.BoundingBoxSnap);
					Handles.Label(asset.GameObject.transform.position + box.Bounds.min, box.Bounds.min.ToString("F2"));
					Handles.Label(asset.GameObject.transform.position + box.Bounds.max, box.Bounds.max.ToString("F2"));
				}
			}
		}

		private Vector3 handleModifyValue(Vector3 value, Vector3 newValue, bool snap)
		{
			if (snap && (newValue - value).magnitude > Mathf.Epsilon)
			{
				if (Mathf.Abs(newValue.x - value.x) > Mathf.Epsilon)
				{
					newValue.x = Mathf.Round(newValue.x * 4) / 4;
				}

				if (Mathf.Abs(newValue.y - value.y) > Mathf.Epsilon)
				{
					newValue.y = Mathf.Round(newValue.y * 4) / 4;
				}

				if (Mathf.Abs(newValue.z - value.z) > Mathf.Epsilon)
				{
					newValue.z = Mathf.Round(newValue.z * 4) / 4;
				}
			}

			return newValue;
		}

	}
}
