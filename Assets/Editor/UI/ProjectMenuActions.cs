using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Adds the menu actions in the unity editor for managing projects.
    /// </summary>
    /// <seealso cref="T:UnityEngine.MonoBehaviour" />
    class ProjectMenuActions : MonoBehaviour
    {
        [MenuItem("Parkitect/New Project")]
        public static void NewProject()
        {
            NewProjectWindow.Show();
        }

        [MenuItem("Parkitect/Open Project")]
        public static void OpenProject()
        {
            LoadProjectWindow.Show();
        }

        [MenuItem("Parkitect/Save Project")]
        public static void SaveProject()
        {
            ProjectManager.Save();
        }

        [MenuItem("Parkitect/Close Project")]
        public static void CloseProject()
        {
            CloseProjectWindow.Show();
        }

        [MenuItem("Parkitect/Save Project", true)]
        public static bool SaveProjectValidate()
        {
            return ProjectManager.Initialized;
        }

        [MenuItem("Parkitect/Close Project", true)]
        public static bool CloseProjectValidate()
        {
            return ProjectManager.Initialized;
        }
    }
}
