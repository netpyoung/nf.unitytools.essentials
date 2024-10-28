using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NF.UnityTools.Essentials.PathEditor
{
    public sealed class Tool_PathEditor : EditorWindow
    {
        [MenuItem("@Tool/Tool_PathEditor")]
        public static void ShowExample()
        {
            Tool_PathEditor wnd = GetWindow<Tool_PathEditor>();
            wnd.titleContent = new GUIContent(nameof(Tool_PathEditor));
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Application.dataPath"))
            {
                Debug.Log(Application.dataPath);
                EditorUtility.RevealInFinder(Application.dataPath);
            }

            if (GUILayout.Button("Application.persistentDataPath"))
            {
                Debug.Log(Application.persistentDataPath);
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }

            if (GUILayout.Button("Application.streamingAssetsPath"))
            {
                Debug.Log(Application.streamingAssetsPath);
                EditorUtility.RevealInFinder(Application.streamingAssetsPath);
            }

            if (GUILayout.Button("Application.temporaryCachePath"))
            {
                Debug.Log(Application.temporaryCachePath);
                EditorUtility.RevealInFinder(Application.temporaryCachePath);
            }

            if (GUILayout.Button("Application.consoleLogPath"))
            {
                //#if UNITY_EDITOR_WIN
                //                string dir = $"{Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%")}\\AppData\\Local\\Unity\\Editor";
                //#else
                //                string dir = $"{Environment.GetEnvironmentVariable("HOME")}/Library/Logs/Unity";
                //#endif
                Debug.Log(Application.consoleLogPath);
                EditorUtility.RevealInFinder(Application.consoleLogPath);
            }

            if (GUILayout.Button("unitypackage"))
            {
#if UNITY_EDITOR_OSX
                string path = InternalEditorUtility.unityPreferencesFolder + System.IO.Path.DirectorySeparatorChar + "../../../Unity/Asset Store-5.x/";
                path = System.IO.Path.GetFullPath(path);
#else
                string path = InternalEditorUtility.unityPreferencesFolder + System.IO.Path.DirectorySeparatorChar + "../../Asset Store-5.x/";
                path = System.IO.Path.GetFullPath(path);
#endif
                Debug.Log(path);
                EditorUtility.RevealInFinder(path);
            }

            if (GUILayout.Button("nuget"))
            {
#if UNITY_EDITOR_WIN
                string dir = $"{Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%")}\\.nuget\\packages";
#else
                string dir = $"{Environment.GetEnvironmentVariable("HOME")}/.nuget/packages";
#endif
                Debug.Log(dir);
                EditorUtility.RevealInFinder(dir);
            }
        }
    }
}
