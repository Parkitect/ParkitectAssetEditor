using Newtonsoft.Json;
using UnityEngine;

namespace ParkitectAssetEditor.Utility
{
    public class BoundingBox
    {
        [JsonIgnore]
        public Bounds Bounds;

        
        [JsonProperty("BoundsMin")]
        public float[] SerializedMin
        {
            get { return new float[3] {Bounds.min.x, Bounds.min.y, Bounds.min.z}; }
            set { Bounds.min = new Vector3(value[0], value[1], value[2]); }
        }
        
        [JsonProperty("BoundsMax")]
        public float[] SerializedMax
        {
            get { return new float[3] {Bounds.max.x, Bounds.max.y, Bounds.max.z}; }
            set { Bounds.max = new Vector3(value[0], value[1], value[2]); }
        }
    }
}