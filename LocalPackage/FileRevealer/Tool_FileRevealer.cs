using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NF.UnityTools.Essentials.FileRevealer
{
    // ref: https://assetstore.unity.com/packages/tools/utilities/finder-explorer-revealer-74168

    [InitializeOnLoad]
    public static class Tool_FileRevealer
    {
        static GUIContent mSearchIcon;
        static GUIContent mDarkSearchIcon;

        static Tool_FileRevealer()
        {
            LoadIcon();

            EditorApplication.projectWindowItemOnGUI += AddRevealerIcon;
        }

        static void LoadIcon()
        {
            // ref: https://github.com/halak/unity-editor-icons

            mSearchIcon = EditorGUIUtility.IconContent("Search Icon");
            mDarkSearchIcon = EditorGUIUtility.IconContent("d_Search Icon");
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

        static GUIContent GetIcon()
        {
            if (mDarkSearchIcon == null || mSearchIcon == null)
            {
                LoadIcon();
            }

            return EditorGUIUtility.isProSkin ? mDarkSearchIcon : mSearchIcon; ;
        }

        static bool IsSelected(string guid)
        {
            return Selection.assetGUIDs.Any(guid.Contains);
        }
    }
}