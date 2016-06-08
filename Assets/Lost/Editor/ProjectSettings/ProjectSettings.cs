﻿//-----------------------------------------------------------------------
// <copyright file="ProjectSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.VersionControl;
    using UnityEngine;

    public static class ProjectSettings
    {
        private static readonly string lostSettingsFile = "./Assets/lost.json";
        private static ProjectSettingsData data;

        public static ProjectSettingsData Data
        {
            get
            {
                if (File.Exists(lostSettingsFile))
                {
                    data = JsonUtility.FromJson<ProjectSettingsData>(File.ReadAllText(lostSettingsFile));
                }

                // if it's still null (serialization error), then make a new one
                if (data == null)
                {
                    data = new ProjectSettingsData();
                    var json = JsonUtility.ToJson(data, true);
                    CreateFile(json, lostSettingsFile, true);
                }

                return data;
            }
        }

        #if !UNITY_CLOUD_BUILD
        [InitializeOnLoadMethod]
        public static void InitializeProjectSettings()
        {
            GenerateP4IgnoreIfDoesNotExist();
            SetP4IgnoreFile();
            OverrideTemplateCSharpFiles();
        }
        #endif

        [MenuItem("Lost/Tools/Generate P4Ignore file")]
        public static void GenerateP4IgnoreIfDoesNotExist()
        {
            if (Data.GenerateP4IgnoreIfDoesNotExist && File.Exists(Data.P4IgnoreFileName) == false)
            {
                if (File.Exists(Data.TemplateP4IgnoreLocation))
                {
                    Logger.LogWarning("Generating P4Ignore File {0}.  Make sure to check it in!", Data.P4IgnoreFileName);
                    CreateFile(File.ReadAllText(Data.TemplateP4IgnoreLocation), Data.P4IgnoreFileName, true);
                }
                else
                {
                    Logger.LogError("Unable to create file {0}.  Template P4Ignore location does not exist!", Data.P4IgnoreFileName);
                }
            }
        }

        [MenuItem("Lost/Tools/Set P4Ignore File")]
        public static void SetP4IgnoreFile()
        {
            if (Data.SetP4IgnoreFlagAtStartup == false)
            {
                return;
            }

            string p4IngnoreEditorPref = "P4IgnoreFileName";
            var currentP4IgnoreFileName = EditorPrefs.GetString(p4IngnoreEditorPref, null);

            if (currentP4IgnoreFileName != Data.P4IgnoreFileName)
            {
                EditorPrefs.SetString(p4IngnoreEditorPref, Data.P4IgnoreFileName);

                Logger.LogInfo("Setting the P4IGNORE environment variable to {0}", Data.P4IgnoreFileName);
                
                #if UNITY_EDITOR_WIN

                System.Diagnostics.Process.Start("p4", "set P4IGNORE=" + Data.P4IgnoreFileName);
                
                #elif UNITY_EDITOR_OSX
                
                System.Diagnostics.Process.Start("launchctl", "setenv P4IGNORE " + Data.P4IgnoreFileName);
                
                #endif
            }
        }

        [MenuItem("Lost/Tools/Override Template C# Files")]
        public static void OverrideTemplateCSharpFiles()
        {
            if (Data.OverrideTemplateCShardFiles == false)
            {
                return;
            }
            
            #if UNITY_EDITOR_WIN
            
            string unityDirectory = Path.GetDirectoryName(System.Environment.CommandLine);
            string templateFilesDirectory = Path.Combine(unityDirectory, "Data/Resources/ScriptTemplates");

            #elif UNITY_EDITOR_OSX
            
            string unityDirectory = Path.GetDirectoryName(System.Environment.CommandLine);
            unityDirectory = Path.GetDirectoryName(unityDirectory);  // going back one more directory
            string templateFilesDirectory = Path.Combine(unityDirectory, "Resources/ScriptTemplates");
            
            #endif
            
            if (Directory.Exists(templateFilesDirectory) == false)
            {
                Logger.LogError("Unable to override template files, couldn't find unity template directory.");
                return;
            }

            try
            {
                CopyFile(Data.TemplateNewBehaviourLocation, Path.Combine(templateFilesDirectory, "81-C# Script-NewBehaviourScript.cs.txt"), false);
                CopyFile(Data.TemplateNewEditorTestLocation, Path.Combine(templateFilesDirectory, "83-Editor Test C# Script-NewEditorTest.cs.txt"), false);
                CopyFile(Data.TemplateNewStatMachineLocation, Path.Combine(templateFilesDirectory, "86-C# Script-NewStateMachineBehaviourScript.cs.txt"), false);
                CopyFile(Data.TemplateNewSubStateMachineLocation, Path.Combine(templateFilesDirectory, "86-C# Script-NewSubStateMachineBehaviourScript.cs.txt"), false);
            }
            catch
            {
                Logger.LogError("Unable to overwrite template files!  Try running Unity in Administrator mode.");
            }
        }

        [MenuItem("Lost/Tools/Convert all C# files to project line endings")]
        public static void ConvertAllCSharpFileLineEndings()
        {
            foreach (string file in Directory.GetFiles(".", "*.cs", SearchOption.AllDirectories))
            {
                // removing the first 2 characters "./" so it's a relative path from the project folder
                string fileName = file.Substring(2);

                CopyFile(fileName, fileName, true);
            }
        }

        public static void CreateFile(string contents, string destinationFile, bool sourceControlAdd)
        {
            string fileContents = ConvertLineEndings(contents);

            // actually writing out the contents
            File.WriteAllText(destinationFile, fileContents);

            // telling source control to add the file
            if (sourceControlAdd && Provider.enabled && Provider.isActive)
            {
                AssetDatabase.Refresh();
                Provider.Add(new Asset(destinationFile), false).Wait();
            }
        }

        public static void CopyFile(string sourceFile, string destinationFile, bool sourceControlCheckout)
        {
            if (File.Exists(sourceFile) == false)
            {
                Logger.LogError("Unable to copy file {0} to {1}.  Source file does not exist!", sourceFile, destinationFile);
            }

            string fileContents = ConvertLineEndings(File.ReadAllText(sourceFile));

            if (fileContents != File.ReadAllText(destinationFile))
            {
                // checking out the file
                if (sourceControlCheckout && Provider.enabled && Provider.isActive)
                {
                    Provider.Checkout(destinationFile, CheckoutMode.Asset).Wait();
                }
                
                // actually writing out the contents
                File.WriteAllText(destinationFile, fileContents);
            }
        }

        public static string ConvertLineEndings(string inputText)
        {
            // checking for a really messed up situtation that happens when mixing max/pc sometimes
            if (inputText.Contains("\r\r\n"))
            {
                inputText = inputText.Replace("\r\r\n", "\n");
            }
            
            //  if it has windows line escaping, then convert everything to Unix
            if (inputText.Contains("\r\n"))
            {
                inputText = inputText.Replace("\r\n", "\n");
            }

            if (Data.ProjectLineEndings == LineEndings.Windows)
            {
                // convert all unix to windows
                inputText = inputText.Replace("\n", "\r\n");
            }
            else if (Data.ProjectLineEndings == LineEndings.Unix)
            {
                // do nothing, already in Unix
            }
            else
            {
                Logger.LogError("Unable to convert line endings, unknown line ending type found: " + LineEndings.Unix);
            }

            return inputText;
        }
    }
}
