namespace NF.UnityTools.Essentials.DefineManagement
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class UnityProjectDefineAttribute : Attribute
    {
    }
}

namespace HelloNamespace
{
    using NF.UnityTools.Essentials.DefineManagement;

    [NF.UnityTools.Essentials.DefineManagement.UnityProjectDefine]
    public enum E_ENUM
    {
        A = 0,
    }
}