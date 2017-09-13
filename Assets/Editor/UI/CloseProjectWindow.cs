using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI
{
    /// <summary>
    /// Class for handling close project window
    /// </summary>
    /// <seealso cref="UnityEditor.EditorWindow" />
    class CloseProjectWindow : EditorWindow
    {
        /// <summary>
        /// Show the CloseProjectWindow.
        /// </summary>
        public new static void Show()
        {
            var window = CreateInstance<CloseProjectWindow>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 50);
            window.ShowUtility();
        }

        /// <summary>
        /// Renders the GUI.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Do you want to close the project without saving?", MessageType.Warning);

            if (GUILayout.Button("Save & Close Project"))
            {
                ProjectManager.Save();
                ProjectManager.Close();

                Close();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Close Project"))
            {
                ProjectManager.Close();

                Close();
            }
        }
    }
}
