using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private const string ATTR_NAME = "NF.UnityTools.Essentials.DefineManagement.UnityProjectDefineAttribute";

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
            return node is EnumDeclarationSyntax;
        }

        private static EnumDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token)
        {
            EnumDeclarationSyntax enumDeclaration = (EnumDeclarationSyntax)context.Node;

            foreach (AttributeListSyntax attrList in enumDeclaration.AttributeLists)
            {
                foreach (AttributeSyntax attr in attrList.Attributes)
                {
                    SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(attr);

                    if (symbolInfo.Symbol is IMethodSymbol methodSymbol
                        && methodSymbol.ContainingType.ToDisplayString() == ATTR_NAME)
                    {
                        return enumDeclaration;
                    }
                }
            }
            return null;
        }

        private static void GenerateEnumBasedEditorClass(SourceProductionContext context, ImmutableArray<EnumDeclarationSyntax> enums)
        {
            if (enums.Length == 0)
            {
                return;
            }

            if (enums.Length > 1)
            {
                foreach (EnumDeclarationSyntax e in enums)
                {
                    Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptorCollection.NF1001, e.GetLocation(), e.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
                return;
            }

            EnumDeclarationSyntax enumDeclaration = enums.First();
            string enumName = enumDeclaration.Identifier.Text;
            string namespaceName = GetNamespace(enumDeclaration);
            string source = GenerateToolDefineEditorClass(enumName, namespaceName);
            context.AddSource($"{enumName}_ToolDefineEditor.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static string GetNamespace(SyntaxNode syntaxNode)
        {
            NamespaceDeclarationSyntax namespaceNode = syntaxNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceNode == null)
            {
                return null;
            }
            return namespaceNode.Name.ToString();
        }

        private static string GenerateToolDefineEditorClass(string enumName, string namespaceName)
        {
            string usingStatement = $"using {namespaceName};";
            if (string.IsNullOrEmpty(namespaceName))
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
            List<string> pre_defines = Enum.GetNames(typeof({enumName})).ToList();
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
