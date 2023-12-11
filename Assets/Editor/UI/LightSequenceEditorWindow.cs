using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHandles_UnityInternal;

namespace ParkitectAssetEditor.UI
{
    /// <inheritdoc />
    /// <summary>
    /// The LightSqeuence editor window.
    /// </summary>
    /// <seealso cref="T:UnityEditor.EditorWindow" />
    /// 
    //[CustomEditor(typeof(LightSequence))]	//Actually NN for SerializedObject EditorGUILayout.PropertyField
    //[CanEditMultipleObjects]
    public class LightSequenceEditorWindow : EditorWindow 
    {
        // For Debug Option
        private static bool showLightIdxDebug = false;
        private static SerializedObject serializedLS;
        private static SerializedProperty serializedlights;
        private static SerializedProperty serializedloops;
        private static SerializedProperty serializedgroups;
        private Vector2 debugScrollPosition;

        private static bool enabledLSEWSceneView = false;

        private static bool lockMoving = true;
        private static bool snapX = true, snapY = true, snapZ = true;

        private static float lightSnapDistance = 0.02f;
        private static float lightSnapRadius = 0.15f;
        private static float lineToolLightDistance = lightRadius * 2 + lightRadius / 2; // or 0.1f 

        private static Vector3Int rotateAroundV3I = new Vector3Int(0, 1, 0);
        private static bool rotateToolDuplicate = true;
        private float rotateTooldegrees = 45.0f;
        private Transform rotateTooltransform;
        private GameObject rotateToolGOchilds;
        private List<LightSequenceLight> selectedLights4RotateT = new List<LightSequenceLight>();

        private const float lightRadius = 0.025f;
        private const float zoomFactor = 50;
        private const float effectInspectorWidth = 300;
        private const int snapZoneSize = 5;
        private LightSequence lightSequence;
        private LightSequenceLight selectedLight;
        private LightSequenceLight lastSelectedLight;
        private LightSequenceGroup selectedGroup;
        private LightSequenceGroup previouslySelectedGroup;
        private LightSequenceEffect selectedEffect;
        private bool playing = false;
        private double lastFrameTime;
        private Vector2 mouseDownPosition;
        private bool mouseDown;
        private bool isDraggingLight = false;
        private LightSequenceEffect draggedEffect;
        private bool isDraggingEffect = false;
        private LightSequenceLight snappedToX = null;
        private LightSequenceLight snappedToY = null;
        private LightSequenceLight snappedToZ = null;
        private Vector3 lineToolStart;
        private Vector3 lineToolEnd;
        private Vector3 lineToolStartNormal;
        private Transform lineToolStartParent;
        private Vector2 sequencerScrollPosition;
        private int controlID;

        private EffectOperation effectOperation;
        private enum EffectOperation {
            None, Move, ResizeLeft, ResizeRight
        }

        private enum Tool {
            Place, PickGroup, LineTool, SelectRotateTool
        }
        private Tool tool;

        public static void ShowWindow(LightSequence lightSequence, Asset asset)
        {
            if (lightSequence.Asset == null)
                lightSequence.SetAsset(asset);

            if (serializedLS != null)
                serializedLS = null;

            LightSequenceEditorWindow lightSequenceEditorWindow = EditorWindow.GetWindow<LightSequenceEditorWindow>(false, "PAE LightSequence Editor Window", true);
            lightSequenceEditorWindow.lightSequence = lightSequence;
            lightSequenceEditorWindow.lightSequence.invalidate();

        }

        public void Awake() {
            SceneView.duringSceneGui += OnScene;    // 'SceneView.onSceneGUIDelegate' is obsolete: 'onSceneGUIDelegate has been deprecated. Use duringSceneGui instead.'	SceneView.duringSceneGui
            enabledLSEWSceneView = false;
        }

        public void OnDestroy() {
            SceneView.duringSceneGui -= OnScene;    // 'SceneView.onSceneGUIDelegate' is obsolete: 'onSceneGUIDelegate has been deprecated. Use duringSceneGui instead.'	SceneView.duringSceneGui
            enabledLSEWSceneView = false;
            serializedLS = null;
        }

        void OnGUI()		//NEEDED A SEPERATED UI, highlight active tools more, seperate left and right area, more descr or tooltip
        {
            if (lightSequence == null)
            {
                EditorGUILayout.HelpBox("Nothing to show here - Please reopen/load the LightSequence Editor Window of the Selected Asset per Parkitect Asset Editor ", MessageType.Warning);
                return;
            }

            GUILayout.BeginHorizontal();

            Rect sequencerRect = new Rect(0, 0, position.width - effectInspectorWidth, position.height);
            GUILayout.BeginArea(sequencerRect);  // left: sequencer


            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUIStyle lsewStyle = new GUIStyle(GUI.skin.button);
            if (enabledLSEWSceneView)
            {
                lsewStyle.fontStyle = FontStyle.Bold;
                lsewStyle.normal.textColor = new Color(0.1f, 0.9f, 0.1f, 1f);
                lsewStyle.fontSize = 13;
            }
            if (GUILayout.Button(enabledLSEWSceneView ? "Activated LS SceneView with controls" : "Deactivated LS SceneView no controls", lsewStyle, GUILayout.Width(400)))
            {
                enabledLSEWSceneView = !enabledLSEWSceneView;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Box($"Loaded LightSequence for:  {lightSequence?.MainGameObject.name}", new GUIStyle(GUI.skin.box), GUILayout.Width(300));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Some Hints: -If LS SceneView activated: LeftClick is Light placement, so Pan, Rotate with MMB, RMB    -Possible UNDO: Light placement, Light/Group Deletion, Line Tool Add & Interpolation, Rotate Around ", MessageType.Info);

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUIStyle buttonPlayStyle = new GUIStyle(GUI.skin.button);
            if (playing)
            {
                buttonPlayStyle.fontStyle = FontStyle.Bold; 
                buttonPlayStyle.normal.textColor = new Color(0.1f, 0.9f, 0.1f, 1f);
            }

            if (GUILayout.Button(playing ? "■ Stop" : "► Play", buttonPlayStyle, GUILayout.Width(180))) {
                playing = !playing;
                lightSequence.invalidate();
                lightSequence.reset();
                lastFrameTime = EditorApplication.timeSinceStartup;
                Repaint();	//Editor Window
                SceneView.RepaintAll();	//Scene View, Scene reicht
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Delete Selected Light (F12)", GUILayout.Width(170)))
            {
                deleteLight();
            }
            GUIStyle buttonIdxStyle = new GUIStyle(GUI.skin.button);
            if (showLightIdxDebug) buttonIdxStyle.normal.textColor = new Color(0.8f, 0f, 0f, 1f);
            if (GUILayout.Button("LightIdx + Debug", buttonIdxStyle, GUILayout.Width(120)))
            {
                showLightIdxDebug = !showLightIdxDebug;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Move Tool: ", MessageType.None);
            GUILayout.BeginHorizontal();

            GUIStyle buttonMoveStyle = new GUIStyle(GUI.skin.button);
            if (!lockMoving)
            {
                buttonMoveStyle.fontStyle = FontStyle.Bold;
                buttonMoveStyle.normal.textColor = new Color(0f, 1f, 0f, 1f);
            }
            if (GUILayout.Button("Moving Lights", buttonMoveStyle, GUILayout.Width(150)))
            {
                lockMoving = !lockMoving;
            }

            GUILayout.Label(lockMoving ? "Locked" : "Unlocked", GUILayout.Width(60));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Snap to near Lights in", GUILayout.Width(130));
            snapX = GUILayout.Toggle(snapX, "X", GUILayout.Width(30));
            snapY = GUILayout.Toggle(snapY, "Y", GUILayout.Width(30));
            snapZ = GUILayout.Toggle(snapZ, "Z", GUILayout.Width(30));
            EditorGUIUtility.labelWidth = 90;
            lightSnapDistance = EditorGUILayout.FloatField("Snap distance", lightSnapDistance, GUILayout.ExpandWidth(false));
            lightSnapRadius = EditorGUILayout.FloatField("within radius", lightSnapRadius, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndHorizontal();

            GUILayout.Space(15);


            EditorGUILayout.HelpBox("Line Tool:     1. Place or Select a Light for Reference    2. Left-Click, then Right-Click on Mesh to project line on Mesh    3. Add next Light or Interpolate Distance between Lights    -Needs always a Selected Light!", MessageType.None);
            GUILayout.BeginHorizontal();
            GUIStyle buttonLineTStyle = new GUIStyle(GUI.skin.button);
            if (tool == Tool.LineTool)
            {
                buttonLineTStyle.fontStyle = FontStyle.Bold;
                buttonLineTStyle.normal.textColor = new Color(0f, 1f, 0f, 1f);
            }
            if (GUILayout.Button("Line Tool", buttonLineTStyle))
            {
                if (tool != Tool.LineTool) {
                    tool = Tool.LineTool;
                }
                else {
                    tool = Tool.Place;
                }
                SceneView.RepaintAll();
            }
            GUILayout.Label("Distance betw. lights", GUILayout.Width(130));
            lineToolLightDistance = EditorGUILayout.FloatField(lineToolLightDistance, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Add")) {
                addLineToolLight();
            }
            if (GUILayout.Button("Interpolate")) {
                interpolateLineToolLights();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15);


            EditorGUILayout.HelpBox("Rotate Around Tool:    1. Select Lights in right Order    2. Set the correct Rotation Axis and degrees    3. and a Transform as Pivot Point    4. Rotate around    -Only on Active Objects", MessageType.None);
            GUILayout.BeginHorizontal();
            GUIStyle selectLTStyle = new GUIStyle(GUI.skin.button);
            if (tool == Tool.SelectRotateTool)
            {
                selectLTStyle.fontStyle = FontStyle.Bold;
                selectLTStyle.normal.textColor = new Color(0f, 1f, 0f, 1f);
            }
            if (GUILayout.Button("Rotate Around Select", selectLTStyle))
            {
                if (tool != Tool.SelectRotateTool) {
                    tool = Tool.SelectRotateTool;
                }
                else
                {
                    tool = Tool.Place;
                    selectedLights4RotateT.Clear();
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Clear Selection"))
            {
                selectedLights4RotateT.Clear();
            }
            //EditorGUIUtility.labelWidth = 100;
            GUILayout.Space(20);
            rotateAroundV3I = EditorGUILayout.Vector3IntField("Rotation Axis (mostly Y as up, and 0 or 1)", rotateAroundV3I);
            rotateAroundV3I.x = Mathf.Clamp(rotateAroundV3I.x, -1, 1);
            rotateAroundV3I.y = Mathf.Clamp(rotateAroundV3I.y, -1, 1);
            rotateAroundV3I.z = Mathf.Clamp(rotateAroundV3I.z, -1, 1);

            GUILayout.Label("Rotation degrees°", GUILayout.Width(110));
            EditorGUIUtility.labelWidth = 100;
            rotateTooldegrees = EditorGUILayout.FloatField(rotateTooldegrees, GUILayout.Width(50));	//, GUILayout.ExpandWidth(false)

            rotateTooltransform = EditorGUILayout.ObjectField("  PivotPoint", rotateTooltransform, typeof(Transform), true, GUILayout.MinWidth(200)) as Transform;

            rotateToolGOchilds = EditorGUILayout.ObjectField("  On GO & childs", rotateToolGOchilds, typeof(GameObject), true, GUILayout.MinWidth(200)) as GameObject;

            rotateToolDuplicate = GUILayout.Toggle(rotateToolDuplicate, "Duplicate", GUILayout.Width(80));

            if (GUILayout.Button("Rotate around!"))
            {
                if (rotateTooltransform != null)
                {
                    Undo.RecordObject(lightSequence, "Rotate around!");

                    if (rotateToolDuplicate)
                    {
                        List<LightSequenceLight> duplicateLights = new List<LightSequenceLight>();
                        foreach (LightSequenceLight light in selectedLights4RotateT)
                        {
                            LightSequenceLight duplicate = lightSequence.addDuplicateLight(light.parent, light.transform, light.normal);
                            duplicateLights.Add(duplicate);
                            if (selectedGroup != null)
                            {
                                selectedGroup.lights.Add(duplicate);
                            }
                        }

                        selectedLights4RotateT.Clear();
                        foreach (LightSequenceLight duplicateLight in duplicateLights)
                        {
                            lightSequence.RotateLightAroundAxis(duplicateLight, rotateTooltransform.position, rotateAroundV3I, rotateTooldegrees, rotateToolGOchilds);
                            selectedLights4RotateT.Add(duplicateLight);
                        }
                    }
                    else
                    {
                        foreach (LightSequenceLight light in selectedLights4RotateT)
                        {
                            lightSequence.RotateLightAroundAxis(light, rotateTooltransform.position, rotateAroundV3I, rotateTooldegrees, rotateToolGOchilds);
                        }
                    }
                    EditorUtility.SetDirty(lightSequence);
                }
                else
                {
                    Debug.LogWarning("Cannot rotate around without a Transform as Pivotpoint. Please choose a Transform");
                }
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (GUILayout.Button("1. Add LightLoop", GUILayout.Width(150)))
            {
                lightSequence.loops.Add(new LightSequenceLoop());
            }
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Light Loops", MessageType.None);

            EditorGUILayout.BeginVertical();
            sequencerScrollPosition = EditorGUILayout.BeginScrollView(sequencerScrollPosition);

            if (Event.current.type == EventType.MouseDown) {
                mouseDownPosition = Event.current.mousePosition;
                draggedEffect = null;
            }

            if (lightSequence.loops != null)
            {
                for (int i = 0; i < lightSequence.loops.Count; i++)
                {
                    drawLoop(lightSequence.loops[i], i);
                }
            }

            if (Event.current.type == EventType.MouseUp) {
                effectOperation = EffectOperation.None;
                draggedEffect = null;
                isDraggingEffect = false;
            }

            GUIStyle TlightsStyle = new GUIStyle(GUI.skin.label);
            TlightsStyle.margin = new RectOffset(5, 0, 10, 0);
            TlightsStyle.alignment = TextAnchor.MiddleLeft;
            TlightsStyle.fontSize = 14;
            GUILayout.Label("Total lights: " + lightSequence.lights.Count, TlightsStyle, GUILayout.Height(30f));
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            GUILayout.EndArea(); // left: sequencer

            // effect inspector
            // Rect sequencerRect = new Rect(0, 0, position.width - effectInspectorWidth, position.height);
            Rect effectInspectorRect = new Rect(position.width - effectInspectorWidth, 0, effectInspectorWidth, position.height);   //position.height
            GUILayout.BeginArea(effectInspectorRect, new GUIStyle(GUI.skin.box));

            if (showLightIdxDebug)
            {
                if (serializedLS == null)
                    serializedLS = new SerializedObject(lightSequence);

                serializedLS.Update();

                EditorGUILayout.BeginVertical();
                debugScrollPosition = EditorGUILayout.BeginScrollView(debugScrollPosition);	// GUILayout.Width(300)
                serializedlights = serializedLS.FindProperty("lights");
                EditorGUILayout.PropertyField(serializedlights, new GUIContent("Lights "), true); // , GUILayout.Width(280), GUILayout.ExpandHeight(true)
                GUILayout.Space(10);
                serializedloops = serializedLS.FindProperty("loops");
                EditorGUILayout.PropertyField(serializedloops, new GUIContent("Loops "), true);
                GUILayout.Space(10);
                serializedgroups = serializedLS.FindProperty("groups");
                EditorGUILayout.PropertyField(serializedgroups, new GUIContent("Groups "), true);
                GUILayout.Space(10);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                drawEffectInspector(selectedEffect);
            }

            GUILayout.EndArea(); // right: effect inspector

            EditorGUILayout.EndHorizontal();
        }


        private void OnScene(SceneView sceneview)   //	CONTROLS	AND RENDERING LIGHTS
        {
            if (!enabledLSEWSceneView)
                return;

            Event e = Event.current;

            if (!e.alt)	// For Control Alt + LMB: Rotate View
            {
                // We use hotControl to lock focus onto the editor (to prevent deselection)
                bool clickedLeft = false;
                controlID = GUIUtility.GetControlID(FocusType.Passive); //New GUI Controls, Passive: No longer keyboard focus
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        if (tool == Tool.LineTool)		// Set Line Tool Line
                        {
                            handleLineToolClick();
                        }
                        else if (e.button == 0)			// Clicked somewhere
                        {
                            mouseDown = true;
                            clickedLeft = true;
                            GUIUtility.hotControl = controlID;

                            mouseDownPosition = Event.current.mousePosition;

                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (e.button == 0 && tool == Tool.Place && selectedLight != null && !lockMoving)	// Move Light
                        {
                            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                            SceneRaycastHit hit;
                            if (EditorHandles_UnityInternal.IntersectRayGameObject(ray, lightSequence.MainGameObject, out hit))
                            {
                                selectedLight.setPosition(hit.transform, hit.point, hit.normal);
                                snapLight(selectedLight);
                                EditorUtility.SetDirty(lightSequence);
                            }

                            isDraggingLight = true;

                            Event.current.Use();
                        }
                        else if (tool == Tool.LineTool)	// Set Line Tool Line
                        {
                            handleLineToolClick();
                        }
                        break;

                    case EventType.KeyDown:                                         //Better not mention KeyCode.Delete, because could delete the GameObject, instead F12
                        if (e.keyCode == KeyCode.F12 || e.keyCode == KeyCode.Delete && selectedLight != null)
                        {
                            deleteLight();
                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseUp:		// 
                        if (e.button == 0)
                        {
                            mouseDown = false;
                            isDraggingLight = false;
                            lastSelectedLight = null;
                            snappedToX = null;
                            snappedToY = null;
                            snappedToZ = null;
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }
                        break;
                }

                if (mouseDown && !isDraggingLight)		// Pick a light
                {
                    // pick light
                    if (clickedLeft)
                    {
                        selectedLight = null;
                    }

                    LightSequenceLight lightBelowMouse = getLightBelowMouse(lastSelectedLight);
                    if (lightBelowMouse != lastSelectedLight)
                    {
                        selectedLight = lightBelowMouse;
                        lastSelectedLight = lightBelowMouse;
                        Repaint();
                        SceneView.RepaintAll();
                    }
                }

                if (clickedLeft)
                {
                    if (selectedLight == null && tool == Tool.Place)	//Add new Light
                    {
                        addNewLight();
                    }
                }

                if (tool == Tool.PickGroup && e.button == 0 && selectedGroup != null && selectedLight != null)	// Pick Group
                {
                    Undo.RecordObject(lightSequence, "Change light group");
                    if (selectedGroup.lights.Contains(selectedLight))
                    {
                        selectedGroup.lights.Remove(selectedLight);
                    }
                    else
                    {
                        selectedGroup.lights.Add(selectedLight);
                    }
                    lightSequence.invalidate();
                    selectedLight = null;
                    EditorUtility.SetDirty(lightSequence);
                    Repaint();
                    SceneView.RepaintAll();
                }

                if (tool == Tool.SelectRotateTool && e.button == 0 && selectedLight != null)	// Selection for Rotate Tool
                {
                    //Undo.RecordObject(lightSequence, "Change light group");
                    if (!selectedLights4RotateT.Contains(selectedLight))
                    {
                        selectedLights4RotateT.Add(selectedLight);
                    }
                    else
                    {
                        selectedLights4RotateT.Remove(selectedLight);
                    }
                    //lightSequence.invalidate();
                    selectedLight = null;
                    //EditorUtility.SetDirty(lightSequence);
                    Repaint();
                    SceneView.RepaintAll();
                }
            }

            // SHOWS THE HALOS	AND RENDERS THE LIGHTS

            if (e.type == EventType.Repaint) 
            {

                if (playing) {
                    lightSequence.draw(Camera.current, true);
                }

                //Handles. Custom 3D GUI controls and drawing in the SceneView.

                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                if (tool == Tool.PickGroup && selectedGroup != null) {
                    foreach (LightSequenceLight light in lightSequence.lights) {
                        if (selectedGroup.lights.Contains(light)) {
                            Handles.color = Color.green;
                        }
                        else if (previouslySelectedGroup != null && previouslySelectedGroup.lights.Contains(light)) {
                            Handles.color = Color.cyan;
                        }
                        else {
                            Handles.color = Color.red;
                        }
                        drawLightHandle(light);
                    }
                }
                else if (tool == Tool.SelectRotateTool && selectedLights4RotateT != null)
                {
                    foreach (LightSequenceLight light in lightSequence.lights)
                    {
                        if (selectedLights4RotateT.Contains(light))
                        {
                            Handles.color = Color.green;
                        }
                        else
                        {
                            Handles.color = Color.red;
                        }
                        drawLightHandle(light);
                    }
                }
                else if (!playing) 
                {
                    for (int i = 0; i < lightSequence.lights.Count; i++) 
                    {

                        if (lightSequence.lights[i] == selectedLight)
                        {
                            Handles.color = Color.green;
                        }
                        else
                        {
                            Handles.color = Color.white;
                        }
                        if (!showLightIdxDebug)
                            drawLightHandle(lightSequence.lights[i]);
                        else
                            drawLightHandle(lightSequence.lights[i], i); 
                    }
                    /*
                    foreach (LightSequenceLight light in lightSequence.lights) {
                        if (light == selectedLight) {
                            Handles.color = Color.green;
                        }
                        else {
                            Handles.color = Color.white;
                        }
                        drawLightHandle(light);
                    }
                    */
                }

                // Snapping
                if (selectedLight != null) {
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Disabled;
                    if (snappedToX != null) {
                        Handles.color = Color.red;		//X
                        Handles.DrawLine(selectedLight.getWorldPosition(), snappedToX.getWorldPosition());
                    }
                    if (snappedToY != null) {
                        Handles.color = Color.green;	//Y
                        Handles.DrawLine(selectedLight.getWorldPosition(), snappedToY.getWorldPosition());
                    }
                    if (snappedToZ != null) {
                        Handles.color = Color.blue;		//Z
                        Handles.DrawLine(selectedLight.getWorldPosition(), snappedToZ.getWorldPosition());
                    }
                }

                // LineTool Line
                if (tool == Tool.LineTool) {
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Disabled;
                    Handles.color = Color.cyan;
                    Handles.DrawLine(lineToolStart, lineToolEnd);
                }
            }

        }

        private LightSequenceLight getLightBelowMouse(LightSequenceLight selectedLight = null) {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            SceneRaycastHit hit;
            if (EditorHandles_UnityInternal.IntersectRayGameObject(ray, lightSequence.MainGameObject, out hit)) {
                foreach (LightSequenceLight light in lightSequence.lights) {
                    float selectionRadius = lightRadius;
                    if (light == selectedLight) {
                        /*
                         *
                         */
                        selectionRadius *= 1.5f;
                    }
                    if (Vector3.Distance(light.getWorldPosition(), hit.point) < selectionRadius) {
                        return light;
                    }
                }
            }

            // no light near geometry where we clicked? maybe there's a floating light below the cursor?
            /*float closestDist = float.MaxValue;
            LightSequenceLight closestLight = null;
            foreach (LightSequenceLight light in lightSequence.lights) {
                float dist = HandleUtility.DistanceToDisc(light.getWorldPosition(), light.normal, lightRadius);
                Debug.Log(1f / HandleUtility.GetHandleSize(light.getWorldPosition()) * lightRadius + " / " + dist);
                if (dist < closestDist && dist < 1f / HandleUtility.GetHandleSize(light.getWorldPosition()) * lightRadius) {
                    closestDist = dist;
                    closestLight = light;
                }
            }*/

            return null;
        }

        /// <summary>
        /// Handles World Position on Click for Line Tool
        /// </summary>
        private void handleLineToolClick() {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            SceneRaycastHit hit;                                        // gameObject of the Component, single
            if (EditorHandles_UnityInternal.IntersectRayGameObject(ray, lightSequence.MainGameObject, out hit)) {
                LightSequenceLight lightBelowMouse = getLightBelowMouse();
                if (lightBelowMouse != null) {
                    hit.point = lightBelowMouse.getWorldPosition();
                    hit.normal = lightBelowMouse.normal;
                    hit.transform = lightBelowMouse.parent;
                    if (Event.current.button == 0) {
                        selectedLight = lightBelowMouse;
                    }
                }

                if (Event.current.button == 0) {
                    lineToolStart = hit.point;
                    lineToolStartNormal = hit.normal;
                    lineToolStartParent = hit.transform;
                }
                else if (Event.current.button == 1) {
                    lineToolEnd = hit.point;
                }
            }

            if (Event.current.button == 0 || Event.current.button == 1) {
                GUIUtility.hotControl = controlID;
                Event.current.Use();
            }
        }

        /// <summary>
        /// Adds first Light at Start point of the line with Line Tool
        /// </summary>
        private void addLineToolLight() 
        {
            Vector3 lineDirection = (lineToolEnd - lineToolStart).normalized;
            Vector3 startPosition = lineToolStart;
            Vector3 normal = lineToolStartNormal;
            Transform parent = lineToolStartParent;
            float offset = 0;

            //Debug.Log($"LightSequence Line Tool addLineToolLight Selected Light : {selectedLight?.index}  LightSequenceLight");
            if (selectedLight != null)	// Adds Light after the new Selected Light
            {
                startPosition = selectedLight.getWorldPosition();
                normal = selectedLight.normal;
                parent = selectedLight.parent;
                offset = lineToolLightDistance;
            }

            Undo.RecordObject(lightSequence, "Add light");	// Does not reset to Selected Light, because selectedLight is here not in LS
            // selectedLight = lastSelectedLight;
            selectedLight = lightSequence.addLight(parent, startPosition + lineDirection * offset, normal);
            //Debug.Log($"LightSequence Line Tool addLineToolLight ADDED Selected Light : {selectedLight.index}  {selectedLight}");
            if (selectedGroup != null) {
                selectedGroup.lights.Add(selectedLight);
            }
            EditorUtility.SetDirty(lightSequence);  // You can use SetDirty when you want to modify an object without creating an undo entry, but still ensure the change is registered and not lost.
        }

        /// <summary>
        /// Interpolate Lights Distance on line with Line Tool
        /// </summary>
        private void interpolateLineToolLights() {
            Vector3 lineDistance = (lineToolEnd - lineToolStart);
            Vector3 lineDirection = lineDistance.normalized;
            Vector3 startPosition = lineToolStart;
            Vector3 normal = lineToolStartNormal;
            Transform parent = lineToolStartParent;
            int fittingLights = Mathf.CeilToInt(lineDistance.magnitude / lineToolLightDistance);
            float offset = 0;

            //Debug.Log($"LightSequence Line Tool interpolateLineToolLights Selected Light : {selectedLight.index}  {selectedLight}");
            if (selectedLight != null)  // Adds Light after the new Selected Light
            {
                startPosition = selectedLight.getWorldPosition();
                normal = selectedLight.normal;
                parent = selectedLight.parent;
                fittingLights--;
            }
            float realDistance = lineDistance.magnitude / fittingLights;
            if (selectedLight != null) {
                offset = realDistance;
            }

            Undo.RecordObject(lightSequence, "Add lights"); // Does not reset to Selected Light, because selectedLight is here not in LS
            for (int i = 0; i < fittingLights - 1; i++) {
                Vector3 interpolatedPosition = startPosition + lineDirection * (offset + i * realDistance);
                Vector3 interpolatedNormal = normal;
                Transform interpolatedParent = parent;
                SceneRaycastHit hit;
                if (EditorHandles_UnityInternal.IntersectRayGameObject(new Ray(interpolatedPosition + normal * 0.025f, -normal), lightSequence.MainGameObject, out hit)) {
                    interpolatedPosition = hit.point;
                    interpolatedNormal = hit.normal;
                    interpolatedParent = hit.transform;
                }
                // selectedLight = lastSelectedLight;
                selectedLight = lightSequence.addLight(interpolatedParent, interpolatedPosition, interpolatedNormal);
                //Debug.Log($"LightSequence Line Tool interpolateLineToolLights ADDED Selected Light : {selectedLight.index}  {selectedLight}");
                if (selectedGroup != null) {
                    selectedGroup.lights.Add(selectedLight);
                }
            }
            EditorUtility.SetDirty(lightSequence);  // You can use SetDirty when you want to modify an object without creating an undo entry, but still ensure the change is registered and not lost.
        }

        protected void Update() {
            if (playing) {
                lightSequence.tick((float)(EditorApplication.timeSinceStartup - lastFrameTime));
                SceneView.RepaintAll();
                Repaint();
                lastFrameTime = EditorApplication.timeSinceStartup;
            }
        }

        /// <summary>
        /// Adds New Light
        /// </summary>
        private void addNewLight() {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            SceneRaycastHit hit;                                        // gameObject of the Component, single
            if (EditorHandles_UnityInternal.IntersectRayGameObject(ray, lightSequence.MainGameObject, out hit)) {
                Undo.RecordObject(lightSequence, "Add light");
                selectedLight = lightSequence.addLight(hit.transform, hit.point, hit.normal);
                if (selectedGroup != null) {
                    selectedGroup.lights.Add(selectedLight);
                }
                snapLight(selectedLight);
                EditorUtility.SetDirty(lightSequence);
            }
        }

        /// <summary>
        /// Deletes selected Light
        /// </summary>
        private void deleteLight() {
            Undo.RecordObject(lightSequence, "Remove light");
            lightSequence.removeLight(selectedLight);
            EditorUtility.SetDirty(lightSequence);
            selectedLight = null;
            lastSelectedLight = null;
        }

        /// <summary>
        /// Snaps Light
        /// </summary>
        private void snapLight(LightSequenceLight light) {
            if (!snapX && !snapY && !snapZ) {
                return;
            }

            Vector3 position = light.getWorldPosition();
            Vector3 snappedPosition = position;
            Vector3 snapDist = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            snappedToX = null;
            snappedToY = null;
            snappedToZ = null;
            foreach (LightSequenceLight otherLight in lightSequence.lights) {
                if (otherLight == light) {
                    continue;
                }

                Vector3 otherPosition = otherLight.getWorldPosition();
                Vector3 dist = position - otherPosition;
                if (dist.magnitude > lightSnapRadius) {
                    continue;
                }

                if (snapX && Mathf.Abs(dist.x) < lightSnapDistance && Mathf.Abs(dist.x) < snapDist.x) {
                    snapDist.x = Mathf.Abs(dist.x);
                    snappedPosition.x = otherPosition.x;
                    snappedToX = otherLight;
                }
                if (snapY && Mathf.Abs(dist.y) < lightSnapDistance && Mathf.Abs(dist.y) < snapDist.y) {
                    snapDist.y = Mathf.Abs(dist.y);
                    snappedPosition.y = otherPosition.y;
                    snappedToY = otherLight;
                }
                if (snapZ && Mathf.Abs(dist.z) < lightSnapDistance && Mathf.Abs(dist.z) < snapDist.z) {
                    snapDist.z = Mathf.Abs(dist.z);
                    snappedPosition.z = otherPosition.z;
                    snappedToZ = otherLight;
                }
            }
            if (snappedToX != null || snappedToY != null || snappedToZ != null) {
                SceneRaycastHit hit;
                if (IntersectRayGameObject(new Ray(snappedPosition - light.normal * 0.05f, light.normal), lightSequence.MainGameObject, out hit)) {
                    light.setPosition(hit.transform, hit.point, hit.normal);
                }

                // don't draw snap lines if object wasn't snapped very far
                Vector3 actualMoveDistance = (position - light.getWorldPosition());
                if (Mathf.Abs(actualMoveDistance.x) < 0.0001f) {
                    snappedToX = null;
                }
                if (Mathf.Abs(actualMoveDistance.y) < 0.0001f) {
                    snappedToY = null;
                }
                if (Mathf.Abs(actualMoveDistance.z) < 0.0001f) {
                    snappedToZ = null;
                }
            }
        }


        /// <summary>
        /// Draws Loop in GUI LightSequence Editor Window
        /// </summary>
        private void drawLoop(LightSequenceLoop loop, int index) {
            GUILayout.BeginHorizontal();
            bool newEnabled = GUILayout.Toggle(loop.enabled, GUIContent.none, GUILayout.ExpandWidth(false));
            if (newEnabled != loop.enabled) {
                loop.enabled = newEnabled;
                lightSequence.invalidate();
            }
            GUIStyle headerStyle = new GUIStyle(EditorStyles.foldout);
            headerStyle.fontStyle = FontStyle.Bold;
            string loopLabel = "Loop " + index;
            if (loop.collapsed && !string.IsNullOrEmpty(loop.name)) {
                loopLabel += " (" + loop.name + ")";
            }
            loop.collapsed = !EditorGUILayout.Foldout(!loop.collapsed, loopLabel, true, headerStyle);
            GUILayout.EndHorizontal();
            if (loop.collapsed) {
                return;
            }

            GUILayout.BeginHorizontal();
            if (index == 0) {
                GUILayout.Space(20 + GUI.skin.button.margin.right);
            }
            else if (GUILayout.Button("↑", GUILayout.Width(20))) {
                int newGroupIndex = index - 1;
                lightSequence.loops.RemoveAt(index);
                lightSequence.loops.Insert(newGroupIndex, loop);
                EditorUtility.SetDirty(lightSequence);
            }
            if (index == lightSequence.loops.Count - 1) {
                GUILayout.Space(20 + GUI.skin.button.margin.right);
            }
            else if (GUILayout.Button("↓", GUILayout.Width(20))) {
                int newGroupIndex = index + 1;
                lightSequence.loops.RemoveAt(index);
                lightSequence.loops.Insert(newGroupIndex, loop);
                EditorUtility.SetDirty(lightSequence);
            }
            GUILayout.Label("Name", GUILayout.Width(40));
            loop.name = GUILayout.TextField(loop.name, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 90;
            loop.duration = EditorGUILayout.FloatField("Loop duration", loop.duration, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Calculate", GUILayout.ExpandWidth(false))) {
                loop.duration = 0;
                foreach (LightSequenceGroup group in loop.groups) {
                    foreach (LightSequenceEffect effect in group.effects) {
                        loop.duration = Mathf.Max(loop.duration, effect.endTime);
                    }
                }
            }
        
            GUILayout.Space(40);

            EditorGUIUtility.labelWidth = 70;
            loop.timeOffset = EditorGUILayout.FloatField("Time offset", loop.timeOffset, GUILayout.ExpandWidth(false));
            GUILayout.Space(20);
            loop.timeScale = EditorGUILayout.FloatField("Time scale", loop.timeScale, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Duplicate Loop", GUILayout.ExpandWidth(false))) {
                duplicateLoop(loop);
                lightSequence.invalidate();
                EditorUtility.SetDirty(lightSequence);
                Repaint();
            }
            if (GUILayout.Button("Duplicate Loop (with assigned lights)", GUILayout.ExpandWidth(false))) {
                duplicateLoop(loop, true);
                lightSequence.invalidate();
                EditorUtility.SetDirty(lightSequence);
                Repaint();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Delete Loop", GUILayout.ExpandWidth(false)) && EditorUtility.DisplayDialog("Delete loop", "Do you really want to delete this loop?", "Yes", "Cancel")) {
                Undo.RecordObject(lightSequence, "Delete Loop");
                lightSequence.removeLoop(loop);
                EditorUtility.SetDirty(lightSequence);
                Repaint();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("2. Add LightGroup", GUILayout.Width(150))) {
                LightSequenceGroup group = new LightSequenceGroup();
                lightSequence.groups.Add(group);
                loop.groups.Add(group);
            }
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Add Lights to an active LightGroup by holding LMB and hover over Lights in SceneView _ or set Group active, then with e.g. Line Tool make a Light Array and these gets added to the LightGroup", MessageType.None);
            GUILayout.Space(10);

            //Draw Sqeuencer with the LightGroups + Effects
            Rect rect = EditorGUILayout.BeginVertical();
            drawSequencer(rect, loop);
            EditorGUILayout.EndVertical();

            GUILayout.Space(30);
            Rect dividerRect = rect;
            dividerRect.height = 2;
            dividerRect.y = rect.yMax + 15;
            EditorGUI.DrawRect(dividerRect, Color.black);
        }

        /// <summary>
        /// Draws Sqeuencer in GUI LightSequence Editor Window
        /// </summary>
        private void drawSequencer(Rect rect, LightSequenceLoop loop) {
            for (int i = 0; i < loop.groups.Count; i++) {
                Rect lineRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                drawSequencerGroupLine(lineRect, loop, i);
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draws Sequencer Group Line in GUI LightSequence Editor Window
        /// </summary>
        private void drawSequencerGroupLine(Rect rect, LightSequenceLoop loop, int groupIndex) 
        {
            LightSequenceGroup group = loop.groups[groupIndex];

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            if (group == selectedGroup)
            {
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.normal.textColor = Color.green;
            }

            if (groupIndex == 0)
            {
                GUILayout.Space(20 + buttonStyle.margin.right);
            }
            else if (GUILayout.Button("↑", GUILayout.Width(20)))
            {
                int newGroupIndex = groupIndex - 1;
                loop.groups.RemoveAt(groupIndex);
                loop.groups.Insert(newGroupIndex, group);
                EditorUtility.SetDirty(lightSequence);
            }
            if (groupIndex == loop.groups.Count - 1)
            {
                GUILayout.Space(20 + buttonStyle.margin.right);
            }
            else if (GUILayout.Button("↓", GUILayout.Width(20)))
            {
                int newGroupIndex = groupIndex + 1;
                loop.groups.RemoveAt(groupIndex);
                loop.groups.Insert(newGroupIndex, group);
                EditorUtility.SetDirty(lightSequence);
            }


            if (GUILayout.Button("Group." + groupIndex.ToString(), buttonStyle, GUILayout.Width(65))) {
                if (selectedGroup == group && tool == Tool.PickGroup) {
                    tool = Tool.Place;
                    selectGroup(null);
                    selectedEffect = null;
                }
                else {
                    tool = Tool.PickGroup;
                    selectGroup(group);
                }
                selectedLight = null;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Duplicate", GUILayout.Width(65)))
            {
                LightSequenceGroup duplicatedGroup = duplicateGroup(group);
                loop.groups.Add(duplicatedGroup);
            }
            if (GUILayout.Button("Duplicate with Lights", GUILayout.Width(130)))
            {
                LightSequenceGroup duplicatedGroup = duplicateGroup(group, true);
                loop.groups.Add(duplicatedGroup);
            }
            if (GUILayout.Button("Delete", GUILayout.Width(65)))
            {
                Undo.RecordObject(lightSequence, "Delete Group");
                lightSequence.removeGroup(group);
                if (selectedGroup == group)
                {
                    selectGroup(null);
                }
            }

            GUILayout.Label("lights: " + group.lights.Count.ToString(), GUILayout.Width(70));

            GUILayout.Space(10);

            if (GUILayout.Button("+ Effect", GUILayout.Width(60))) {
                float lastEffectTime = 0;
                foreach (LightSequenceEffect groupEffect in group.effects) {
                    lastEffectTime = Mathf.Max(lastEffectTime, groupEffect.endTime);
                }

                LightSequenceEffect effect = new LightSequenceEffect();
                effect.startTime = lastEffectTime;
                effect.duration = 1;
                group.effects.Add(effect);
                lightSequence.invalidate();
            }

            // Sequencer Line //

            Rect lineAreaRect = EditorGUILayout.BeginHorizontal();
        
            lineAreaRect.height = rect.height;
            lineAreaRect.width = loop.duration * zoomFactor;
        
            // draw an empty label with the length of the sequencer bar to expand scroll view
            GUILayout.Label(GUIContent.none, GUILayout.Width(lineAreaRect.width));
        
            Rect innerRect = lineAreaRect;
            innerRect.height -= 2;
            innerRect.y += 1;
        
            EditorGUI.DrawRect(innerRect, Color.gray);

            if (playing) {
                Rect scrubberRect = lineAreaRect;
                scrubberRect.xMin += loop.time * zoomFactor - 1;
                scrubberRect.y -= 1;
                scrubberRect.height += 2;
                scrubberRect.width = 3;
                EditorGUI.DrawRect(scrubberRect, Color.white);
            }

            foreach (LightSequenceEffect effect in group.effects) {
                Rect effectRect = innerRect;
                effectRect.xMin += effect.startTime * zoomFactor;
                effectRect.width = effect.duration * zoomFactor;

                Rect moveRect = effectRect;
                moveRect.width -= 12;
                moveRect.x += 6;
                EditorGUIUtility.AddCursorRect(moveRect, MouseCursor.MoveArrow);

                Rect resizeLeftRect = effectRect;
                resizeLeftRect.width = 6;
                EditorGUIUtility.AddCursorRect(resizeLeftRect, MouseCursor.ResizeHorizontal);

                Rect resizeRightRect = effectRect;
                resizeRightRect.width = 6;
                resizeRightRect.x = moveRect.xMax;
                EditorGUIUtility.AddCursorRect(resizeRightRect, MouseCursor.ResizeHorizontal);

                if (Event.current.type == EventType.MouseDown) {
                    if (moveRect.Contains(Event.current.mousePosition)) {
                        effectOperation = EffectOperation.Move;
                        draggedEffect = effect;
                    }
                    else if (resizeRightRect.Contains(Event.current.mousePosition)) {
                        effectOperation = EffectOperation.ResizeRight;
                        draggedEffect = effect;
                    }
                    else if (resizeLeftRect.Contains(Event.current.mousePosition)) {
                        effectOperation = EffectOperation.ResizeLeft;
                        draggedEffect = effect;
                    }
                }
                else if (Event.current.type == EventType.MouseUp) {
                    if (effectRect.Contains(Event.current.mousePosition)) {
                        if (!isDraggingEffect) {
                            if (selectedEffect == effect) {
                                selectedEffect = null;
                                selectGroup(null);
                            }
                            else {
                                selectedEffect = effect;
                                selectGroup(group);
                            }
                        }
                        GUIUtility.keyboardControl = 0; // unfocus input fields
                        Repaint();
                    }
                }
                else if (Event.current.type == EventType.MouseDrag) {
                    if (effect == draggedEffect) {
                        selectedEffect = effect;
                        selectedGroup = group;
                        isDraggingEffect = true;
                        float deltaX = (Event.current.mousePosition.x - mouseDownPosition.x) / zoomFactor;
                        //mouseDownPosition = Event.current.mousePosition;
                        if (effectOperation == EffectOperation.Move) {
                            float oldValue = effect.startTime;
                            snapEffect(loop, effect, effect.startTime, ref deltaX);
                            snapEffect(loop, effect, effect.endTime, ref deltaX);
                            effect.startTime += deltaX;
                            effect.startTime = Mathf.Clamp(effect.startTime, 0, loop.duration - effect.duration);
                            mouseDownPosition.x += (effect.startTime - oldValue) * zoomFactor;
                        }
                        else if (effectOperation == EffectOperation.ResizeRight) {
                            float oldValue = effect.duration;
                            snapEffect(loop, effect, effect.endTime, ref deltaX);
                            effect.duration += deltaX;
                            effect.duration = Mathf.Clamp(effect.duration, 0.01f, loop.duration - effect.startTime);
                            mouseDownPosition.x += (effect.duration - oldValue) * zoomFactor;
                        }
                        else if (effectOperation == EffectOperation.ResizeLeft) {
                            float oldValue = effect.startTime;
                            snapEffect(loop, effect, effect.startTime, ref deltaX);
                            float newValue = effect.startTime + deltaX;
                            newValue = Mathf.Clamp(newValue, 0, effect.endTime - 0.01f);
                            float deltaValue = (newValue - oldValue);
                            if (effect.duration - deltaValue < 0.01f) {
                                deltaValue = 0;
                            }
                            else {
                                effect.startTime = newValue;
                                effect.duration -= deltaValue;
                            }
                            mouseDownPosition.x += deltaValue * zoomFactor;
                        }
                        Repaint();
                    }
                }

                Rect fillRect = effectRect;
                if (effect == selectedEffect) {
                    EditorGUI.DrawRect(effectRect, Color.black);
                    fillRect.x += 2;
                    fillRect.y += 2;
                    fillRect.width -= 4;
                    fillRect.height -= 4;
                }

                Color effectColor = effect.getColor(lightSequence);
                EditorGUI.DrawRect(fillRect, effectColor);  //effectColor
                //if (effect.colorType != LightSequenceEffect.ColorType.Fixed) {
                Rect customColorRect = fillRect;
                customColorRect.yMax = customColorRect.yMin + 2;
                EditorGUI.DrawRect(customColorRect, Color.white);
                //}

                if (effect.effectType == LightSequenceEffect.EffectType.Fade) {
                    GUI.enabled = false;
                    for (int i = 0; i <= effect.effectRepetitions; i++) {
                        Rect curveRect = effectRect;
                        curveRect.width /= effect.effectRepetitions + 1;
                        curveRect.x += i * curveRect.width;
                        EditorGUI.CurveField(curveRect, effect.fadeCurve, new Color(1 - effectColor.r, 1 - effectColor.g, 1 - effectColor.b), new Rect(0, 0, 1, 1));
                    }
                    GUI.enabled = true;
                }
                else if (effect.effectType == LightSequenceEffect.EffectType.Twinkle) {
                    Rect labelRect = effectRect;
                    labelRect.x += 4;
                    labelRect.width -= 8;
                    labelRect.y += 2;
                    GUI.BeginGroup(labelRect);
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.fontStyle = FontStyle.Bold;
                    GUI.Label(new Rect(0, 0, 100, 15), effect.effectType.ToString(), style);
                    GUI.EndGroup();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Snap effect
        /// </summary>
        private void snapEffect(LightSequenceLoop loop, LightSequenceEffect effect, float time, ref float deltaX) {
            if (Mathf.Abs(deltaX * zoomFactor) < snapZoneSize) {
                foreach (LightSequenceGroup group in loop.groups) {
                    foreach (LightSequenceEffect otherEffect in group.effects) {
                        if (otherEffect == effect) {
                            continue;
                        }
                        if (Mathf.Abs(otherEffect.startTime - time) * zoomFactor < snapZoneSize) {
                            deltaX = otherEffect.startTime - time;
                        }
                        else if (Mathf.Abs(otherEffect.endTime - time) * zoomFactor < snapZoneSize) {
                            deltaX = otherEffect.endTime - time;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Effect Inspector with Intensity, Start time, Duration, Repitions, etc. on the right area
        /// </summary>
        private void drawEffectInspector(LightSequenceEffect effect) 
        {

            EditorGUILayout.HelpBox("Effect Inspector", MessageType.None);

            if (effect == null) {
                return;
            }

            effect.colorType = (LightSequenceEffect.ColorType)EditorGUILayout.EnumPopup("Color Type", effect.colorType);
            /*
            if (effect.colorType == LightSequenceEffect.ColorType.Fixed) {
                effect.color = EditorGUILayout.ColorField(effect.color);
            }
            */
            effect.intensity = EditorGUILayout.FloatField("Intensity", effect.intensity);
            effect.startTime = EditorGUILayout.FloatField("Start time", effect.startTime);
            effect.duration = EditorGUILayout.FloatField("Duration", effect.duration);
            GUILayout.Label("End time: " + effect.endTime);
            effect.effectType = (LightSequenceEffect.EffectType)EditorGUILayout.EnumPopup("Effect", effect.effectType);

            if (effect.effectType == LightSequenceEffect.EffectType.Fade && (effect.fadeCurve == null || effect.fadeCurve.keys.Length == 0)) {
                effect.fadeCurve = new AnimationCurve{keys = new Keyframe[] {
                    new Keyframe(0, 0, 0, 0), new Keyframe(0.5f, 1, 0, 0), new Keyframe(1f, 0, 0, 0)
                }};
            }

            switch (effect.effectType) {
                case LightSequenceEffect.EffectType.Fade:
                effect.effectRepetitions = EditorGUILayout.IntField("Repetitions", effect.effectRepetitions);
                //effect.fadeCurve = EditorGUILayout.CurveField("Intensity", effect.fadeCurve);		//Hidden, because it gets substituted anyway
                break;
                case LightSequenceEffect.EffectType.Twinkle:
                effect.effectRepetitions = EditorGUILayout.IntField("Repetitions", effect.effectRepetitions);
                break;
            }

            GUILayout.Space(40);
            if (GUILayout.Button("Delete")) {
                foreach (LightSequenceGroup group in lightSequence.groups) {
                    group.effects.Remove(effect);
                }
                lightSequence.invalidate();
                selectedEffect = null;
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(lightSequence);
                lightSequence.invalidate();
            }
        }

        /// <summary>
        /// Select Group
        /// </summary>
        private void selectGroup(LightSequenceGroup group) {
            previouslySelectedGroup = selectedGroup;
            selectedGroup = group;
        }

        /// <summary>
        /// Duplicate Group
        /// </summary>
        private LightSequenceGroup duplicateGroup(LightSequenceGroup group, bool copyAssignedLights = false) {
            LightSequenceGroup duplicatedGroup = new LightSequenceGroup();
            foreach (LightSequenceEffect effect in group.effects) {
                duplicatedGroup.effects.Add(new LightSequenceEffect(effect));
            }
            if (copyAssignedLights)
            {
                foreach (LightSequenceLight light in group.lights)
                {
                    duplicatedGroup.lights.Add(light);
                }
            }
            lightSequence.groups.Add(duplicatedGroup);
            return duplicatedGroup;
        }

        /// <summary>
        /// Duplicate Loop
        /// </summary>
        private LightSequenceLoop duplicateLoop(LightSequenceLoop loop, bool copyAssignedLights = false) {
            LightSequenceLoop duplicatedLoop = new LightSequenceLoop();
            duplicatedLoop.duration = loop.duration;
            duplicatedLoop.timeOffset = loop.timeOffset;
            duplicatedLoop.timeScale = loop.timeScale;
            lightSequence.loops.Add(duplicatedLoop);

            foreach (LightSequenceGroup group in loop.groups) {
                LightSequenceGroup duplicatedGroup = duplicateGroup(group, copyAssignedLights);
                duplicatedLoop.groups.Add(duplicatedGroup);
                /*
                if (copyAssignedLights) {
                    foreach (LightSequenceLight light in group.lights) {    // Moved to duplicateGroup
                        duplicatedGroup.lights.Add(light);
                    }
                }*/
            }

            return duplicatedLoop;
        }

        /// <summary>
        /// Draw Light Handle as solid Disc
        /// </summary>
        private void drawLightHandle(LightSequenceLight light) 
        {
            Vector3 worldPosition = light.getWorldPosition();
            Vector3 viewPos = Camera.current.WorldToViewportPoint(worldPosition);
            if (viewPos.z > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1) {
                Handles.DrawSolidDisc(worldPosition + light.normal * 0.002f, light.normal, lightRadius);
            }
        }

        /// <summary>
        /// Draw Light Handle as solid Disc with Index for Debugging
        /// </summary>
        private void drawLightHandle(LightSequenceLight light, int index)
        {
            Vector3 worldPosition = light.getWorldPosition();
            Vector3 viewPos = Camera.current.WorldToViewportPoint(worldPosition);
            if (viewPos.z > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            {
                Handles.DrawSolidDisc(worldPosition + light.normal * 0.002f, light.normal, lightRadius);

                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    normal = { textColor = new Color(0.9f, 0.02f, 0.02f, 1f) },
                };
                Handles.Label(worldPosition, index.ToString(), style);
            }
        }
    }
}
