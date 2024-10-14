using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NF.UnityTools.Essentials.Generator;

namespace CodeGenTest;

[TestClass]
public class Test_DefineEditorGenerator
{
    [DataTestMethod]
    [DataRow("Sample/A.Origin.cs", "Sample/A.Expected.cs")]
    public void GeneratedCode_ShouldMatchExpected(string pathOrigin, string pathExpected)
    {
        string codeOrigin = File.ReadAllText(pathOrigin);
        string codeExpected = File.ReadAllText(pathExpected);
        CSharpCompilation compilation = CSharpCompilation.Create("TestAssembly", [CSharpSyntaxTree.ParseText(codeOrigin)]);
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(new DefineEditorGenerator());
        GeneratorDriverRunResult result = driver.RunGenerators(compilation).GetRunResult();
        var codeGenerated = result.GeneratedTrees.First().ToString();
        Assert.AreEqual(codeExpected.Trim(), codeGenerated.Trim(), "Generated code does not match the expected output.");
    }
}
