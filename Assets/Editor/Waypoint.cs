using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor
{
    public class Waypoint
    {
        
        [JsonIgnore]
        public Vector3 Position;

        public bool IsOuter;

        public bool IsRabbitHoleGoal;

        public List<int> ConnectedTo = new List<int>();


        [JsonProperty("Position")]
        public float[] SerializedPosition
        {
            get { return new float[3] {Position.x, Position.y, Position.z}; }
            set { Position = new Vector3(value[0], value[1], value[2]); }
        }

        public static void DeletePoint(Asset asset,Waypoint SelectedWaypoint)
        {
            int selectedWaypointIndex = asset.Waypoints.FindIndex(delegate(Waypoint wp) { return wp == SelectedWaypoint; });
            foreach (Waypoint waypoint in asset.Waypoints)
            {
                waypoint.ConnectedTo.Remove(selectedWaypointIndex);
            }

            asset.Waypoints.Remove(SelectedWaypoint);

            foreach (Waypoint waypoint in  asset.Waypoints)
            {
                for (int i = 0; i < waypoint.ConnectedTo.Count; i++)
                {
                    if (waypoint.ConnectedTo[i] > selectedWaypointIndex)
                    {
                        waypoint.ConnectedTo[i]--;
                    }
                }
            }

            asset.SelectedWaypoint = null;
        }
        
        public static Waypoint addWaypoint(Asset asset,Vector3 position)
        {
            Waypoint waypoint = new Waypoint();
            waypoint.Position = asset.GameObject.transform.InverseTransformPoint(position);
            asset.Waypoints.Add(waypoint);
            return waypoint;
        }
    }
}