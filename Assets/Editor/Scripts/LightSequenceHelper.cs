using UnityEditor;
using UnityEngine;

public static class LightSequenceHelper    // Small AssetManager
{
    public static Mesh Ride_LightMesh;
    public static Material RideLightMat;

    public static void LoadRideLightM()
    {
        if (RideLightMat == null)
        {
            RideLightMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Editor/RideLight.mat", typeof(Material));
            //Debug.Log($"Loaded Material: {RideLightMat}");
        }
        if (Ride_LightMesh == null)
        {
            Ride_LightMesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Editor/ride_light.fbx", typeof(Mesh));
            //Debug.Log($"Loaded Ride_Light Mesh: {Ride_LightMesh}");
        }
    }
    public static void renderLightS(GameObject gameObject)  //Renders only one Ride_Light on a GameObject
    {
        LoadRideLightM();

        RideLightMat.SetPass(0);
        Graphics.DrawMeshNow(Ride_LightMesh, gameObject.transform.position, gameObject.transform.rotation);
        RideLightMat.SetPass(1);
        Graphics.DrawMeshNow(Ride_LightMesh, gameObject.transform.position, gameObject.transform.rotation);
    }

    public static Transform recursiveFindTransform(Transform transform, string childname)
    {
        foreach (Transform componentsInChild in transform.GetComponentsInChildren<Transform>(true))
        {
            if (componentsInChild.gameObject.name.Equals(childname))
                return componentsInChild;
        }
        return (Transform)null;
    }
}

