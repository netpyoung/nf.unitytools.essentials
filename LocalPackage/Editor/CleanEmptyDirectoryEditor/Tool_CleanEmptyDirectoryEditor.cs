using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NF.UnityTools.Essentials.CleanEmptyDirectoryEditor
{
    // ref: https://github.com/muscly/UnityCleanEmptyDirectories

    public sealed class Tool_CleanEmptyDirectoryEditor : EditorWindow
    {
        private const float DIR_LABEL_HEIGHT = 21;

        private List<DirectoryInfo> _emptyDirs = new List<DirectoryInfo>();
        private Vector2 _scrollPosition;
        private bool _lastCleanOnSave;
        private string? _delayedNotiMsg;
        private GUIContent folderContent = null!;

        private bool hasNoEmptyDir
        {
            get { return _emptyDirs == null || _emptyDirs.Count == 0; }
        }

        [MenuItem("@Tool/Tool_CleanEmptyDirectoryEditor")]
        public static void ShowWindow()
        {
            GetWindow<Tool_CleanEmptyDirectoryEditor>(nameof(Tool_CleanEmptyDirectoryEditor));
        }

        private void OnEnable()
        {
            folderContent = EditorGUIUtility.IconContent("Folder Icon");

            _lastCleanOnSave = Core.IsCleanOnSave;
            Core.OnAutoClean += Core_OnAutoClean;
            _delayedNotiMsg = "Click 'Find Empty Dirs' Button.";
        }

        private void OnDisable()
        {
            Core.IsCleanOnSave = _lastCleanOnSave;
            Core.OnAutoClean -= Core_OnAutoClean;
        }

        private void Core_OnAutoClean()
        {
            _delayedNotiMsg = "Cleaned on Save";
        }

        private void OnGUI()
        {
            if (_delayedNotiMsg != null)
            {
                ShowNotification(new GUIContent(_delayedNotiMsg));
                _delayedNotiMsg = null;
            }

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Find Empty Dirs"))
                    {
                        Core.FillEmptyDirList(_emptyDirs);

                        if (hasNoEmptyDir)
                        {
                            ShowNotification(new GUIContent("No Empty Directory"));
                        }
                        else
                        {
                            RemoveNotification();
                        }
                    }

                    if (ColorButton("Delete All", !hasNoEmptyDir, Color.red))
                    {
                        Core.DeleteAllEmptyDirAndMeta(_emptyDirs);
                        _emptyDirs.Clear();
                        ShowNotification(new GUIContent("Deleted All"));
                    }
                }

                EditorGUILayout.EndHorizontal();


                bool cleanOnSave = GUILayout.Toggle(_lastCleanOnSave, " Clean Empty Dirs Automatically On Save");
                if (cleanOnSave != _lastCleanOnSave)
                {
                    _lastCleanOnSave = cleanOnSave;
                    Core.IsCleanOnSave = cleanOnSave;
                }

                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                if (!hasNoEmptyDir)
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            foreach (DirectoryInfo dirInfo in _emptyDirs)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    folderContent.text = Core.GetRelativePath(dirInfo.FullName, Application.dataPath);
                                    GUILayout.Label(folderContent, GUILayout.Height(DIR_LABEL_HEIGHT));
                                    string assetPath = ConvertToAssetPath(dirInfo);
                                    Object? obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                                    EditorGUILayout.ObjectField(obj, typeof(Object), false);
                                }
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private bool ColorButton(string btnTitle, bool enabled, Color color)
        {
            bool oldEnabled = GUI.enabled;
            Color oldColor = GUI.color;

            GUI.enabled = enabled;
            GUI.color = color;

            bool ret = GUILayout.Button(btnTitle);

            GUI.enabled = oldEnabled;
            GUI.color = oldColor;

            return ret;
        }


        public static string ConvertToAssetPath(DirectoryInfo directoryInfo)
        {
            string projectPath = Application.dataPath;
            projectPath = projectPath.Substring(0, projectPath.Length - "Assets".Length);

            string fullPath = directoryInfo.FullName.Replace('\\', '/');

            if (!fullPath.StartsWith(projectPath))
            {
                throw new System.Exception("The provided directory is not inside the Unity project.");
            }

            string relativePath = fullPath.Substring(projectPath.Length);
            return relativePath;
        }
    }

    [InitializeOnLoad]
    public sealed class Core : AssetModificationProcessor
    {
        private const string CLEAN_ON_SAVE_KEY = "k1";
        private static bool _cleanOnSave;

        public static event Action? OnAutoClean;

        public static bool IsCleanOnSave
        {
            get { return EditorPrefs.GetBool(CLEAN_ON_SAVE_KEY, false); }
            set { EditorPrefs.SetBool(CLEAN_ON_SAVE_KEY, value); }
        }

        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (!IsCleanOnSave)
            {
                return paths;
            }

            List<DirectoryInfo> emptyDirs = new List<DirectoryInfo>();
            FillEmptyDirList(emptyDirs);
            if (emptyDirs.Count > 0)
            {
                DeleteAllEmptyDirAndMeta(emptyDirs);

                Debug.Log("[Clean] Cleaned Empty Directories on Save");

                if (OnAutoClean != null)
                {
                    OnAutoClean();
                }
            }

            return paths;
        }

        public static void DeleteAllEmptyDirAndMeta(List<DirectoryInfo> emptyDirs)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (DirectoryInfo dirInfo in emptyDirs)
                {
                    Debug.Log($"dirInfo.FullName: {dirInfo.FullName}");
                    AssetDatabase.MoveAssetToTrash(GetRelativePathFromCd(dirInfo.FullName));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();    
            }
        }

        public static void FillEmptyDirList(List<DirectoryInfo> emptyDirs)
        {
            emptyDirs.Clear();

            DirectoryInfo assetDir = new DirectoryInfo(Application.dataPath);

            WalkDirectoryTree(assetDir, (dirInfo, areSubDirsEmpty) =>
            {
                bool isDirEmpty = areSubDirsEmpty && DirHasNoFile(dirInfo);
                if (isDirEmpty)
                    emptyDirs.Add(dirInfo);

                return isDirEmpty;
            });
        }

        // return: Is this directory empty?
        delegate bool IsEmptyDirectory(DirectoryInfo dirInfo, bool areSubDirsEmpty);

        // return: Is this directory empty?
        static bool WalkDirectoryTree(DirectoryInfo root, IsEmptyDirectory pred)
        {
            DirectoryInfo[] subDirs = root.GetDirectories();

            bool areSubDirsEmpty = true;
            foreach (DirectoryInfo dirInfo in subDirs)
            {
                if (false == WalkDirectoryTree(dirInfo, pred))
                {
                    areSubDirsEmpty = false;
                }
            }

            bool isRootEmpty = pred(root, areSubDirsEmpty);
            return isRootEmpty;
        }

        static bool DirHasNoFile(DirectoryInfo dirInfo)
        {
            FileInfo[]? files = null;

            try
            {
                files = dirInfo.GetFiles("*.*");
                files = files.Where(x => !x.Name.EndsWith(".meta")).ToArray();
            }
            catch
            {
                // ignored
            }

            return files == null || files.Length == 0;
        }

        static string GetRelativePathFromCd(string filespec)
        {
            return GetRelativePath(filespec, Directory.GetCurrentDirectory());
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}