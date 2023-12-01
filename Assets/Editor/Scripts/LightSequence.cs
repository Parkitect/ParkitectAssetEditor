using Newtonsoft.Json;
using ParkitectAssetEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using static EditorHandles_UnityInternal;

public class LightSequence : ScriptableObject, ILightEffect, ISerializationCallbackReceiver // ScriptableObject Instead of MonoBehaviour, so it can be located in "Editor" Folder
{
    /*/////////////////////////////// ASSET DATA //////////////////////////////////////*/
    [JsonIgnore]
    public Asset Asset { get; private set; }
    [JsonIgnore]
    public GameObject MainGameObject { get; private set; }

    private bool enabled = true;    //Pseudo Behaviour enabled

    [JsonIgnore] 
    public Color[] customColors;   // = Asset.LS_Colors

    //[HideInInspector]   //For Debug not
    public List<LightSequenceLight> lights = new List<LightSequenceLight>();    // = Asset.LS_Lights   //Light, just the Objects
    //[HideInInspector]
    public List<LightSequenceGroup> groups = new List<LightSequenceGroup>();    // = Asset.LS_Groups   //LightGroup, controls several Lights with Effect sequences in a timeline
    //[HideInInspector]
    public List<LightSequenceLoop> loops = new List<LightSequenceLoop>();       // = Asset.LS_Loops    //Loops, has the LightGroups in it

    /*////////////////////////////////////////////////////////////////////////////////*/

    private MaterialPropertyBlock materialPropertyBlock;
    private LightInstancingData[] instancingData;
    private int instancingDataBufferNameID;
    private LightEffectGroupData[] effectGroupData;
    private int effectGroupDataBufferNameID;
    private LightParentData[] parentData;
    private int parentDataBufferNameID;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private ComputeBuffer instancingDataBuffer;
    private ComputeBuffer effectGroupDataBuffer;
    private ComputeBuffer parentDataBuffer;
    private Bounds bounds;

    private List<LightSequenceParent> lightSequenceParents = new List<LightSequenceParent>();
    private Dictionary<Transform, LightSequenceParent> transformParentAssoc = new Dictionary<Transform, LightSequenceParent>();
    private Dictionary<LightSequenceLight, int> lightIndexAssoc = new Dictionary<LightSequenceLight, int>();

    public struct LightInstancingData
    {
        public Matrix4x4 localToWorld;
        public int parentIndex;
        public int groupIndex;
        public float twinkleIntensity;
    }

    public struct LightEffectGroupData
    {
        public Vector4 _Color;
        public float _Intensity;
        public int twinkle;
    }

    public struct LightParentData
    {
        public Matrix4x4 localToWorld;
    }

    private bool isEnabled = true;


    public void SetAsset(Asset asset)
    {
        Asset = asset;
    }

    public void SetGameObject(GameObject gameObject)
    {
        MainGameObject = gameObject;
    }

    public void InitiateAssetData(Asset asset, GameObject gameObject)
    {
        SetAsset(asset);
        SetGameObject(gameObject);
        customColors = asset.LS_Colors;
        lights = asset.LS_Lights;
        foreach (var light in lights) { light.setParent(this); }
        loops = asset.LS_Loops;
        groups = asset.LS_Groups;

        //OnBeforeSerialize();      // Before Saving the AssetPack to .assetProject
        OnAfterDeserialize();       // After Loading the AssetPack from .assetProject

        initialize();
        reset();

        bounds = new Bounds(MainGameObject.transform.position, new Vector3(100.0f, 100.0f, 100.0f));

        Debug.Log($"Initiated LightSqeuence Asset Data for Asset: {asset.Name} - GUID: {asset.Guid}");

    }


    void Awake()    //Awakes if CreatedInstance in Asset with Call Asset.AddLightSequence(), which gets called by Open LSEW
    {
        //initialize(); // Moved to InitiateAssetData()
        //reset();
        updateEnabledState();   //Actually NN

        /*  No need in Unity Editor
        if (GameController.Instance != null)
        {
            GameController.Instance.park.addLightSequence(this);
            GameController.Instance.park.OnLightsOnChanged += onLightsOnChanged;
        }
        */
    }

    /*void Start()    // Not an UnityEngine Invoke in ScriptableObject
    {
        updateEnabledState();

        bounds = new Bounds(transform.position, new Vector3(100.0f, 100.0f, 100.0f));

        Debug.Log("LightSequence Start was executed!");
    }
    */

    /*void OnValidate() {
        if (loops.Count == 0) {
            loops.Clear();
            LightSequenceLoop loop = new LightSequenceLoop();
            foreach (LightSequenceGroup group in groups) {
                loop.groups.Add(group);
            }
            loops.Add(loop);

            UnityEditor.EditorUtility.SetDirty(this);
        }
    }*/

    void OnDestroy()
    {
        argsBuffer?.Release();
        instancingDataBuffer?.Release();
        effectGroupDataBuffer?.Release();
        parentDataBuffer?.Release();

        /*  //No need in Unity Editor
        if (GameController.Instance == null || GameController.Instance.isQuittingGame)
        {
            return;
        }

        GameController.Instance.park.removeLightSequence(this);
        GameController.Instance.park.OnLightsOnChanged -= onLightsOnChanged;
        */
    }

    public void setCustomColors(Color[] newColors)
    {
        if (newColors != null)
        {
            if (customColors == null || newColors.Length > customColors.Length)
            {
                customColors = new Color[newColors.Length];
            }

            for (int i = 0; i < newColors.Length; i++)
            {
                newColors[i].a = customColors[i].a;
                customColors[i] = newColors[i];
            }
        }

        invalidate();
    }

    private void onLightsOnChanged(bool lightsAreOn)
    {
        updateEnabledState();
    }

    public bool getEnabled()
    {
        return isEnabled;
    }

    public void setEnabled(bool value)
    {
        isEnabled = value;
        updateEnabledState();
    }

    private void updateEnabledState()   //Behaviour.enabled: Behaviours are updated - NN
    {
        enabled = isEnabled; //No Need in Unity Edior: && GameController.Instance != null && GameController.Instance.park.lightsShouldBeOn();
    }

    public void Update()    //LateUpdate or Update - GET NEVER CALLED IN SCRIPTBALE OBJECT - NN, is only for Runtime
    {
        if (true) //Global.RIDE_LIGHTS_ENABLED = true;
        {
            //Debug.Log("LightSequence Update gets executed!");
            tick(Time.deltaTime);
            draw(null);
        }
    }

    public void RotateLightAroundAxis(LightSequenceLight light, Vector3 pointOfRotation, Vector3 rotationAxis, float angleOfRotation, GameObject objects)
    {
        light.RotateMatrixAroundAxis(this, pointOfRotation, rotationAxis, angleOfRotation, objects);

        // invalidate();
    }

    public LightSequenceLight addLight(Transform parent, Vector3 position, Vector3 normal)
    {
        LightSequenceLight lightSequenceLight = new LightSequenceLight();
        lightSequenceLight.setPosition(parent, position, normal);
        lights.Add(lightSequenceLight);
        invalidate();

        return lightSequenceLight;
    }

    public LightSequenceLight addDuplicateLight(Transform parent, Matrix4x4 transform, Vector3 normal)
    {
        LightSequenceLight lightSequenceLight = new LightSequenceLight();
        lightSequenceLight.parent = parent;
        lightSequenceLight.transform = transform;
        lightSequenceLight.normal = normal;
        lights.Add(lightSequenceLight);

        return lightSequenceLight;
    }

    public void removeLight(LightSequenceLight light)
    {
        lights.Remove(light);
        foreach (LightSequenceGroup group in groups)
        {
            group.lights.Remove(light);
        }
        invalidate();
    }

    public void removeLoop(LightSequenceLoop loop)
    {
        for (int i = loop.groups.Count - 1; i >= 0; i--)
        {
            removeGroup(loop.groups[i]);
        }
        loops.Remove(loop);
        invalidate();
    }

    public void removeGroup(LightSequenceGroup group)
    {
        groups.Remove(group);
        foreach (LightSequenceLoop loop in loops)
        {
            loop.groups.Remove(group);
        }
        invalidate();
    }

    public void tick(float deltaTime)   //Gets called by LightSequenceEditorWindow Update() only, thats fine
    {
        for (int i = 0; i < loops.Count; i++)
        {
            loops[i].tick(deltaTime);
        }
    }

    public void reset()
    {
        foreach (LightSequenceLoop loop in loops)
        {
            loop.resetTime();
        }
    }

    public void invalidate()
    {
        initialize();

        bounds = new Bounds(MainGameObject.transform.position, new Vector3(100.0f, 100.0f, 100.0f));
    }


    public void draw(Camera camera, bool visibilityChecks = true)
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.Log("System doesn't support any ComputeShaders");
            return;
        }

        // check if parent transforms of lights changed to figure out if we need to update their positions
        bool isVisible = false;
        for (int i = 0; i < lightSequenceParents.Count; i++)
        {
            LightSequenceParent parent = lightSequenceParents[i];

            if (visibilityChecks)
            {
                isVisible |= parent.meshRenderer.isVisible;
            }
            else
            {
                isVisible = true;
            }
        }

        if (!isVisible)
        {
            return;
        }

        foreach (LightSequenceParent parent in lightSequenceParents)
        {
            LightSequence.LightParentData lightParentData = parentData[parent.index];
            lightParentData.localToWorld = parent.transform.localToWorldMatrix;
            parentData[parent.index] = lightParentData;
        }

        Profiler.BeginSample("LightSequence: Fill data streams");
        for (int i = 0; i < loops.Count; i++)
        {
            loops[i].fillDataStreams(instancingData, effectGroupData);
        }
        Profiler.EndSample();

        Profiler.BeginSample("LightSequence: Draw");
        if (lights.Count > 0)
        {
            instancingDataBuffer.SetData(instancingData);
            materialPropertyBlock.SetBuffer(instancingDataBufferNameID, instancingDataBuffer);
            effectGroupDataBuffer.SetData(effectGroupData);
            materialPropertyBlock.SetBuffer(effectGroupDataBufferNameID, effectGroupDataBuffer);
            parentDataBuffer.SetData(parentData);
            materialPropertyBlock.SetBuffer(parentDataBufferNameID, parentDataBuffer);

            // AssetManager.Instance.rideLightMesh AssetManager.Instance.rideLightMaterial
            Graphics.DrawMeshInstancedIndirect(LightSequenceHelper.Ride_LightMesh, 0, LightSequenceHelper.RideLightMat, bounds, argsBuffer, 0, materialPropertyBlock, ShadowCastingMode.Off, false, 0, camera);  // camera.current: SceneView is ok, nn null
        }
        Profiler.EndSample();
    }


    private void initialize()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            return;
        }

        LightSequenceHelper.LoadRideLightM();

        materialPropertyBlock = new MaterialPropertyBlock();
        instancingDataBufferNameID = Shader.PropertyToID("LightInstancingDataBuffer");
        effectGroupDataBufferNameID = Shader.PropertyToID("LightEffectGroupDataBuffer");
        parentDataBufferNameID = Shader.PropertyToID("LightParentDataBuffer");

        instancingData = new LightInstancingData[lights.Count];

        argsBuffer?.Release();
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = (uint)LightSequenceHelper.Ride_LightMesh.GetIndexCount(0);   //AssetManager.Instance.rideLightMesh.GetIndexCount(0);
        args[1] = (uint)lights.Count;
        args[2] = (uint)LightSequenceHelper.Ride_LightMesh.GetIndexStart(0);   //AssetManager.Instance.rideLightMesh.GetIndexStart(0);
        args[3] = (uint)LightSequenceHelper.Ride_LightMesh.GetBaseVertex(0);   //AssetManager.Instance.rideLightMesh.GetBaseVertex(0);
        argsBuffer.SetData(args);

        instancingDataBuffer?.Release();    // MESSAGE on Start, because lights.Count is null   // Please use ComputeBuffer.Release() or .Dispose() to manually release the buffer. UnityEngine.ComputeBuffer:Finalize()
        instancingDataBuffer = new ComputeBuffer(lights.Count != 0 ? lights.Count : 1, sizeof(float) * 4 * 4 + sizeof(int) + sizeof(int) + sizeof(float));

        lightSequenceParents.Clear();
        transformParentAssoc.Clear();

        for (int i = 0; i < lights.Count; i++)
        {
            LightSequenceLight light = lights[i];

            if (light.initParentName != null && light.parent == null)
                light.setParent(this);

            LightSequenceParent parent;
            if (!transformParentAssoc.TryGetValue(light.parent, out parent))
            {
                parent = new LightSequenceParent();
                parent.index = lightSequenceParents.Count;
                parent.transform = light.parent;
                parent.meshRenderer = light.parent.GetComponent<MeshRenderer>();
                lightSequenceParents.Add(parent);
                transformParentAssoc.Add(light.parent, parent);
            }

            light.initialize(i, parent, this);

            LightSequence.LightInstancingData instanceData = instancingData[light.index];
            instanceData.localToWorld = light.transform;
            instanceData.parentIndex = parent.index;
            instancingData[light.index] = instanceData;
        }

        parentData = new LightParentData[lightSequenceParents.Count];
        parentDataBuffer?.Release();
        parentDataBuffer = new ComputeBuffer(lightSequenceParents.Count != 0 ? lightSequenceParents.Count : 1, sizeof(float) * 4 * 4);

        effectGroupData = new LightEffectGroupData[groups.Count];
        effectGroupDataBuffer?.Release();
        effectGroupDataBuffer = new ComputeBuffer(groups.Count != 0 ? groups.Count : 1, sizeof(float) * 4 + sizeof(float) + sizeof(int));

        int groupIndex = 0;
        foreach (LightSequenceLoop loop in loops)   //
        {
            for (int i = 0; i < loop.groups.Count; i++)
            {
                //Debug.Log($"LightSquence Initialize() LSLoop Group Count: {loop.groups.Count} , foreach LSLoop Group {i} ");
                loop.groups[i].initialize(groupIndex, this, loop, instancingData, effectGroupData);
                groupIndex++;
            }
        }
    }

    public void OnBeforeSerialize()         // Before Saving the AssetPack to .assetProject
    {
        lightIndexAssoc.Clear();
        for (int i = 0; i < lights.Count; i++)
        {
            lightIndexAssoc.Add(lights[i], i);
        }

        foreach (LightSequenceLoop loop in loops)
        {
            loop.groupIndizes.Clear();
            foreach (LightSequenceGroup group in loop.groups)
            {
                loop.groupIndizes.Add(groups.IndexOf(group));
            }
        }

        foreach (LightSequenceGroup group in groups)
        {
            group.lightIndizes.Clear();
            foreach (LightSequenceLight light in group.lights)
            {
                group.lightIndizes.Add(lightIndexAssoc[light]);
            }
        }
        //Debug.Log("LightSqeuence OnBeforeSerialize() got executed!");
    }

    public void OnAfterDeserialize()        // After Loading the AssetPack from .assetProject
    {
        foreach (LightSequenceLoop loop in loops)
        {
            loop.groups.Clear();
            foreach (int groupIndex in loop.groupIndizes)
            {
                if (groupIndex >= 0 && groupIndex < groups.Count)
                {
                    loop.groups.Add(groups[groupIndex]);
                }
            }
        }

        foreach (LightSequenceGroup group in groups)
        {
            group.lights.Clear();
            foreach (int lightIndex in group.lightIndizes)
            {
                if (lightIndex >= 0 && lightIndex < lights.Count)
                {
                    group.lights.Add(lights[lightIndex]);
                }
            }
        }
        //Debug.Log("LightSqeuence OnAfterDeserialize() got executed! After");
    }

}

[System.Serializable]
public class LightSequenceLight
{
    [JsonIgnore]
    public Transform parent;
    [JsonIgnore]
    internal string initParentName;

    [JsonProperty("parent")]
    public string Serializedparent     //Unfortunately only Reference to name possible. So GameObjects need unique naming and need Ref to Component LightSequence // And MainGameObject.name changes as Prefab using GUID as name instead in Parkitect!
    {
        get { return parent == null ? initParentName : parent.gameObject.name; }    //initParentName is needed here in PAE, because of the JSON DeSerialization 
        set { initParentName = value; }
    }


    /// <summary>
    /// Transform Matrix4x4 with TRS(Position, Rotation(Quaternion), Scale), shear=none, in Local Space
    /// </summary>
    [JsonIgnore]
    //[HideInInspector]
    public Matrix4x4 transform; // LOCAL

    [JsonProperty("transform")]
    public float[] Serializedtransform
    {
        get { return new float[16] { transform[0, 0], transform[1, 0], transform[2, 0], transform[3, 0], transform[0, 1], transform[1, 1], transform[2, 1], transform[3, 1], transform[0, 2], transform[1, 2], transform[2, 2], transform[3, 2], transform[0, 3], transform[1, 3], transform[2, 3], transform[3, 3] }; }
        set { transform = new Matrix4x4(new Vector4(value[0], value[1], value[2], value[3]), new Vector4(value[4], value[5], value[6], value[7]), new Vector4(value[8], value[9], value[10], value[11]), new Vector4(value[12], value[13], value[14], value[15])); }
        /*      // Position X Y Z    // Rotation X Y Z W, transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w    // Couldn't get it work with TRS only, says InvalidTRS Matrix 
        get { return new float[7] {transform[0, 3], transform[1, 3], transform[2, 3], transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w }; }
        set { transform = Matrix4x4.TRS(new Vector3(value[0], value[1], value[2]), new Quaternion(value[3], value[4], value[5], value[6]), Vector3.one); }  // Matrix4x4.TRS()
        */
    }

    /// <summary>
    /// Normal Vector for Halos, in World Space
    /// </summary>
    [JsonIgnore]
    public Vector3 normal;

    [JsonProperty("normal")]
    public float[] Serializednormal
    {
        get { return new float[3] { normal.x, normal.y, normal.z }; }
        set { normal = new Vector3(value[0], value[1], value[2]); }     //normal = new Vector3(value[0], value[1], value[2]);
    }

    [JsonIgnore][System.NonSerialized]
    public float randomValue;

    [JsonIgnore][System.NonSerialized]
    public int lastUpdateFrame;

    [JsonIgnore] public int index { get; private set; }
    [JsonIgnore] public LightSequenceParent lightSequenceParent { get; private set; }

    public void setParent(LightSequence lightSequence)
    {
        //Debug.Log($"LightSequence Light initParentName: {initParentName}, LS MainGameObject: {lightSequence.MainGameObject.name}");
        parent = lightSequence.MainGameObject.name == initParentName ? lightSequence.MainGameObject.transform : LightSequenceHelper.recursiveFindTransform(lightSequence.MainGameObject.transform, initParentName);
    }

    public void initialize(int index, LightSequenceParent lightSequenceParent, LightSequence lightSequence)
    {
        this.index = index;
        this.lightSequenceParent = lightSequenceParent;
        randomValue = UnityEngine.Random.value;
    }

    /// <summary>
    /// Gets the World Position of the Light transform relative to his parent.
    /// </summary>
    public Vector3 getWorldPosition()
    {
        return parent.TransformPoint(new Vector3(transform[0, 3], transform[1, 3], transform[2, 3]));
    }
    
    // selectedLight.setPosition(hit.transform, hit.point, hit.normal);
    public void setPosition(Transform parent, Vector3 position, Vector3 normal)
    {
        normal.Normalize();
        this.parent = parent;   // PP of parent in Local Space as M4x4 * M4x4.Trans.Rot.Scale, is like Matrix4x4.LookAt
        this.transform = CleaningMatrix4x4(parent.worldToLocalMatrix * Matrix4x4.TRS(position, Quaternion.LookRotation(normal, parent.up), Vector3.one));
        this.normal = CleaningNormal(normal);
    }
                                                    
    public void RotateMatrixAroundAxis(LightSequence lightSequence, Vector3 pointOfRotation, Vector3 rotationAxis, float angleOfRotation, GameObject objects = null)    // point axis angle
    {
        Vector3 position = getWorldPosition();
        //Debug.Log($"LightSequence Light RotateMatrixAroundAxis() this.parent: {this.parent.name} on World Position: {this.parent.position}");

        Quaternion quaternion = Quaternion.AngleAxis(angleOfRotation, rotationAxis);    // Vector.up

        Vector3 rotatedNormal = quaternion * normal;   // is actually always correct

        normal = rotatedNormal;

        Matrix4x4 rotation = Matrix4x4.Rotate(quaternion);

        Matrix4x4 translationRotation = Matrix4x4.Translate(position - pointOfRotation);    // pointOfRotation is in WorldSpace

        translationRotation = rotation * translationRotation;

        Matrix4x4 result = Matrix4x4.Translate(pointOfRotation) * translationRotation;   // NN!? * Matrix4x4.Rotate(transform.rotation)     // Already here always rotated to X direction

        /*        */
        try
        {
            SceneRaycastHit hit;                                                                                                        // light.normal * -1f, Invert Direction
            if (EditorHandles_UnityInternal.IntersectRayGameObject(new Ray(new Vector3(result[0, 3]+(0.01f*rotatedNormal.x), result[1, 3]+(0.01f*rotatedNormal.y), result[2, 3]+(0.01f*rotatedNormal.z)), rotatedNormal), objects != null ? objects : lightSequence.MainGameObject, out hit))
            {
                normal = CleaningNormal(hit.normal);

                if (hit.transform != parent)
                {
                    parent = hit.transform;
                    Debug.Log("LightSequence Rotate4x4MatrixAroundAxis() New Parent: " + hit.transform.name);
                }
                transform = CleaningMatrix4x4(parent.worldToLocalMatrix * Matrix4x4.TRS(hit.point, Quaternion.LookRotation(normal, parent.up), Vector3.one));
            }
            else if (EditorHandles_UnityInternal.IntersectRayGameObject(new Ray(new Vector3(result[0, 3]+(0.01f*rotatedNormal.x), result[1, 3]+(0.01f*rotatedNormal.y), result[2, 3]+(0.01f*rotatedNormal.z)), rotatedNormal * -1f), objects != null ? objects : lightSequence.MainGameObject, out hit))
            {
                normal = CleaningNormal(hit.normal);

                if (hit.transform != parent)
                {
                    parent = hit.transform;
                    Debug.Log("LightSequence Rotate4x4MatrixAroundAxis() New Parent: " + hit.transform.name);
                }
                transform = CleaningMatrix4x4(parent.worldToLocalMatrix * Matrix4x4.TRS(hit.point, Quaternion.LookRotation(normal, parent.up), Vector3.one));
            }
            else
            {
                Debug.LogWarning($"Could not correctly Rotate Around and find a GameObject Parent for this Light, Index: {index}, Parent: {parent.name} - Please Undo or Debug!");

                result *= Matrix4x4.Rotate(Quaternion.LookRotation(normal, parent.up)); // Doesnt work always

                result = parent.worldToLocalMatrix * result;

                transform = CleaningMatrix4x4(result);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("LightSequence Rotate Around! Something went wrong! Please Undo - Error Message: " + e.Message);
        }

        lightSequence.invalidate();
        //Debug.Log("LightSqeuence Light RotationMatrixAroundAxis After is ValidTRS? " + transform.ValidTRS().ToString());
    }

    public Matrix4x4 CleaningMatrix4x4(Matrix4x4 matrix)    // Just makes nearly 0.0 or 1.0 to Integer as float
    {

        float toleranceZero = 0.00001f; //1E-05f

        for (int b = 0; b < 4; b++)
        {
            for (int a = 0; a < 4; a++)
            {
                var value = matrix[a, b];
                if (Math.Abs(value) > 0.1f && Math.Abs(Math.Abs(value) - Math.Round(Math.Abs(value))) < toleranceZero)
                {
                    //var before = matrix[a, b];
                    matrix[a, b] = (float) Math.Round(Math.Abs(value)) * Math.Sign(matrix[a, b]);
                    //Debug.Log($"LightSequence RotateAround CleaningMatrix4x4() For matrix[{a}, {b}] -1- Set Value {before} to {matrix[a, b]}");
                    continue;
                }
                else if (Math.Abs(matrix[a, b]) <= toleranceZero)
                {
                    //var before = matrix[a, b];
                    matrix[a, b] = 0.0f;
                    //Debug.Log($"LightSequence RotateAround CleaningMatrix4x4() For matrix[{a}, {b}] -0- Set Value {before} to {matrix[a, b]}");
                    continue;
                }
                /*
                else
                {
                    //var before = matrix[a, b];
                    //Debug.Log($"LightSequence RotateAround CleaningMatrix4x4() For matrix[{a}, {b}] -X-  Value {before}");
                }
                */
            }
        }
        return matrix;
    }

    public Vector3 CleaningNormal(Vector3 vector)     // Just makes nearly 0.0 or 1.0 to Integer as float
    {
        for (int i = 0; i < 3; i++) // x, y, z
        {
            if (Math.Abs(vector[i]) > 0.1f && Math.Abs(vector[i] - Math.Round(vector[i])) < 1E-05f)
            {
                vector[i] = (float)Math.Round(vector[i]);
                continue;
            }
            else if (Math.Abs(vector[i]) <= 1E-05f)
            {
                vector[i] = 0.0f;
                continue;
            }
        }
        return vector;
    }

    public void updateActiveGroup(LightSequenceGroup group, LightSequence.LightInstancingData[] instancingData)
    {
        LightSequence.LightInstancingData instanceData = instancingData[index];
        instanceData.groupIndex = group.index;
        instancingData[index] = instanceData;
    }
}

[System.Serializable]
public class LightSequenceLoop
{
    [JsonIgnore]
    [System.NonSerialized]
    public List<LightSequenceGroup> groups = new List<LightSequenceGroup>();
    // can't store groups directly on this object, otherwise Unity complains about serialization depth :|
    //[HideInInspector]
    public List<int> groupIndizes = new List<int>();

    public string name = "name";
    [JsonIgnore]
    public bool collapsed;
    public bool enabled = true;
    public float duration = 1;
    public float timeScale = 1;
    public float timeOffset;

    [JsonIgnore]
    public float time { get; private set; }
    private float deltaTime;
    private int frameCount;

    public void tick(float deltaTime)
    {
        float realDeltaTime = deltaTime * timeScale;
        this.deltaTime = realDeltaTime;
        time = (time + realDeltaTime) % duration;
        frameCount++;
    }

    public void resetTime()
    {
        time = timeOffset;
        deltaTime = 0;
        frameCount = 0;
    }

    public void fillDataStreams(LightSequence.LightInstancingData[] instancingData, LightSequence.LightEffectGroupData[] effectGroupData)
    {
        if (!enabled)
        {
            return;
        }

        for (int i = 0; i < groups.Count; i++)
        {
            groups[i].fillDataStreams(time, deltaTime, frameCount, instancingData, effectGroupData);
        }
    }
}

[System.Serializable]
public class LightSequenceGroup
{
    [JsonIgnore]
    [System.NonSerialized]
    public List<LightSequenceLight> lights = new List<LightSequenceLight>();
    // Unity can't store references to objects, so we manually serialize the light indizes and restore them after loading
    //[HideInInspector]
    public List<int> lightIndizes = new List<int>();

    public List<LightSequenceEffect> effects = new List<LightSequenceEffect>();

    [JsonIgnore] 
    public int index { get; private set; }

    [JsonIgnore]
    public LightSequence lightSequence { get; private set; }

    [JsonIgnore] 
    public LightSequenceLoop lightSequenceLoop { get; private set; }

    private LightSequenceEffect activeEffect;

    public void initialize(int index, LightSequence lightSequence, LightSequenceLoop lightSequenceLoop, LightSequence.LightInstancingData[] instancingData, LightSequence.LightEffectGroupData[] effectGroupData)
    {
        //Debug.Log($"LightSequence Group initialize - Index {index}, LSLoop {lightSequenceLoop.name}");
        this.index = index;
        this.lightSequenceLoop = lightSequenceLoop;
        this.lightSequence = lightSequence;

        // still need to transform lights if they are off
        LightSequenceEffect lastEffect = null;
        for (int i = 0; i < effects.Count; i++)
        {
            LightSequenceEffect effect = effects[i];
            if (lastEffect == null || effect.endTime > lastEffect.endTime)
            {
                lastEffect = effect;
            }
        }

        if (lastEffect != null)
        {
            for (int j = 0; j < lights.Count; j++)
            {
                lights[j].updateActiveGroup(this, instancingData);
            }

            LightSequence.LightEffectGroupData effectData = effectGroupData[index];
            effectData._Color = lastEffect.getColor(lightSequence);
            effectData._Intensity = lastEffect.intensity;
            effectData.twinkle = (lastEffect.effectType == LightSequenceEffect.EffectType.Twinkle) ? 1 : 0;
            effectGroupData[index] = effectData;
        }
    }

    public void fillDataStreams(float time, float deltaTime, int frameCount, LightSequence.LightInstancingData[] instancingData, LightSequence.LightEffectGroupData[] effectGroupData)
    {
        LightSequenceEffect previousActiveEffect = activeEffect;
        if (activeEffect == null || !(time >= activeEffect.startTime && time < activeEffect.endTime))
        {
            activeEffect = null;
            for (int i = 0; i < effects.Count; i++)
            {
                LightSequenceEffect effect = effects[i];
                if (time >= effect.startTime && time < effect.endTime)
                {
                    activeEffect = effect;
                    break;
                }
            }
        }

        if (activeEffect != null)
        {
            activeEffect.fillDataStreams(lightSequence, this, time, deltaTime, frameCount, instancingData, effectGroupData);
        }
        else
        {
            LightSequence.LightEffectGroupData effectData = effectGroupData[index];
            effectData._Color.w = 0;
            effectGroupData[index] = effectData;
        }

        if (previousActiveEffect != activeEffect)
        {
            if (activeEffect != null)
            {
                for (int j = 0; j < lights.Count; j++)
                {
                    lights[j].updateActiveGroup(this, instancingData);
                }
            }
        }
    }
}

[System.Serializable]
public class LightSequenceEffect
{
    public float startTime;
    public float duration;
    
    public ColorType colorType = 0;

    [JsonIgnore]
    public Color color = Color.white;

    public float intensity = 1;
    public EffectType effectType;
    public int effectRepetitions;

    [JsonIgnore]
    public AnimationCurve fadeCurve;

    public enum ColorType
    {
        //Fixed = 100,
        CustomColor1 = 0,
        CustomColor2 = 1,
        CustomColor3 = 2,
        CustomColor4 = 3
    }

    public LightSequenceEffect()
    {
        if (fadeCurve == null)
        {
            fadeCurve = new AnimationCurve
            {
                keys = new Keyframe[] {
                    new Keyframe(0, 0, 0, 0), new Keyframe(0.5f, 1, 0, 0), new Keyframe(1f, 0, 0, 0)
                }
            };
        }
    }

    public LightSequenceEffect(LightSequenceEffect effect)  // For Copying LSEffects from another Group
    {
        startTime = effect.startTime;
        duration = effect.duration;
        colorType = effect.colorType;
        color = effect.color;
        intensity = effect.intensity;
        effectType = effect.effectType;
        effectRepetitions = effect.effectRepetitions;
        if (effect.fadeCurve != null) {
            fadeCurve = new AnimationCurve(effect.fadeCurve.keys);
        }
    }

    public enum EffectType
    {
        OnOff, Fade, Twinkle
    }

    public float endTime
    {
        get
        {
            return startTime + duration;
        }
        set
        {
            duration = value - startTime;
        }
    }

    public void fillDataStreams(LightSequence lightSequence, LightSequenceGroup group, float time, float deltaTime, int frameCount, LightSequence.LightInstancingData[] instancingData, LightSequence.LightEffectGroupData[] effectGroupData)
    {
        LightSequence.LightEffectGroupData effectData = effectGroupData[group.index];
        effectData._Color = getColor(lightSequence);
        effectData._Intensity = intensity;
        effectData.twinkle = (effectType == LightSequenceEffect.EffectType.Twinkle) ? 1 : 0;

        switch (effectType)
        {
            case EffectType.OnOff:
                effectData._Color.w = 1;
                break;
            case EffectType.Fade:
                float realDurationFade = duration / (effectRepetitions + 1);
                float progress = ((time - startTime) % realDurationFade) / realDurationFade;
                effectData._Color.w = fadeCurve.Evaluate(progress);
                break;
            case EffectType.Twinkle:
                foreach (LightSequenceLight light in group.lights)
                {
                    float realDurationTwinkle = duration / (effectRepetitions + 1);
                    float twinkleProgress = ((time + light.randomValue * realDurationTwinkle) % realDurationTwinkle) / realDurationTwinkle;
                    LightSequence.LightInstancingData instanceData = instancingData[light.index];
                    instanceData.twinkleIntensity = twinkleProgress;
                    instancingData[light.index] = instanceData;
                }
                break;
        }

        effectGroupData[group.index] = effectData;
    }

    public Color getColor(LightSequence lightSequence)
    {

        int colorIndex = (int)colorType;
        Assert.IsNotNull(lightSequence.customColors);
        if (lightSequence.customColors != null && colorIndex < lightSequence.customColors.Length)
        {
            return lightSequence.customColors[colorIndex];
        }

        return color;
    }
}

public class LightSequenceParent
{
    [JsonIgnore]
    public MeshRenderer meshRenderer;
    [JsonIgnore]
    public Transform transform;
    [JsonIgnore]
    public int index;
}
