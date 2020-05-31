using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ParkitectAssetEditor.Compression;
using ParkitectAssetEditor.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public static bool Initialized
        {
            get { return Project != null; }
        }

        private static string _autoSaveHash = "";

        /// <summary>
        /// Saves the project.
        /// </summary>
        public static bool Save()
        {
            var path = Path.Combine(Project.Value.ProjectDirectory, Project.Value.ProjectFile);

            Debug.Log(string.Format("Starting saving project {0}", path));

            Directory.CreateDirectory(Project.Value.ModDirectory);

            if (AssetPack.Assets.Count == 0)
            {
                Debug.Log("There are no defined assets in the Asset Pack, can't save");
                return false;
            }

            string output = JsonConvert.SerializeObject(AssetPack);

            File.WriteAllText(path, output);

            Debug.Log(string.Format("Finished saving project {0}", path));

            return true;
        }

        /// <summary>
        /// Saves and exports this project.
        /// </summary>
        /// <param name="exportAssetZip">If the assets folder should be exported as an archive with the pack.</param>
        public static bool Export(bool exportAssetZip)
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            var path = Path.Combine(Project.Value.ProjectDirectory, Project.Value.ProjectFile);

            if (Save() && AssetPack.CreateAssetBundle())
            {
                File.Copy(path, Path.Combine(Project.Value.ModDirectory, Project.Value.ProjectFile), true);

                var assetZipPath = Path.Combine(Project.Value.ModDirectory, "assets.zip");

                // Always delete the old file. It will get recreated if the user checked the checkbox.
                if (File.Exists(assetZipPath))
                {
                    File.Delete(assetZipPath);
                }

                if (exportAssetZip)
                {
                    Debug.Log(string.Format("Archiving {0} to {1}", Project.Value.ProjectDirectory, assetZipPath));

                    ArchiveHelper.CreateZip(assetZipPath, Project.Value.ProjectDirectory);
                }

                var previewImagePath = Path.Combine(Project.Value.ProjectDirectory, "Resources/preview.png");
                File.Copy(previewImagePath, Path.Combine(Project.Value.ModDirectory, "preview.png"), true);

                return true;
            }

            Debug.LogWarning(string.Format("Failed saving project {0}", path));

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

            Debug.Log(string.Format("Start loading project {0}", path));

            Project = new Project
            {
                ProjectName = Path.GetFileNameWithoutExtension(path),
                ProjectDirectory = Path.GetDirectoryName(path),
                ModDirectory = Path.Combine(Utility.Utility.ParkitectModPath, Path.GetFileNameWithoutExtension(path)),
                ProjectFile = Path.GetFileName(path),
                ProjectFileAutoSave = Path.GetFileName(path) + ".autosave"
            };

            AssetPack = JsonConvert.DeserializeObject<AssetPack>(File.ReadAllText(path));

            AssetPack.LoadGameObjects();
            AssetPack.InitAssetsInScene();

            EditorPrefs.SetString("loadedProject", string.Format("{0}.autosave", Project.Value.ProjectFile));


            AssetEditorWindow.ShowWindow();

            Debug.Log(string.Format("Finished loading project {0}", path));
        }

        /// <summary>
        /// Auto saves the project if something has changed. Save path is the project path + .autosave
        /// </summary>
        /// <remarks>
        /// Does NOT export to an assetbundle.
        /// </remarks>
        public static void AutoSave()
        {
            var path = EditorPrefs.GetString("loadedProject");

            string output = JsonConvert.SerializeObject(AssetPack);

            using (var md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(output));
                var hash = string.Join("", md5.Hash.Select(b => b.ToString("x2")).ToArray());

                if (hash != _autoSaveHash)
                {
                    Debug.Log(string.Format("Starting auto saving project {0}", path));

                    File.WriteAllText(path, output);

                    _autoSaveHash = hash;

                    Debug.Log(string.Format("Finished auto saving project {0}", path));
                }
            }

            EditorPrefs.SetString("loadedProject", Path.Combine(Project.Value.ProjectDirectory, Project.Value.ProjectFileAutoSave));
        }

        /// <summary>
        /// Auto loads the project that was closed by an unity recompile.
        /// </summary>
        public static void AutoLoad()
        {
            var path = EditorPrefs.GetString("loadedProject");

            // .autosave = 9 characters, them hacks!
            var pathWithoutAutoSave = path.Remove(path.Length - 9);

            Debug.Log(string.Format("Start auto loading project {0}", path));

            Project = new Project
            {
                ProjectName = Path.GetFileNameWithoutExtension(pathWithoutAutoSave),
                ProjectDirectory = Path.GetDirectoryName(pathWithoutAutoSave),
                ModDirectory = Path.Combine(Utility.Utility.ParkitectModPath, Path.GetFileNameWithoutExtension(pathWithoutAutoSave)),
                ProjectFile = Path.GetFileName(pathWithoutAutoSave),
                ProjectFileAutoSave = Path.GetFileName(pathWithoutAutoSave) + ".autosave"
            };

            AssetPack = JsonConvert.DeserializeObject<AssetPack>(File.ReadAllText(path));

            EditorPrefs.SetString("loadedProject", Path.Combine(Project.Value.ProjectDirectory, Project.Value.ProjectFileAutoSave));

            Debug.Log(string.Format("Finished auto loading project {0}", path));
        }

        /// <summary>
        /// Closes the loaded project without saving.
        /// </summary>
        public static void Close()
        {
            AssetPack.RemoveAssetsFromScene();

            AssetPack = null;
            Project = null;

            EditorPrefs.SetString("loadedProject", null);
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

            var projectDirectory = Application.dataPath;
            var modDirectory = Path.Combine(Utility.Utility.ParkitectModPath, name);
            var projectFile = string.Format("{0}.assetProject", name);
            var projectFileAutoSave = string.Format("{0}.autosave", projectFile);

            var projectFilePath = Path.Combine(projectDirectory, projectFile);
            if (File.Exists(projectFilePath))
            {
                throw new ProjectAlreadyExistsException(string.Format("There already is a project at {0}", projectFilePath));
            }

            if (Directory.Exists(modDirectory))
            {
                throw new ProjectAlreadyExistsException(string.Format("Your Parkitect installation already has a mod called {0} at {1}", name, modDirectory));
            }

            Project = new Project
            {
                ProjectName = name,
                ProjectDirectory = projectDirectory,
                ModDirectory = modDirectory,
                ProjectFile = projectFile,
                ProjectFileAutoSave = projectFileAutoSave
            };

            AssetPack = new AssetPack
            {
                Name = name,
				Description = "An asset pack"
            };

            EditorPrefs.SetString("loadedProject", Path.Combine(Project.Value.ProjectDirectory, Project.Value.ProjectFileAutoSave));
        }
    }
}
