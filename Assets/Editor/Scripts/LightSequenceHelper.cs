using System;
using System.Collections.Generic;
using System.Linq;
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
            RideLightMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Resources/Reference Objects/Materials/RideLight.mat", typeof(Material));
            //Debug.Log($"Loaded Material: {RideLightMat}");
        }
        if (Ride_LightMesh == null)
        {
            Ride_LightMesh = Resources.Load<Mesh>("Reference Objects/ride_light");
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

