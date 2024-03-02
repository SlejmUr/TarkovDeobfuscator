using Mono.Cecil;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class BaseTypeOf
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (!string.IsNullOrEmpty(config.BaseTypeOf))
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Name == config.BaseTypeOf)
                    {
                        File.WriteAllText("BaseTypeOf.txt", t.Name + " " + t.BaseType.Name + "\n");
                        returner.Add(t.BaseType.Resolve());
                    }
                }
                return returner;
            }
            return types;
        }
    }
}
