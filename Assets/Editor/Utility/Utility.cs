using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.Utility {
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
                return Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),".steam/steam/steamapps/common/Parkitect/Mods"));
#endif
            }
        }

        
		private static Mesh npcMesh;
		private static Material sceneViewMaterial;

        public static void renderSeatGizmo(GameObject gameObject) {
            if (npcMesh == null)
			{
				npcMesh = Resources.Load<Mesh>("Reference Objects/reference_guest_sitting");
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
