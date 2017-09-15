using System;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Window for initiating a new project.
    /// </summary>
    /// <seealso cref="T:UnityEditor.EditorWindow" />
    class NewProjectWindow : EditorWindow
    {
        /// <summary>
        /// The name of the project.
        /// </summary>
        private string _name = "My Asset Pack";

        /// <summary>
        /// The error message to show if something went wrong with creating.
        /// </summary>
        private string _errorMsg;

        /// <summary>
        /// Show the NewProjectWindow.
        /// </summary>
        public new static void Show()
        {
            var window = CreateInstance<NewProjectWindow>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 50);
            window.ShowUtility();
        }
        
        public void OnGUI()
        {
            _name = EditorGUILayout.TextField("Project Name", _name);

            if (!string.IsNullOrEmpty(_errorMsg))
            {
                EditorGUILayout.HelpBox(_errorMsg, MessageType.Error);
            }

            if (GUILayout.Button("Create"))
            {
                CreateProject();
            }
        }

        /// <summary>
        /// Tries to create the project with name from <see cref="_name"/>.
        /// </summary>
        private void CreateProject()
        {
            try
            {
                ProjectManager.Init(_name);

                Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                _errorMsg = e.Message;
            }
        }
    }
}
