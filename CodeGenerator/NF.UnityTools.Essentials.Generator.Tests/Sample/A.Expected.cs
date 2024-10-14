#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace NF.UnityTools.Essentials.DefineManagement.Tool_DefineEditor.Generated
{
    [InitializeOnLoad]
    public sealed class Tool_DefineEditor : NF.UnityTools.Essentials.DefineManagement.Tool_DefineEditor<E_ENUM>
    {
        static Tool_DefineEditor()
        {
            List<string> pre_defines = Enum.GetNames(typeof(E_ENUM)).ToList();
            Init(pre_defines);
        }

        [MenuItem("@Tool/Tool_DefineEditor")]
        private static void OpenScriptDefines()
        {
            OpenWindow<Tool_DefineEditor>();
        }
    }
}
#endif // UNITY_EDITOR