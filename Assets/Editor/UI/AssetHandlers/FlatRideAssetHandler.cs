using ParkitectAssetEditor.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class FlatRideAssetHandler : SeatAssetHandler
    {
        public override void DrawDetailsSection(Asset selectedAsset)
        {
            DrawFlatRideDetailSection(selectedAsset);
            GUILayout.Space(15);
            DrawBoundingBoxDetailSection(selectedAsset);
            GUILayout.Space(15);
            DrawWaypointsDetailSection(selectedAsset);
            GUILayout.Space(15);
            base.DrawDetailsSection(selectedAsset);
        }

        private void DrawFlatRideDetailSection(Asset selectedAsset)
        {
            Animator animator = selectedAsset.GameObject.GetComponent<Animator>();
            if (animator == null)
            {
                EditorGUILayout.HelpBox("This ride has no animator", MessageType.Error);
            }
            else if (animator.runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox("This ride has no animator controller assigned (you can use Assets/Resources/Flat Rides/FlatRideAnimator.controller)", MessageType.Error);
            }

            //shows the rating of the ride
            GUILayout.Label("Rating", EditorStyles.boldLabel);
            selectedAsset.Excitement = EditorGUILayout.Slider("Excitement", selectedAsset.Excitement * 100, 0, 100) / 100f;
            selectedAsset.Intensity = EditorGUILayout.Slider("Intensity", selectedAsset.Intensity * 100, 0, 100) / 100f;
            selectedAsset.Nausea = EditorGUILayout.Slider("Nausea", selectedAsset.Nausea * 100, 0, 100) / 100f;

            //the footprint that the ride covers
            GUILayout.Label("Ride Footprint", EditorStyles.boldLabel);
            selectedAsset.FootprintX = EditorGUILayout.IntField("X", selectedAsset.FootprintX);
            selectedAsset.FootprintZ = EditorGUILayout.IntField("Z", selectedAsset.FootprintZ);

            //category of the ride
            GUILayout.Label("Ride settings", EditorStyles.boldLabel);
            selectedAsset.FlatRideCategory = AttractionType.CategoryTag[
                EditorGUILayout.Popup("Category",
                    Array.IndexOf(AttractionType.CategoryTag, selectedAsset.FlatRideCategory),
                    AttractionType.CategoryDisplayName)];


            selectedAsset.RainProtection = EditorGUILayout.Slider("Rain Protection", selectedAsset.RainProtection * 100, 0, 100) / 100f;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Description");
            selectedAsset.Description = EditorGUILayout.TextArea(selectedAsset.Description, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBoundingBoxDetailSection(Asset _selectedAsset)
        {
            GUILayout.Label("Collisions", "PreToolbar");

            if (_selectedAsset.BoundingBoxes.Count == 0)
            {
                EditorGUILayout.HelpBox("This ride has no collision yet", MessageType.Error);
            }

            Event e = Event.current;

            for (int i = 0; i < _selectedAsset.BoundingBoxes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Color gui = GUI.color;
                if (_selectedAsset.BoundingBoxes[i] == _selectedAsset.SelectedBoundingBox)
                {
                    GUI.color = Color.red;
                }

                if (GUILayout.Button("BoundingBox" + (i + 1)))
                {
                    if (_selectedAsset.SelectedBoundingBox == _selectedAsset.BoundingBoxes[i])
                    {
                        _selectedAsset.SelectedBoundingBox = null;
                        return;
                    }
                    _selectedAsset.SelectedBoundingBox = _selectedAsset.BoundingBoxes[i];
                }
                GUI.color = gui;
                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                {
                    _selectedAsset.BoundingBoxes.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (_selectedAsset.SelectedBoundingBox != null)
            {
                GUILayout.Label("Hold S - Snap to 0.25");
            }

            if (GUILayout.Button("Add bounding box"))
            {
                BoundingBox boundingBox = new BoundingBox()
                {
                    Bounds = new Bounds(new Vector3(0, 0.5f, 0), Vector3.one)
                };

                _selectedAsset.BoundingBoxes.Add(boundingBox);
            }
        }


        private void DrawWaypointsDetailSection(Asset _selectedAsset)
        {
            //waypoint tool for NPC pathing
            GUILayout.Label("Waypoints", "PreToolbar");

            if (_selectedAsset.Waypoints.Count == 0)
            {
                EditorGUILayout.HelpBox("This ride has no waypoints yet", MessageType.Error);
            }

            _selectedAsset.EnableWaypointEditing =
                GUILayout.Toggle(_selectedAsset.EnableWaypointEditing, "Enable Editing Waypoints", "Button");

            if (_selectedAsset.EnableWaypointEditing)
            {
                GUILayout.Label("Ctrl - snap to plane height");
                _selectedAsset.HelperPlaneY = EditorGUILayout.FloatField("Helper Plane Y", _selectedAsset.HelperPlaneY);

                //generates an initial gride of waypoints around the outer squares
                if (GUILayout.Button("Generate outer grid"))
                {

                    float minX = -_selectedAsset.FootprintX / 2;
                    float maxX = _selectedAsset.FootprintX / 2;
                    float minZ = -_selectedAsset.FootprintZ / 2;
                    float maxZ = _selectedAsset.FootprintZ / 2;
                    for (int xi = 0; xi < Mathf.RoundToInt(maxX - minX); xi++)
                    {
                        for (int zi = 0; zi < Mathf.RoundToInt(maxZ - minZ); zi++)
                        {
                            float x = minX + xi;
                            float z = minZ + zi;
                            if (!(x == minX || x == maxX - 1) && !(z == minZ || z == maxZ - 1))
                            {
                                continue;
                            }

                            Waypoint newWaypoint = new Waypoint
                            {
                                Position = new Vector3(x + 0.5f, 0, z + 0.5f),
                                IsOuter = true
                            };

                            _selectedAsset.Waypoints.Add(newWaypoint);
                        }
                    }
                }

                //adds a waypoint at (0,0,0) relative to the unity object
                if (GUILayout.Button("Add Waypoint"))
                {
                    Waypoint.addWaypoint(_selectedAsset, _selectedAsset.GameObject.transform.position);
                }

                //clears all the waypoint
                if (GUILayout.Button("Clear all"))
                {
                    _selectedAsset.Waypoints.Clear();
                    _selectedAsset.SelectedWaypoint = null;
                }

                //provides a list of all the waypoints
                for (int i = 0; i < _selectedAsset.Waypoints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("#" + i);
                    _selectedAsset.Waypoints[i].IsOuter =
                        GUILayout.Toggle(_selectedAsset.Waypoints[i].IsOuter, "IsOuter", "Button");
                    _selectedAsset.Waypoints[i].IsRabbitHoleGoal =
                        GUILayout.Toggle(_selectedAsset.Waypoints[i].IsRabbitHoleGoal, "IsRabbitHoleGoal", "Button");
                    if (GUILayout.Button("Delete"))
                    {
                        Waypoint.DeletePoint(_selectedAsset, _selectedAsset.SelectedWaypoint);
                    }

                    EditorGUILayout.EndHorizontal();

                    _selectedAsset.Waypoints[i].Position =
                        EditorGUILayout.Vector3Field("Position", _selectedAsset.Waypoints[i].Position);
                }
            }
        }
    }
}
