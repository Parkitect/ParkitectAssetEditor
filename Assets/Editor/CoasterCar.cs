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

        public float SeatWaypointOffset = 0.2f;
        public float OffsetBack;
        public float OffsetFront;
		public List<CoasterRestraints> Restraints = new List<CoasterRestraints>();

		public CoasterCar(string guid)
		{
			Guid = guid;
		}
	}
}
