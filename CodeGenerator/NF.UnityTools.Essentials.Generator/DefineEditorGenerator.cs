using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace NF.UnityTools.Essentials.Generator
{
    [Generator]
    public class DefineEditorGenerator : IIncrementalGenerator
    {
        private const string ATTR_NAME = "UnityProjectDefine";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<ImmutableArray<EnumDeclarationSyntax>> enumDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(IsEnumTargetForGeneration, GetSemanticTargetForGeneration)
                .Where(static m => m is not null)
                .Select(static (m, _) => m!)
                .Collect();
            context.RegisterSourceOutput(enumDeclarations, GenerateEnumBasedEditorClass);
        }

        private bool IsEnumTargetForGeneration(SyntaxNode node, CancellationToken token)
        {
            return node is EnumDeclarationSyntax enumDeclarationSyntax
                && enumDeclarationSyntax.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Any(attr => attr.Name.ToString() == ATTR_NAME);
        }

        private static EnumDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token)
        {
            EnumDeclarationSyntax enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;
            return enumDeclarationSyntax;
        }

        private static void GenerateEnumBasedEditorClass(SourceProductionContext context, ImmutableArray<EnumDeclarationSyntax> enums)
        {
            foreach (EnumDeclarationSyntax enumDeclaration in enums)
            {
                string enumName = enumDeclaration.Identifier.Text;
                string namespaceName = GetNamespace(enumDeclaration);
                string source = GenerateToolDefineEditorClass(enumName, namespaceName);
                context.AddSource($"{enumName}_ToolDefineEditor.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        private static string GetNamespace(SyntaxNode syntaxNode)
        {
            NamespaceDeclarationSyntax namespaceNode = syntaxNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceNode?.Name.ToString() ?? "GlobalNamespace";
        }

        private static string GenerateToolDefineEditorClass(string enumName, string namespaceName)
        {
            string usingStatement = $"using {namespaceName};";
            if (namespaceName == "GlobalNamespace")
            {
                usingStatement = string.Empty;
            }
            return $@"
#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
{usingStatement}

namespace NF.UnityTools.Essentials.DefineManagement.Tool_DefineEditor.Generated
{{
    [InitializeOnLoad]
    public sealed class Tool_DefineEditor : NF.UnityTools.Essentials.DefineManagement.Tool_DefineEditor<{enumName}>
    {{
        static Tool_DefineEditor()
        {{
            var pre_defines = Enum.GetNames(typeof({enumName})).ToList();
            Init(pre_defines);
        }}

        [MenuItem(""@Tool/Tool_DefineEditor"")]
        private static void OpenScriptDefines()
        {{
            OpenWindow<Tool_DefineEditor>();
        }}
    }}
}}
#endif // UNITY_EDITOR
";
        }

    }

}
