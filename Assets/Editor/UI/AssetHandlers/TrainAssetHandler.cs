using System;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class TrainAssetHandler : SeatAssetHandler
    {
        private static string[] trackedRideNames = new[]
        {
            "Alpine Coaster",
            "Boat Dark Ride",
            "Boat Transport",
            "Calm River Ride",
            "Car Ride",
            "Floorless Coaster",
            "Flying Coaster",
            "Gentle Monorail Ride",
            "Ghost Mansion Ride",
            "Giga Coaster",
            "Hydraulically-Launched Coaster",
            "Hyper Coaster",
            "Inverted Coaster",
            "Inverted Dark Ride",
            "Inverting Spinning Coaster",
            "Inverting Wooden Coaster",
            "Junior Coaster",
            "Log Flume",
            "Mine Train Coaster",
            "Miniature Railway",
            "Mini Coaster",
            "Mini Monorail",
            "Monorail",
            "Monorail Coaster",
            "Pivot Coaster",
            "Powered Coaster",
            "Spinning Coaster",
            "Splash Battle",
            "Stand-up Coaster",
            "Steel Coaster",
            "Steeplechase",
            "Submarines",
            "Suspended Coaster",
            "Suspended Monorail",
            "Tilt Coaster",
            "Vertical Drop Coaster",
            "Water Coaster",
            "Wild Mouse",
            "Wing Coaster",
            "Wooden Coaster",
        };

        public override void DrawDetailsSection(Asset _selectedAsset)
        {
            if (_selectedAsset.GameObject == null)
            {
                return;
            }

            GUILayout.Label("Train settings:", EditorStyles.boldLabel);

            if (_selectedAsset.GameObject.transform.Find("backAxis") == null)
            {
                EditorGUILayout.HelpBox("There is no backAxis marker!", MessageType.Error);
            }

            int trackedRideNameIndex = EditorGUILayout.Popup("Ride", Array.IndexOf(trackedRideNames, _selectedAsset.TrackedRideName), trackedRideNames);
            if (trackedRideNameIndex >= 0 && trackedRideNameIndex < trackedRideNames.Length)
            {
                _selectedAsset.TrackedRideName = trackedRideNames[trackedRideNameIndex];
            }

            _selectedAsset.DefaultTrainLength = EditorGUILayout.IntSlider("Default train length: ", _selectedAsset.DefaultTrainLength, 1, 12);
            _selectedAsset.MinTrainLength = EditorGUILayout.IntSlider("Minimum train length: ", _selectedAsset.MinTrainLength, 1, 12);
            _selectedAsset.MaxTrainLength = EditorGUILayout.IntSlider("Maximum train length: ", _selectedAsset.MaxTrainLength, 1, 12);

            GUILayout.Space(15);

            if (_selectedAsset.LeadCar == null)
            {
                CoasterCar car = new CoasterCar(_selectedAsset.Guid + ".leadCar")
                {
                    GameObject = _selectedAsset.GameObject
                };

                _selectedAsset.LeadCar = car;
            }
            GUILayout.Label("Lead Car:", EditorStyles.boldLabel);
            DrawCarDetailSection(_selectedAsset.LeadCar);

            GUILayout.Space(30);

            if (_selectedAsset.Car == null)
            {
                CoasterCar car = new CoasterCar(_selectedAsset.Guid + ".car");
                _selectedAsset.Car = car;
            }

            if (_selectedAsset.Car.GameObject == _selectedAsset.LeadCar.GameObject)
            {
                _selectedAsset.Car.GameObject = null;
            }

            GUILayout.Label("Normal Car:", EditorStyles.boldLabel);
            DrawCarDetailSection(_selectedAsset.Car);

            GUILayout.Space(30);

            if (_selectedAsset.RearCar == null)
            {
                CoasterCar car = new CoasterCar(_selectedAsset.Guid + ".rearCar");
                _selectedAsset.RearCar = car;
            }

            if (_selectedAsset.RearCar.GameObject == _selectedAsset.LeadCar.GameObject || _selectedAsset.RearCar.GameObject == _selectedAsset.Car.GameObject)
            {
                _selectedAsset.RearCar.GameObject = null;
            }

            GUILayout.Label("Rear Car:", EditorStyles.boldLabel);
            DrawCarDetailSection(_selectedAsset.RearCar);
        }

        /// <summary>
        /// Adds a new train type to the list of trains.
        /// </summary>
        /// <param name="name">Name of the type to add.</param>
        public static void AddRideType(string name)
            => ArrayUtility.Add(ref trackedRideNames, name);

        private void DrawCarDetailSection(CoasterCar car)
        {
            var newAsset = EditorGUILayout.ObjectField("Drop to add:", car.GameObject, typeof(GameObject), true) as GameObject;
            if (newAsset != null && newAsset != car.GameObject && newAsset.scene.name != null) // scene name is null for prefabs, yay for unity for checking it this way
            {
                car.GameObject = newAsset;
            }

            car.CoasterCarType = (CoasterCarType)EditorGUILayout.EnumPopup("Type:", car.CoasterCarType);
            if (car.CoasterCarType == CoasterCarType.Spinning)
            {
                car.SpinFriction = EditorGUILayout.Slider("Spin friction:", car.SpinFriction, 0f, 1f);
                car.SpinStrength = Mathf.RoundToInt(EditorGUILayout.Slider("Spin strength:", car.SpinStrength / 100f, 0f, 1f) * 100);
                car.SpinSymmetrySides = EditorGUILayout.IntField("Spin symmetry sides:", car.SpinSymmetrySides);
            }
            else if (car.CoasterCarType == CoasterCarType.Swinging)
            {
                car.SwingFriction = EditorGUILayout.Slider("Swing friction:", car.SwingFriction, 0f, 1f);
                car.SwingStrength = EditorGUILayout.Slider("Swing strength:", car.SwingStrength, 0f, 1f);
                car.SwingMaxAngle = EditorGUILayout.FloatField("Max swing angle:", car.SwingMaxAngle);
                car.SwingArmLength = EditorGUILayout.FloatField("Swing arm length:", car.SwingArmLength);
            }

            car.SeatWaypointOffset = EditorGUILayout.FloatField("Seat waypoint offset:", car.SeatWaypointOffset);
            car.OffsetFront = EditorGUILayout.FloatField("Offset front:", car.OffsetFront);
            car.OffsetBack = EditorGUILayout.FloatField("Offset back:", car.OffsetBack);

            base.Draw(car.GameObject);

            GUILayout.Space(15);

            GUILayout.Label("Restraints", "PreToolbar");
            for (int i = car.Restraints.Count - 1; i >= 0; i--)
            {
                CoasterRestraints restraints = car.Restraints[i];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("#" + i);
                if (GUILayout.Button("Delete"))
                {
                    car.Restraints.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();

                restraints.TransformName = EditorGUILayout.TextField("Transform name", restraints.TransformName);
                restraints.ClosedAngle = EditorGUILayout.FloatField("Closed angle (X-Axis)", restraints.ClosedAngle);
            }

            if (GUILayout.Button("Add"))
            {
                CoasterRestraints restraints = new CoasterRestraints()
                {
                    TransformName = "restraint"
                };

                car.Restraints.Add(restraints);
            }
        }
    }
}
