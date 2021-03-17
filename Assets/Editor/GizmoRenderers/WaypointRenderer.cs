using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ParkitectAssetEditor.GizmoRenderers
{
    public class WaypointRenderer : IGizmoRenderer, IHandleRenderer
    {
        public enum WaypointState
        {
            NONE,
            CONNECT
        }

        private bool _snapToPlane = false;

        public bool CanRender(Asset asset)
        {
            return asset.Type == AssetType.FlatRide;
        }

        public void Render(Asset asset)
        {

        }

        public void Handle(Asset asset)
        {

            if (asset.EnableWaypointEditing)
            {
                if (_snapToPlane && asset.SelectedWaypoint != null)
                {
                    asset.SelectedWaypoint.Position.y = asset.HelperPlaneY;
                }

                switch (Event.current.type)
                {
                    case EventType.Layout:
                        break;
                    case EventType.KeyDown:
                        if (Event.current.keyCode == KeyCode.LeftControl)
                        {
                            _snapToPlane = true;
                        }

                        break;
                    case EventType.KeyUp:
                        if (Event.current.keyCode == KeyCode.C)
                        {
                            if (asset.WaypointState != WaypointState.CONNECT)
                            {
                                asset.WaypointState = WaypointState.CONNECT;
                            }
                            else
                            {
                                asset.WaypointState = WaypointState.NONE;
                            }
                        }

                        if (Event.current.keyCode == KeyCode.R && asset.SelectedWaypoint != null)
                        {
                            if (asset.SelectedWaypoint == null)
                            {
                                break;
                            }

                            Waypoint.DeletePoint(asset, asset.SelectedWaypoint);
                        }

                        if (Event.current.keyCode == KeyCode.A)
                        {
                            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                            Vector3 hitPosition = Vector3.zero;
                            EditorHandles_UnityInternal.SceneRaycastHit hit;
                            if (EditorHandles_UnityInternal.IntersectRayGameObject(ray, asset.GameObject, out hit))
                            {
                                hitPosition = hit.point;
                            }
                            else
                            {
                                Plane plane = new Plane(Vector3.up, new Vector3(0, asset.HelperPlaneY, 0));
                                float enter = 0;
                                plane.Raycast(ray, out enter);
                                hitPosition = ray.GetPoint(enter);
                            }

                            asset.SelectedWaypoint = Waypoint.addWaypoint(asset, hitPosition);
                        }

                        if (Event.current.keyCode == KeyCode.O && asset.SelectedWaypoint != null)
                        {
                            asset.SelectedWaypoint.IsOuter = !asset.SelectedWaypoint.IsOuter;
                        }

                        if (Event.current.keyCode == KeyCode.I && asset.SelectedWaypoint != null)
                        {
                            asset.SelectedWaypoint.IsRabbitHoleGoal = !asset.SelectedWaypoint.IsRabbitHoleGoal;
                        }

                        if (Event.current.keyCode == KeyCode.LeftControl)
                        {
                            _snapToPlane = false;
                        }

                        SceneView.RepaintAll();
                        HandleUtility.Repaint();
                        break;
                }

                //render helper plane
                Vector3 topLeft = new Vector3(-((float)asset.FootprintX) / 2.0f, 0, (float)asset.FootprintZ / 2.0f) +
                                  asset.GameObject.transform.position + new Vector3(0, asset.HelperPlaneY, 0);
                Vector3 topRight = new Vector3(((float)asset.FootprintX) / 2.0f, 0, (float)asset.FootprintZ / 2.0f) +
                                   asset.GameObject.transform.position + new Vector3(0, asset.HelperPlaneY, 0);
                Vector3 bottomLeft =
                    new Vector3(-((float)asset.FootprintX) / 2.0f, 0, -(float)asset.FootprintZ / 2.0f) +
                    asset.GameObject.transform.position + new Vector3(0, asset.HelperPlaneY, 0);
                Vector3 bottomRight =
                    new Vector3(((float)asset.FootprintX) / 2.0f, 0, -(float)asset.FootprintZ / 2.0f) +
                    asset.GameObject.transform.position + new Vector3(0, asset.HelperPlaneY, 0);

                Color fill = Color.white;
                fill.a = 0.1f;
                Handles.zTest = CompareFunction.LessEqual;
                Handles.color = Color.yellow;
                Handles.DrawSolidRectangleWithOutline(new[] { topLeft, topRight, bottomRight, bottomLeft }, fill,
                    Color.black);
                Handles.zTest = CompareFunction.Always;
            }

            for (int x = 0; x < asset.Waypoints.Count; x++)
            {
                if (asset.EnableWaypointEditing && asset.Waypoints[x] == asset.SelectedWaypoint)
                {
                    Handles.color = Color.red;
                }
                else if (asset.Waypoints[x].IsOuter)
                {
                    Handles.color = Color.green;
                }
                else if (asset.Waypoints[x].IsRabbitHoleGoal)
                {
                    Handles.color = Color.blue;
                }
                else
                {
                    Handles.color = Color.yellow;
                }

                Vector3 worldPos = asset.Waypoints[x].Position + asset.GameObject.transform.position;

                Handles.zTest = CompareFunction.LessEqual;

                if (Handles.Button(worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.2f,
                    HandleUtility.GetHandleSize(worldPos) * 0.2f, Handles.SphereHandleCap))
                {
                    if (asset.EnableWaypointEditing)
                    {
                        handleClick(asset, asset.Waypoints[x]);
                    }
                }

                Handles.color = Color.blue;
                foreach (int connectedIndex in asset.Waypoints[x].ConnectedTo)
                {
                    Handles.DrawLine(worldPos,
                        asset.Waypoints[connectedIndex].Position + asset.GameObject.transform.position);
                }
                Handles.color = Color.white;
                Handles.Label(worldPos, "#" + x);

                Handles.zTest = CompareFunction.Always;
            }

            if (asset.EnableWaypointEditing && asset.SelectedWaypoint != null)
            {
                Vector3 worldPos = asset.SelectedWaypoint.Position + asset.GameObject.transform.position;

                if (asset.WaypointState == WaypointState.CONNECT)
                {
                    Handles.Label(worldPos, "\nConnecting...");
                }
                else
                {
                    asset.SelectedWaypoint.Position = asset.GameObject.transform.InverseTransformPoint(
                        Handles.PositionHandle(
                            asset.GameObject.transform.TransformPoint(asset.SelectedWaypoint.Position),
                            Quaternion.identity));
                    Handles.Label(worldPos, "\n(A)dd\n(C)onnect\n(R)emove\n(O)uter\nRabb(i)t Hole");
                }
            }
        }

        private void handleClick(Asset asset, Waypoint waypoint)
        {
            if (asset.WaypointState == WaypointState.NONE && waypoint != null)
            {
                asset.SelectedWaypoint = waypoint;
            }
            else if (asset.WaypointState == WaypointState.CONNECT && waypoint != null)
            {
                int closestWaypointIndex = asset.Waypoints.FindIndex(delegate (Waypoint wp) { return wp == waypoint; });
                int selectedWaypointIndex = asset.Waypoints.FindIndex(delegate (Waypoint wp)
                {
                    return wp == asset.SelectedWaypoint;
                });
                if (closestWaypointIndex >= 0 && selectedWaypointIndex >= 0)
                {
                    if (!asset.SelectedWaypoint.ConnectedTo.Contains(closestWaypointIndex))
                    {
                        asset.SelectedWaypoint.ConnectedTo.Add(closestWaypointIndex);
                        waypoint.ConnectedTo.Add(selectedWaypointIndex);
                    }
                    else
                    {
                        asset.SelectedWaypoint.ConnectedTo.Remove(closestWaypointIndex);
                        waypoint.ConnectedTo.Remove(selectedWaypointIndex);
                    }
                }
            }
        }

    }
}