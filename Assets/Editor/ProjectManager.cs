using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CSharp;
using Newtonsoft.Json;
using ParkitectAssetEditor.Compression;
using ParkitectAssetEditor.UI;
using ParkitectAssetEditor.Utility;
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

        public static bool UpdateProjectSetup()
        {

            String projectPath = Path.Combine(Application.dataPath.Replace("/Assets", ""),"ParkitectMod");

            if (!Directory.Exists(projectPath)) {
                Directory.CreateDirectory(projectPath);
                File.WriteAllText(Path.Combine(projectPath, "Main.cs"), ProjectDocument.MainCS);
            }

            XmlDocument csProj = new XmlDocument();
            csProj.LoadXml(ProjectDocument.CSProj.Replace("${RootNamespace}", "ParkitectMod")
                .Replace("${AssemblyName}","ParkitectMod")
                .Replace("${OutputPath}", ProjectManager.Project.Value.ModDirectory));

            var manager = new XmlNamespaceManager(csProj.NameTable);
            manager.AddNamespace("x", ProjectDocument.DefaultCsProjNamespace);

            var project = csProj.SelectNodes("//x:Project", manager).Cast<XmlNode>().First();

            XmlElement itemGroup = csProj.CreateElement("ItemGroup",ProjectDocument.DefaultCsProjNamespace);
            project.AppendChild(itemGroup);

            foreach (var assmb in AssetPack.ProjectAssemblies)
            {
                XmlElement reff = csProj.CreateElement("Reference",ProjectDocument.DefaultCsProjNamespace);
                var refrenceAttribute = csProj.CreateAttribute("Include");
                refrenceAttribute.InnerText = assmb;
                reff.Attributes.Append(refrenceAttribute);

                var prv = csProj.CreateElement("Private",ProjectDocument.DefaultCsProjNamespace);
                prv.InnerText = "False";
                reff.AppendChild(prv);
                if (!string.IsNullOrEmpty(AssetPack.ParkitectPath))
                {
                    var hint = csProj.CreateElement("HintPath",ProjectDocument.DefaultCsProjNamespace);
                    hint.InnerText = Path.Combine(Path.Combine(AssetPack.ParkitectPath, "Parkitect_Data/Managed/"), assmb + ".dll");
                    reff.AppendChild(hint);
                }

                itemGroup.AppendChild(reff);
            }

            XmlElement sourceGroup = csProj.CreateElement("ItemGroup",ProjectDocument.DefaultCsProjNamespace);
            project.AppendChild(sourceGroup);

            foreach (var ff in Directory.GetFiles(projectPath,"*.*", SearchOption.AllDirectories))
            {
                if (ff.EndsWith(".cs"))
                {
                    String fil = ff.Substring(projectPath.Length + 1);
                    XmlElement comp = csProj.CreateElement("Compile",ProjectDocument.DefaultCsProjNamespace);
                    var refrenceAttribute = csProj.CreateAttribute("Include");
                    refrenceAttribute.InnerText = fil;
                    comp.Attributes.Append(refrenceAttribute);

                    sourceGroup.AppendChild(comp);

                }
            }

            csProj.Save(Path.Combine(projectPath,"ParkitectMod.csproj"));

            return true;
        }

        /// <summary>
        /// Saves and exports this project.
        /// </summary>
        /// <param name="exportAssetZip">If the assets folder should be exported as an archive with the pack.</param>
        public static bool Export(bool exportAssetZip)
        {
            AssetPack.BuildGuid = GUID.Generate().ToString();
            
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            var path = Path.Combine(Project.Value.ProjectDirectory, Project.Value.ProjectFile);

            String projectPath = Path.Combine(Application.dataPath.Replace("/Assets", ""),"ParkitectMod");
            if (Save() && AssetPack.CreateAssetBundle())
            {
                if (Directory.Exists(projectPath))
                {
                    AssetPack.CompileModule(projectPath);
                }

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
