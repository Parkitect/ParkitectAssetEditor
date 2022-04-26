using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.Utility
{
    /// <summary>
    /// Helper methods/properties
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets the path to the parkitect mod folder.
        /// </summary>
        public static string ParkitectModPath
        {
            get
            {
#if UNITY_STANDALONE_OSX
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/../Library/Application Support/Parkitect/", "Mods"));
#elif UNITY_STANDALONE_WIN
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect/", "Mods"));
#else
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath + "/..", "Mods"));
#endif
            }
        }


        private static Dictionary<SittingType, Mesh> npcMeshes = new Dictionary<SittingType,Mesh>();
        private static Material sceneViewMaterial;

        public static void renderSeatGizmo(GameObject gameObject, SittingType sittingType)
        {
            if (!npcMeshes.TryGetValue(sittingType, out Mesh npcMesh))
            {
                switch (sittingType)
                {
                    case SittingType.Bench:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_bench");
                        break;
                    case SittingType.Car:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_kart");
                        break;
                    case SittingType.Flat:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_flat");
                        break;
                    case SittingType.Horse:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_horse");
                        break;
                    case SittingType.Pedaling:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_pedaling");
                        break;
                    case SittingType.Rowing:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting");
                        break;
                    case SittingType.MonorailCoaster:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_monorailcoaster");
                        break;
                    case SittingType.NormalRide:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting");
                        break;
                    case SittingType.StandingNormal:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest");
                        break;
                    case SittingType.SteepleChaseFront:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_steeple_front");
                        break;
                    case SittingType.SteepleChaseBack:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting_steeple_front");
                        break;
                    default:
                        npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting");
                        break;
                }
                
                npcMeshes.Add(sittingType, npcMesh);
            }

            if (sceneViewMaterial == null)
            {
                sceneViewMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Editor/SceneViewGhostMaterial.mat", typeof(Material));
            }

            var seats = gameObject.
                GetComponentsInChildren<Transform>(true).
                Where(transform => transform.name.StartsWith("Seat", true, System.Globalization.CultureInfo.InvariantCulture));

            foreach (var seat in seats)
            {
                sceneViewMaterial.SetPass(0);
                Graphics.DrawMeshNow(npcMesh, seat.position, seat.rotation);
                sceneViewMaterial.SetPass(1);
                Graphics.DrawMeshNow(npcMesh, seat.position, seat.rotation);
            }
        }
    }
}
