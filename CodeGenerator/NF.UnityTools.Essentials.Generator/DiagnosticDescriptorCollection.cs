using Microsoft.CodeAnalysis;

namespace NF.UnityTools.Essentials.Generator
{
    internal class DiagnosticDescriptorCollection
    {
        // ex) Assets\NewMonoBehaviourScript.cs(21,1): error NF1001: {messageFormat}
        internal static readonly DiagnosticDescriptor NF1001 = new(
            id: "NF1001", //
            title: string.Empty,
            messageFormat: "Multiple UnityProjectDefine attributes are not allowed. {0}", //
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}
