using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NF.UnityTools.Essentials.InitialSettingEditor
{
    
    // ref: https://github.com/Thundernerd/Unity3D-LayersTagsGenerator
    // Inspired by Unity3D-LayersTagsGenerator, but while it uses CodeDom, this implementation utilizes StringBuilder for better maintainability.
    
    // Edit > Project Settings ... > Tags And Layers
    // ProjectSettings/TagManager.asset

    // Tag
    // Layer - https://docs.unity3d.com/ScriptReference/LayerMask.html 
    // Sorted Layer
    // Rendering Layer 

    // SortingLayer.layers
    //tags
    //layers
    //GetLayersWithId
    // TagManager.asset

    public class TagManagerGenerator : EditorWindow
    {
        [MenuItem("@Tool/InitialSetting/Tag Manager Generator")]
        public static void ShowWindow()
        {
            GetWindow<TagManagerGenerator>(nameof(TagManagerGenerator));
        }

        private SerializedObject? _tagManagerOrNull;

        private void OnEnable()
        {
            Object tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            _tagManagerOrNull = new SerializedObject(tagManagerAsset);
        }

        private void OnDisable()
        {
            if (_tagManagerOrNull != null)
            {
                _tagManagerOrNull.Dispose();
                _tagManagerOrNull = null;
            }
        }

        private void OnDestroy()
        {
            if (_tagManagerOrNull != null)
            {
                _tagManagerOrNull.Dispose();
                _tagManagerOrNull = null;
            }

            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Tag Manager Utility", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Classes"))
            {
                if (_tagManagerOrNull == null)
                {
                    return;
                }

                _tagManagerOrNull.Update();

                string outDir = $"{Application.dataPath}/Generated";
                try
                {
                    AssetDatabase.StartAssetEditing();
                    Directory.CreateDirectory(outDir);

                    GenerateTagClass(outDir);
                    GenerateLayerClass(outDir);
                    GenerateSortingLayerClass(outDir);
                    GenerateRenderingLayerClass(outDir);
                }
                finally
                {
                    AssetDatabase.ImportAsset("Assets/Generated", ImportAssetOptions.ImportRecursive);
                    AssetDatabase.StopAssetEditing();
                }
            }
        }

        string GetValidName(string inName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in inName)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToUpper(c));
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }

        void GenerateTagClass(string outDir)
        {
            SerializedProperty tags = _tagManagerOrNull!.FindProperty("tags");
            StringBuilder sb = new StringBuilder();
            sb.Append(@$"
// ********************************************
// DO NOT EDIT THIS FILE! ( Auto generated )
// ********************************************
namespace Generated
{{
public static partial class TagManager
{{
public static class Tag
{{
");
            for (int i = 0; i < tags.arraySize; ++i)
            {
                string tag = tags.GetArrayElementAtIndex(i).stringValue;
                sb.AppendLine($"public const string {GetValidName(tag)} = \"{tag}\";");
            }

            sb.AppendLine("}}}");
            File.WriteAllText($"{outDir}/TagManager_Tag.g.cs", sb.ToString());
        }

        void GenerateLayerClass(string outDir)
        {
            SerializedProperty p = _tagManagerOrNull!.FindProperty("layers");
            StringBuilder sb = new StringBuilder();
            sb.Append(@$"
// ********************************************
// DO NOT EDIT THIS FILE! ( Auto generated )
// ********************************************
using System;
using System.ComponentModel;
using UnityEngine;

namespace Generated
{{
public static partial class TagManager
{{
");
            sb.AppendLine(@"
public static class LayerMASK
{");
            for (int i = 0; i < p.arraySize; ++i)
            {
                string layer = p.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(layer))
                {
                    continue;
                }

                sb.AppendLine(
                    @$"public readonly static LayerMask {GetValidName(layer)} = LayerMask.GetMask(""{layer}"");");
            }

            sb.AppendLine("}");

            sb.AppendLine(@"
public static class LayerINDEX
{");
            for (int i = 0; i < p.arraySize; ++i)
            {
                string layer = p.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(layer))
                {
                    continue;
                }

                sb.AppendLine(
                    @$"public readonly static int {GetValidName(layer)} = LayerMask.NameToLayer(""{layer}"");");
            }

            sb.AppendLine("}");

            sb.AppendLine(@"
[Flags]
public enum E_LAYER_FLAG : int
{");
            for (int i = 0; i < p.arraySize; ++i)
            {
                string layer = p.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(layer))
                {
                    continue;
                }

                int m = LayerMask.NameToLayer(layer);
                sb.Append(@$"
[Description(""{layer}"")]
{GetValidName(layer)} = 1 << {m},");
            }

            sb.AppendLine("}");

            sb.AppendLine("}}");
            File.WriteAllText($"{outDir}/TagManager_Layer.g.cs", sb.ToString());
        }

        void GenerateSortingLayerClass(string outDir)
        {
            SerializedProperty p = _tagManagerOrNull!.FindProperty("m_SortingLayers");
            StringBuilder sb = new StringBuilder();
            sb.Append(@$"
// ********************************************
// DO NOT EDIT THIS FILE! ( Auto generated )
// ********************************************
using System;
using System.ComponentModel;
using UnityEngine;

namespace Generated
{{
public static partial class TagManager
{{
");
            sb.AppendLine(@"
public static class SortingLayerName
{");
            for (int i = 0; i < p.arraySize; ++i)
            {
                SerializedProperty layer = p.GetArrayElementAtIndex(i);
                string name = layer.FindPropertyRelative("name").stringValue;
                sb.AppendLine(@$"public const string {GetValidName(name)} = ""{name}"";");
            }

            sb.AppendLine("}");

            sb.AppendLine(@"
public static class SortingLayerID
{");
            for (int i = 0; i < p.arraySize; ++i)
            {
                SerializedProperty layer = p.GetArrayElementAtIndex(i);
                string name = layer.FindPropertyRelative("name").stringValue;
                sb.AppendLine(
                    @$"public readonly static int {GetValidName(name)} = SortingLayer.NameToID(""{name}"");");
            }

            sb.AppendLine("}");

            sb.AppendLine("}}");
            File.WriteAllText($"{outDir}/TagManager_SortingLayer.g.cs", sb.ToString());
        }

        void GenerateRenderingLayerClass(string outDir)
        {
            SerializedProperty p = _tagManagerOrNull!.FindProperty("m_RenderingLayers");
            StringBuilder sb = new StringBuilder();
            sb.Append(@$"
// ********************************************
// DO NOT EDIT THIS FILE! ( Auto generated )
// ********************************************
using System;
using System.ComponentModel;
using UnityEngine;

namespace Generated
{{
public static partial class TagManager
{{
");
            sb.AppendLine(@"
[Flags]
public enum E_RENDERINGLAYER_FLAG : uint
{");
            for (int i = 0; i < p.arraySize; ++i)
            {
                string layer = p.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(layer))
                {
                    continue;
                }

                sb.Append(@$"
[Description(""{layer}"")]
{GetValidName(layer)} = 1 << {i},");
            }

            sb.AppendLine("}");

            sb.AppendLine("}}");
            File.WriteAllText($"{outDir}/TagManager_RenderingLayer.g.cs", sb.ToString());
        }
    }
}