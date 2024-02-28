using Mono.Cecil;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class SearchBaseTypes
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (!string.IsNullOrEmpty(config.BaseType))
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.BaseType != null && t.BaseType.Name != "Object")
                    {
                        if (t.BaseType.Name.Contains(config.BaseType))
                        {
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }
    }
}
