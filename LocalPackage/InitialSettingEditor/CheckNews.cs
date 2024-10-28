using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace NF.UnityTools.Essentials.InitialSettingEditor
{
    public class CheckNews : EditorWindow
    {
        private readonly string _unityVersion = Application.unityVersion;

        private readonly string[] _urls = new string[]
        {
            "https://github.com/Unity-Technologies/UnityCsReference",
            "https://github.com/Unity-Technologies/Graphics/tree/master/Packages",
            "https://github.com/needle-mirror/com.unity.scriptablebuildpipeline",
            "https://github.com/needle-mirror/com.unity.entities",
        };

        private static volatile int _lockFetch = 0;
        private const string InitialUrl = "https://symbolserver.unity3d.com/000Admin/history.txt";
        private static string[] _unityVersionStrs;
        private static UnityVersion[] _unityVersions;
        private static SeachableComboBox _seachableComboBox;

        [MenuItem("@Tool/InitialSetting/CheckNews")]
        public static void ShowWindow()
        {
            _lockFetch = 0;
            GetWindow<CheckNews>(nameof(CheckNews));
        }

        private void OnGUI()
        {
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.LabelField(_unityVersion, EditorStyles.boldLabel);

            if (GUILayout.Button("editor/archive"))
            {
                string url = "https://unity.com/releases/editor/archive";
                Debug.Log($"url: {url}");
                Application.OpenURL(url);
            }

            if (GUILayout.Button("whats-new"))
            {
                string version = _unityVersion;
                Match match = Regex.Match(version, @"^(\d+\.\d+\.\d+)");
                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }

                string url = $"https://unity.com/releases/editor/whats-new/{version}";
                Debug.Log($"url: {url}");
                Application.OpenURL(url);
            }

            if (GUILayout.Button("ScriptReference"))
            {
                string version = _unityVersion;
                Match match = Regex.Match(version, @"^(\d+\.\d+)");
                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }

                // 6000.0
                string url = $"https://docs.unity3d.com/{version}/Documentation/ScriptReference/";
                Debug.Log($"url: {url}");
                Application.OpenURL(url);
            }


            foreach (string url in _urls)
            {
                if (GUILayout.Button(ExtractRepoName(url)))
                {
                    Debug.Log($"url: {url}");
                    Application.OpenURL(url);
                }
            }

            using (new EditorGUI.DisabledScope(_lockFetch != 0))
            {
                // UnityEditor.Scripting.ScriptCompilation
                if (GUILayout.Button("versionlist"))
                {
                    _ = FetchAndProcessVersionsAsync();
                }
            }

            if (_seachableComboBox != null)
            {
                _seachableComboBox.OnGUI();
            }
        }

        static string ExtractRepoName(string url)
        {
            Uri uri = new Uri(url);
            string[] segments = uri.Segments.Select(s => s.Trim('/')).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return $"{segments[0]}/{segments[1]}";
        }


        private async Task FetchAndProcessVersionsAsync()
        {
            int lockFetch = Interlocked.Increment(ref _lockFetch);
            if (lockFetch != 1)
            {
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync(InitialUrl);
                    Regex versionPattern = new Regex(@"Unity"",""(\d+\.\d+\.\d+f\d+)""");
                    HashSet<string> versionSet = versionPattern.Matches(content)
                        .Select(m => m.Groups[1].Value).ToHashSet();
                    versionSet.Add(Application.unityVersion);

                    List<UnityVersion> unityVersions = new List<UnityVersion>(versionSet.Count);
                    foreach (string version in versionSet)
                    {
                        if (UnityVersion.TryParse(version, out UnityVersion ver))
                        {
                            unityVersions.Add(ver);
                        }
                    }

                    unityVersions.Sort();
                    _unityVersions = unityVersions.ToArray();
                    _unityVersionStrs = unityVersions.Select(x => x.ToString()).ToArray();
                    _seachableComboBox = new SeachableComboBox(_unityVersionStrs);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Interlocked.Decrement(ref _lockFetch);
            }
        }
    }
}