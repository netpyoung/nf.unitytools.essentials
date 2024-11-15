using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NF.UnityTools.Essentials.FileRevealer
{
    // ref: https://assetstore.unity.com/packages/tools/utilities/finder-explorer-revealer-74168

    [InitializeOnLoad]
    public static class Tool_FileRevealer
    {
        static GUIContent? _searchIcon;
        static GUIContent? _darkSearchIcon;

        static Tool_FileRevealer()
        {
            LoadIcon();

            EditorApplication.projectWindowItemOnGUI += AddRevealerIcon;
        }

        static void LoadIcon()
        {
            // ref: https://github.com/halak/unity-editor-icons

            _searchIcon = EditorGUIUtility.IconContent("Search Icon");
            _darkSearchIcon = EditorGUIUtility.IconContent("d_Search Icon");
        }

        static void AddRevealerIcon(string guid, Rect rect)
        {
            bool isHover = rect.Contains(Event.current.mousePosition) && Tool_FileRevealerSetting.ShowOnHover;
            bool isSelected = IsSelected(guid) && Tool_FileRevealerSetting.ShowOnSelected;

            bool isVisible = isHover || isSelected;

            if (!isVisible)
            {
                return;
            }

            float iconSize = EditorGUIUtility.singleLineHeight;
            const int Offset = 1;
            Rect iconRect = new Rect(
                rect.width + rect.x - iconSize - Tool_FileRevealerSetting.OffsetInProjectView,
                rect.y,
                iconSize - Offset,
                iconSize - Offset
            );

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (GUI.Button(iconRect, GetIcon(), GUIStyle.none))
            {
                EditorUtility.RevealInFinder(path);
            }

            EditorApplication.RepaintProjectWindow();
        }

        static GUIContent? GetIcon()
        {
            if (_darkSearchIcon == null || _searchIcon == null)
            {
                LoadIcon();
            }

            return EditorGUIUtility.isProSkin ? _darkSearchIcon : _searchIcon;
        }

        static bool IsSelected(string guid)
        {
            return Selection.assetGUIDs.Any(guid.Contains);
        }
    }
}