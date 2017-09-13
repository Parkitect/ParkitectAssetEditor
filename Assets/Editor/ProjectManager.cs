using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ParkitectAssetEditor.UI;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor
{
    /// <summary>
    /// Class for saving/loading projects.
    /// </summary>
    static class ProjectManager
    {
        /// <summary>
        /// Gets the loaded project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public static Project? Project { get; private set; }

        /// <summary>
        /// Gets the asset repo.
        /// </summary>
        /// <value>
        /// The asset repo.
        /// </value>
        public static AssetPack AssetPack { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ProjectManager"/> is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool Initialized => Project != null;
        
        /// <summary>
        /// Saves the project.
        /// </summary>
        public static bool Save()
        {
            if (Initialized)
            {
                return Save(Project.Value.ProjectFile);
            }

            Debug.LogWarning("Project not initialized, can't save.");

            return false;
        }

        /// <summary>
        /// Saves the project to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public static bool Save(string path)
        {
            Debug.Log($"Starting saving project {path}");

            if (AssetPack.Assets.Count == 0)
            {
                Debug.LogError("There are no defined assets in the Asset Pack, can't save");
                return false;
            }

            if (AssetPack.CreateAssetBundle())
            {
                string output = JsonConvert.SerializeObject(AssetPack);

                File.WriteAllText(path, output);

                Debug.Log($"Finished saving project {path}");

                return true;
            }
            
            Debug.LogWarning($"Failed saving project {path}");

            return false;
        }

        /// <summary>
        /// Loads the project from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void Load(string path)
        {
            if (Initialized)
            {
                Close();
            }

            Debug.Log($"Start loading project {path}");

            Project = new Project
            {
                ProjectName = Path.GetFileNameWithoutExtension(path),
                ProjectDirectory = Path.GetDirectoryName(path),
                ProjectFile = path
            };
            
            AssetPack = JsonConvert.DeserializeObject<AssetPack>(File.ReadAllText(path));

            AssetPack.LoadAssetBundle();
            AssetPack.InitAssetsInScene();

            EditorPrefs.SetString("assetPackPath", Project.Value.ProjectFile);
            
            AssetEditorWindow.ShowWindow();

            Debug.Log($"Finished loading project {path}");
        }

        /// <summary>
        /// Closes the loaded project without saving.
        /// </summary>
        public static void Close()
        {
            AssetPack?.RemoveAssetsFromScene();

            AssetPack = null;
            Project = null;
            EditorPrefs.SetString("assetPackPath", null);
        }

        /// <summary>
        /// Initializes a new project with the specified name.
        /// </summary>
        /// <param name="name">The name. <remarks>May only container letters, numbers and spaces</remarks></param>
        /// <exception cref="ProjectAlreadyExistsException">When the directory already exists.</exception>
        public static void Init(string name)
        {
            if (!Regex.IsMatch(name, "^[0-9A-Za-z ]+$"))
            {
                throw new InvalidProjectNameException("Project name may not contain any special characters.");
            }

            var projectDirectory = Path.Combine(Utility.ParkitectModPath, name);
            var projectFile = Path.Combine(projectDirectory, $"{name}.assetProject");

            Project = new Project
            {
                ProjectName = name,
                ProjectDirectory = projectDirectory,
                ProjectFile = projectFile
            };
            
            if (Directory.Exists(projectDirectory))
            {
                throw new ProjectAlreadyExistsException($"There already is a project at {projectDirectory}");
            }

            Directory.CreateDirectory(projectDirectory);

            AssetPack = new AssetPack(name);
        }
    }
}
