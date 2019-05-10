// source: https://forum.unity.com/threads/editor-raycast-against-scene-meshes-without-collider-editor-select-object-using-gui-coordinate.485502/
//powerful class, allows to detect intersection with mesh, without requiring any collider, etc
//Works in editor only
//
// Main Author https://gist.github.com/MattRix
// Igor Aherne improved it to include object picking as well   facebook.com/igor.aherne
//https://github.com/MattRix/UnityDecompiled/blob/master/UnityEditor/UnityEditor/HandleUtility.cs
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorHandles_UnityInternal {
    static Type type_HandleUtility;
    static MethodInfo meth_IntersectRayMesh;
    static MethodInfo meth_PickObjectMeth;
 
    static EditorHandles_UnityInternal() {
        var editorTypes = typeof(Editor).Assembly.GetTypes();
 
        type_HandleUtility = editorTypes.FirstOrDefault(t => t.Name == "HandleUtility");
        meth_IntersectRayMesh = type_HandleUtility.GetMethod("IntersectRayMesh",
                                                              BindingFlags.Static | BindingFlags.NonPublic);
        meth_PickObjectMeth = type_HandleUtility.GetMethod("PickGameObject",
                                                            BindingFlags.Static | BindingFlags.Public,
                                                            null,
                                                            new [] {typeof(Vector2), typeof(bool)},
                                                            null);
    }
 
 
    //get a point from interected with any meshes in scene, based on mouse position.
    //WE DON'T NOT NEED to have to have colliders ;)
    //usually used in conjunction with  PickGameObject()
    public static bool IntersectRayMesh(Ray ray, MeshFilter meshFilter, out RaycastHit hit) {
        return IntersectRayMesh(ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, out hit);
    }
 
    //get a point from interected with any meshes in scene, based on mouse position.
    //WE DON'T NOT NEED to have to have colliders ;)
    //usually used in conjunction with  PickGameObject()
    public static bool IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit) {
        var parameters = new object[] { ray, mesh, matrix, null };
        bool result = (bool)meth_IntersectRayMesh.Invoke(null, parameters);
        hit = (RaycastHit)parameters[3];
        return result;
    }
 
    public static bool IntersectRayGameObject(Ray ray, GameObject gameObject, out SceneRaycastHit hit) {
        hit = new SceneRaycastHit();
        float nearestHitDistance = float.MaxValue;
        bool hitSomething = false;
        foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>()) {
            RaycastHit meshFilterHit = new RaycastHit();
            if (IntersectRayMesh(ray, meshFilter, out meshFilterHit) && meshFilterHit.distance < nearestHitDistance) {
                nearestHitDistance = meshFilterHit.distance;
                hit.point = meshFilterHit.point;
                hit.normal = meshFilterHit.normal;

                Vector3[] vertices = meshFilter.sharedMesh.vertices;
                int[] triangles = meshFilter.sharedMesh.triangles;
                Vector3 p0 = vertices[triangles[meshFilterHit.triangleIndex * 3 + 0]];
                Vector3 p1 = vertices[triangles[meshFilterHit.triangleIndex * 3 + 1]];
                Vector3 p2 = vertices[triangles[meshFilterHit.triangleIndex * 3 + 2]];
                Transform hitTransform = meshFilter.transform;
                p0 = hitTransform.TransformPoint(p0);
                p1 = hitTransform.TransformPoint(p1);
                p2 = hitTransform.TransformPoint(p2);

                // unity seems to return the interpolated smoothened normal instead of the actual triangle geometry one that we want, so we calculate it ourselves
                hit.normal = Vector3.Cross(p0 - p1, p1 - p2).normalized;

                hit.transform = meshFilter.transform;
                hitSomething = true;
            }
        }

        return hitSomething;
    }
 
//select a gameObject in scene, based on mouse position.
    //Object DOES NOT NEED to have to have colliders ;)
    //If you DON'T want object to be included into  Selection.activeGameObject,
    //(parameter works only in gui functions and scene view delegates) specify updateSelection = false
    public static GameObject PickGameObject(Vector2 position, bool updateSelection = true, bool selectPrefabRoot = false) {
 
        if (updateSelection == false && Event.current != null) {
            int blocking_ix = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(blocking_ix);
            GUIUtility.hotControl = blocking_ix; //tell unity that your control is active now, so it won't do selections etc.
        }
 
        GameObject pickedGameObject = (GameObject)meth_PickObjectMeth.Invoke(null,
                                                       new object[] { position, selectPrefabRoot });
 
        return pickedGameObject;
    }
 
    public struct SceneRaycastHit {
        public Vector3 point;
        public Vector3 normal;
        public Transform transform;
    }
}