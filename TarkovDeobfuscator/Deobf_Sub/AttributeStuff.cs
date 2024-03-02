using Mono.Cecil;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class AttributeStuff
    {
        public static List<CustomAttribute> customAttributes = new();

        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (!string.IsNullOrEmpty(config.AttributeOf))
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Name == config.AttributeOf)
                    {
                        foreach (var item in t.CustomAttributes)
                        {
                            File.WriteAllText("AttributeOf.txt", t.Name + " " + item.AttributeType.Name + "\n");
                            returner.Add(item.AttributeType.Resolve());
                        }
                    }
                }
                return returner;
            }
            return types;
        }
    }
}
