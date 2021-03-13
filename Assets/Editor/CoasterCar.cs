using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ParkitectAssetEditor
{
    public class CoasterCar
    {
        public string Guid { get; private set; }

        [JsonIgnore]
        public GameObject GameObject
        {
            get { return GameObjectHashMap.Instance.Get(Guid); }
            set { GameObjectHashMap.Instance.Set(Guid, value); }
        }

        public CoasterCarType CoasterCarType;
        public float SeatWaypointOffset = 0.2f;
        public float OffsetBack;
        public float OffsetFront;
        public List<CoasterRestraints> Restraints = new List<CoasterRestraints>();

        // spinning cars
        public float SpinFriction = 0.25f;
        public int SpinStrength = 60;
        public int SpinSymmetrySides = 2;

        // swinging cars
        public float SwingMaxAngle = 90;
        public float SwingFriction = 0.2f;
        public float SwingArmLength = 1.1f;
        public float SwingStrength = 1f;


        public CoasterCar(string guid)
        {
            Guid = guid;
        }
    }

    public enum CoasterCarType
    {
        Normal, Spinning, Swinging
    }
}
