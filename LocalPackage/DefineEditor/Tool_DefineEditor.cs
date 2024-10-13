using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NF.UnityTools.Essentials.DefineManagement
{
    /*
        public enum E_DEFINE
        {
            A,
            B,
            C,
        }

        [InitializeOnLoad]
        public sealed class Tool_DefineEditor : NF.UnityTools.Essentials.DefineManagement.Tool_DefineEditor<E_DEFINE>
        {
            static Tool_DefineEditor()
            {
                var pre_defines = Enum.GetNames(typeof(E_DEFINE)).ToList();
                Init(pre_defines);
            }

            [MenuItem("@Tool/Tool_DefineEditor")]
            private static void OpenScriptDefines()
            {
                OpenWindow<Tool_DefineEditor>();
            }
        }
        */
    public class Tool_DefineEditor<T> : EditorWindow where T : Enum
    {
        private static List<string> mPreDefines = new List<string>();

        private bool mIsChanged;
        private int mCurSelectedIndex;

        private readonly HashSet<string> mDefines = new HashSet<string>();
        private readonly List<string> mErrors = new List<string>();

        private string mFilterStr = string.Empty;
        private bool mIsLoaded;
        private Vector2 mScrollPos;

        public static void OpenWindow<TSelf>() where TSelf : Tool_DefineEditor<T>
        {
            GetWindow<TSelf>(true);
        }

        public static void Init(List<string> pre_defines)
        {
            mPreDefines = pre_defines;
        }

        public static void ApplyDefines(params T[] defines)
        {
            string defs = string.Join(";", defines.Select(x => x.ToString()).ToArray());

            foreach (BuildTargetGroup type in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (type == BuildTargetGroup.Unknown)
                {
                    continue;
                }
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(type), defs);
            }
        }

        private void OnEnable()
        {
            this.titleContent = new GUIContent("Script Defines");

            if (!this.mIsLoaded)
            {
                this.mIsLoaded = true;
                Reload();
            }

            Repaint();
        }

        private string[] GetFilteredDefines(List<string> defines, string filter_str)
        {
            if (string.IsNullOrEmpty(filter_str))
            {
                return defines.ToArray();
            }

            return defines.FindAll(x => { return x.Contains(filter_str); }).ToArray();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorApplication.isCompiling ? "Compiling..." : " ");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reload"))
            {
                Reload();
            }

            GUI.enabled = this.mIsChanged && this.mErrors.Count == 0 && !EditorApplication.isCompiling;

            if (GUILayout.Button("Apply"))
            {
                Apply();
                Reload();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            this.mScrollPos = EditorGUILayout.BeginScrollView(this.mScrollPos);

            bool changed = false;
            EditorGUI.BeginChangeCheck();
            List<string> buffer = this.mDefines.ToList();

            for (int i = 0; i < buffer.Count; ++i)
            {
                string define = buffer.ElementAt(i);
                GUILayout.BeginHorizontal();
                GUILayout.Label(define);

                if (GUILayout.Button("X"))
                {
                    this.mDefines.Remove(define);
                }

                GUILayout.EndHorizontal();
            }

            changed |= EditorGUI.EndChangeCheck();

            GUILayout.Space(24);
            string[] filtered_defines = GetFilteredDefines(mPreDefines, this.mFilterStr);

            this.mCurSelectedIndex = EditorGUILayout.Popup(this.mCurSelectedIndex, filtered_defines.ToArray());

            this.mFilterStr = EditorGUILayout.TextField(this.mFilterStr);

            if (filtered_defines.Length != 0)
            {
                if (GUILayout.Button("Add"))
                {
                    this.mDefines.Add(filtered_defines.ElementAt(this.mCurSelectedIndex));
                    changed = true;
                }
            }

            GUI.enabled = true;

            GUILayout.Space(50);
            EditorGUILayout.EndScrollView();
            GUILayout.Label("Project Settings > Player > Other Settings > Configuration > Script Define Symbols");

            if (this.mErrors.Count != 0)
            {
                GUILayout.Label("ERRORS:");
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
                errorStyle.normal.textColor = Color.red;

                for (int i = 0; i < this.mErrors.Count; i++)
                {
                    string error = this.mErrors[i];
                    GUILayout.Label(error, errorStyle);
                }
            }

            //		if(changed)
            //		{
            //			TestForErrors();
            //		}
            this.mIsChanged |= changed;
        }

        private void TestForErrors()
        {
            Regex containsInvalidRegex = new Regex("^[a-zA-Z0-9_]*$");
            Regex isNumberRegex = new Regex("^[0-9]*$");

            bool containsInvalid = false;
            bool isFirstNumber = false;
            bool isEmptyString = false;

            foreach (string define in this.mDefines)
            {
                if (define.Length == 0)
                {
                    isEmptyString = true;
                }

                if (!containsInvalidRegex.IsMatch(define))
                {
                    containsInvalid = true;
                }

                if (define.Length != 0)
                {
                    if (isNumberRegex.IsMatch(define[0].ToString()))
                    {
                        isFirstNumber = true;
                    }
                }
            }

            this.mErrors.Clear();

            if (containsInvalid)
            {
                this.mErrors.Add("A script define contains invalid characters.");
            }

            if (isEmptyString)
            {
                this.mErrors.Add("A script define is empty.");
            }

            if (isFirstNumber)
            {
                this.mErrors.Add("A number cannot be used as the first character of a define.");
            }
        }

        private void Reload()
        {
            this.mIsChanged = false;
            this.mDefines.Clear();

            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));

            foreach (string define in defines.Split(';'))
            {
                if (string.IsNullOrEmpty(define))
                {
                    continue;
                }

                this.mDefines.Add(define);
            }

            EditorGUIUtility.editingTextField = false;
        }

        private void Apply()
        {
            string defines = string.Empty;

            if (this.mDefines.Count == 0)
            {
            }
            else if (this.mDefines.Count == 1)
            {
                defines = this.mDefines.First();
            }
            else
            {
                defines = this.mDefines.Aggregate((cur, nxt) => cur + ";" + nxt);
            }

            foreach (BuildTargetGroup type in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (type == BuildTargetGroup.Unknown)
                {
                    continue;
                }
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(type), defines);
            }
        }
    }
}