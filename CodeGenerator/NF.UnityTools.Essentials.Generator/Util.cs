using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;

namespace NF.UnityTools.Essentials.Generator
{
    internal sealed class Util
    {
        internal static Func<GeneratorSyntaxContext, CancellationToken, EnumDeclarationSyntax> CreateEnumSemanticTargetForGeneration(string attrName)
        {
            return (context, token) =>
            {
                EnumDeclarationSyntax enumDeclaration = (EnumDeclarationSyntax)context.Node;

                foreach (AttributeListSyntax attrList in enumDeclaration.AttributeLists)
                {
                    foreach (AttributeSyntax attr in attrList.Attributes)
                    {
                        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(attr);

                        if (symbolInfo.Symbol is IMethodSymbol methodSymbol
                            && methodSymbol.ContainingType.ToDisplayString() == attrName)
                        {
                            return enumDeclaration;
                        }
                    }
                }
                return null;
            };
        }
    }
}
