using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace NF.UnityTools.Essentials.InitialSettingEditor
{
    public class GitignoreDownloader : EditorWindow
    {
        private bool _isOverride;

        [MenuItem("@Tool/InitialSetting/Download Unity Gitignore")]
        public static void ShowWindow()
        {
            GetWindow<GitignoreDownloader>(nameof(GitignoreDownloader));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Download .gitignore"))
            {
                _ = DownloadGitignore(_isOverride);
            }

            _isOverride = EditorGUILayout.Toggle("override?", _isOverride);
            EditorGUILayout.EndHorizontal();
        }

        private async Task DownloadGitignore(bool isOverride)
        {
            string savePath = Path.Combine(Application.dataPath, "../.gitignore");
            if (!isOverride)
            {
                if (File.Exists(savePath))
                {
                    Debug.LogError($".gitignore exist");
                    return;
                }
            }

            string url = "https://raw.githubusercontent.com/github/gitignore/main/Unity.gitignore";
            try
            {
                (bool isSuccess, string errStr) = await DownloadFileAsync(url, savePath);
                if (isSuccess)
                {
                    Debug.Log($"Successfully downloaded .gitignore to: {savePath}");
                }
                else
                {
                    Debug.LogError($"errStr : {errStr}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download .gitignore: {e.Message}");
            }
        }

        private async Task<(bool isSuccess, string errStr)> DownloadFileAsync(string url, string savePath)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerFile(savePath);

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Delay(100);
                    EditorUtility.DisplayProgressBar("Downloading .gitignore",
                        "Progress: " + (operation.progress * 100f).ToString("F0") + "%",
                        operation.progress);
                }

                EditorUtility.ClearProgressBar();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return (true, string.Empty);
                }

                return (false, request.error);
            }
        }
    }
}