using System;
using System.Collections.Generic;
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
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath + "/..", "Mods"));
#endif
            }
        }

           /// <summary>
        /// Gets the path to the game path.
        /// </summary>
        public static string GamePath
        {
            get
            {
#if UNITY_STANDALONE_OSX
				return System.IO.Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/../Library/Application Support/Parkitect/");
#elif UNITY_STANDALONE_WIN
				return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect/", "Mods"));
#else
                return System.IO.Path.GetFullPath("~/Steam/steamapps/common/Parkitect/");
#endif
            }
        }

        public  static readonly string[] DefaultAssemblies =
        {
            "System", "System.Core", "System.Data", "Parkitect", "System.Data", "System.Xml",
        };

        public static readonly string[] SystemAssemblies = {
            "System.ComponentModel.DataAnnotations",
            "System.Configuration",
            "System.Configuration.Install",
            "System.Core",
            "System.Data",
            "System.Design",
            "System.DirectoryServices",
            "System",
            "System.Drawing",
            "System.EnterpriseServices",
            "System.IdentityModel",
            "System.IdentityModel.Selectors",
            "System.Messaging",
            "System.Numerics",
            "System.Runtime.Serialization",
            "System.Runtime.Serialization.Formatters.Soap",
            "System.Security",
            "System.ServiceModel.Activation",
            "System.ServiceModel",
            "System.ServiceModel.Internals",
            "System.Transactions",
            "System.Web.ApplicationServices",
            "System.Web",
            "System.Web.Services",
            "System.Windows.Forms",
            "System.Xml",
        };

        public static readonly string[] ParkitectAssemblies =
        {
            "Accessibility",
            "DOTween43",
            "DOTween46",
            "DOTween50",
            "DOTween",
            "ICSharpCode.SharpZipLib",
            "Mono.Data.Sqlite",
            "Mono.Data.Tds",
            "Mono.Messaging",
            "Mono.Posix",
            "Mono.Security",
            "Mono.WebBrowser",
            "mscorlib",
            "Novell.Directory.Ldap",
            "Parkitect",
            "protobuf-net",
            "System.ComponentModel.DataAnnotations",
            "System.Configuration",
            "System.Configuration.Install",
            "System.Core",
            "System.Data",
            "System.Design",
            "System.DirectoryServices",
            "System",
            "System.Drawing",
            "System.EnterpriseServices",
            "System.IdentityModel",
            "System.IdentityModel.Selectors",
            "System.Messaging",
            "System.Numerics",
            "System.Runtime.Serialization",
            "System.Runtime.Serialization.Formatters.Soap",
            "System.Security",
            "System.ServiceModel.Activation",
            "System.ServiceModel",
            "System.ServiceModel.Internals",
            "System.Transactions",
            "System.Web.ApplicationServices",
            "System.Web",
            "System.Web.Services",
            "System.Windows.Forms",
            "System.Xml",
            "ThirdParty.",
            "ThirdParty",
            "ThirdParty.DynamicDecals",
            "ThirdParty.GraphMaker",
            "ThirdParty.Lutify",
            "ThirdParty.ScreenSpaceCloudShadow",
            "ThirdParty.TiltShift",
            "ThirdParty.UnityUiExtensions",
            "UnityEngine.AccessibilityModule",
            "UnityEngine.AIModule",
            "UnityEngine.Analytics",
            "UnityEngine.AnimationModule",
            "UnityEngine.ARModule",
            "UnityEngine.AssetBundleModule",
            "UnityEngine.AudioModule",
            "UnityEngine.ClothModule",
            "UnityEngine.ClusterInputModule",
            "UnityEngine.ClusterRendererModule",
            "UnityEngine.CoreModule",
            "UnityEngine.CrashLog",
            "UnityEngine.CrashReportingModule",
            "UnityEngine.DirectorModule",
            "UnityEngine",
            "UnityEngine.GameCenterModule",
            "UnityEngine.GridModule",
            "UnityEngine.ImageConversionModule",
            "UnityEngine.IMGUIModule",
            "UnityEngine.InputModule",
            "UnityEngine.JSONSerializeModule",
            "UnityEngine.Networking",
            "UnityEngine.ParticlesLegacyModule",
            "UnityEngine.ParticleSystemModule",
            "UnityEngine.PerformanceReportingModule",
            "UnityEngine.Physics2DModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.ScreenCaptureModule",
            "UnityEngine.SharedInternalsModule",
            "UnityEngine.SpatialTracking",
            "UnityEngine.SpriteMaskModule",
            "UnityEngine.SpriteShapeModule",
            "UnityEngine.StandardEvents",
            "UnityEngine.StyleSheetsModule",
            "UnityEngine.TerrainModule",
            "UnityEngine.TerrainPhysicsModule",
            "UnityEngine.TextRenderingModule",
            "UnityEngine.TilemapModule",
            "UnityEngine.Timeline",
            "UnityEngine.UI",
            "UnityEngine.UIElementsModule",
            "UnityEngine.UIModule",
            "UnityEngine.UNETModule",
            "UnityEngine.UnityAnalyticsModule",
            "UnityEngine.UnityConnectModule",
            "UnityEngine.UnityWebRequestAudioModule",
            "UnityEngine.UnityWebRequestModule",
            "UnityEngine.UnityWebRequestTextureModule",
            "UnityEngine.UnityWebRequestWWWModule",
            "UnityEngine.VehiclesModule",
            "UnityEngine.VideoModule",
            "UnityEngine.VRModule",
            "UnityEngine.WebModule",
            "UnityEngine.WindModule",
            "UnityFbxPrefab",
            "Unity.Postprocessing.Runtime"
        };

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
