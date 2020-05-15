using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.CSharp;
using NUnit.Framework;
using ParkitectAssetEditor.Utility;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor
{
    public static class AssetPackModCompiler
    {
        public static bool CompileModule(this AssetPack assetPack, String path)
        {
            String csprojFile = null;
            foreach (var file in Directory.GetFiles(path))
            {
                if (file.Contains(".csproj"))
                {
                    csprojFile = Path.Combine(path, file);
                    break;
                }
            }

// update system path for mono
#if UNITY_STANDALONE_OSX
            var PATH = System.Environment.GetEnvironmentVariable("PATH");
            var MonoPath = Path.Combine(EditorApplication.applicationContentsPath, "Frameworks/Mono/bin");
            var value = PATH + ":" + MonoPath;
            var target = System.EnvironmentVariableTarget.Process;
            System.Environment.SetEnvironmentVariable("PATH", value, target);
#elif UNITY_STANDALONE_WIN
#else
            var PATH = System.Environment.GetEnvironmentVariable("PATH");
            var MonoPath = Path.Combine(EditorApplication.applicationContentsPath, "Mono/bin");
            var value = PATH + ":" + MonoPath;
            var target = System.EnvironmentVariableTarget.Process;
            Environment.SetEnvironmentVariable("PATH", value, target);
#endif

            if (csprojFile == null) return false;

            XmlDocument document = new XmlDocument();
            document.Load(csprojFile);

            var manager = new XmlNamespaceManager(document.NameTable);
            manager.AddNamespace("x", ProjectDocument.DefaultCsProjNamespace);

            // List the referenced assemblies of the mod.
           List<String> assembleRefrencesInclude = document.SelectNodes("//x:Reference", manager)
                .Cast<XmlNode>()
                .Select(node => node.Attributes["Include"])
                .Select(name => name.Value.Split(',').FirstOrDefault()).ToList();


           List<String> unresolvedSourceFiles = document.SelectNodes("//x:Compile", manager)
               .Cast<XmlNode>()
               .Select(node => node.Attributes["Include"].Value).ToList();

           var assemblyFiles = new List<string>();
           var sourceFiles = new List<string>();
           foreach (var inc in assembleRefrencesInclude)
           {
               if (Utility.Utility.SystemAssemblies.Contains(inc))
               {
                   Debug.Log("Resolved Assembly Reference:" + inc + ".dll");
                   assemblyFiles.Add(inc + ".dll");
               }
               else
               {
                   var pt = Path.Combine(Path.Combine(assetPack.ParkitectPath, "Parkitect_Data/Managed/"),
                       inc + ".dll");
                   Debug.Log("Resolved Assembly Reference:" + pt);
                   assemblyFiles.Add(pt);
               }
           }

           sourceFiles.AddRange(unresolvedSourceFiles.Select(file => Path.Combine(path, file)));

           Debug.Log("Compile using compiler version v4.0");
           var csCodeProvider =
               new CSharpCodeProvider(new Dictionary<string, string>
               {
                   // {"CompilerVersion", "v4.0"}
               });

           var parameters = new CompilerParameters(assemblyFiles.ToArray(), Path.Combine(ProjectManager.Project.Value.ModDirectory,"build.dll"));
            var result = csCodeProvider.CompileAssemblyFromFile(parameters, sourceFiles.ToArray());

            foreach (var o in result.Output)
            {
                Debug.Log(o);
            }
            foreach (var error in result.Errors.Cast<CompilerError>())
            {
                Debug.LogError(error.ErrorNumber + ":" + error.Line + ":" + error.Column + ":" + error.ErrorText + " in " + error.FileName);
            }

            return true;
        }
    }
}
