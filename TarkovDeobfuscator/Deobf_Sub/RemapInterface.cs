using Mono.Cecil;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class RemapInterface
    {
        public static Dictionary<TypeDefinition, (string InterfaceFullName, string InterfaceName)> InterfaceTypes = new();

        static void TypeHelper(List<TypeDefinition> types)
        {
            foreach (var t in types)
            {
                foreach (var interfaceImplementation in t.Interfaces)
                {
                    string InterfaceName = interfaceImplementation.InterfaceType.Name;
                    string InterfaceFullName = interfaceImplementation.InterfaceType.FullName;

                    if (interfaceImplementation.InterfaceType.Name.Contains("`"))
                    {
                        var tmpname = interfaceImplementation.InterfaceType.Name.Split("`");
                        var tmp = tmpname[0] + tmpname[1][1..];
                        InterfaceName = tmp;
                    }

                    if (interfaceImplementation.InterfaceType.FullName.Contains("`"))
                    {
                        var tmpname = interfaceImplementation.InterfaceType.FullName.Split("`");
                        var tmp = tmpname[0] + tmpname[1][1..];
                        InterfaceFullName = tmp;
                    }

                    InterfaceTypes.TryAdd(t, (InterfaceFullName, InterfaceName));
                }
            }
        }

        internal static void Init(AssemblyDefinition oldAssembly)
        {
            TypeHelper(oldAssembly.MainModule.GetTypes().OrderBy(x => x.Name).ToList());
        }

        internal static List<TypeDefinition> Remap(AutoRemapperInfo config)
        {
            List<TypeDefinition> typeDefinitions = new();
            if (config.HasInterfaces != null && config.HasInterfaces.Length != 0)
            {
                foreach (var t in InterfaceTypes)
                {
                    for (int i = 0; i < config.HasInterfaces.Length; i++)
                    {
                        var facename = config.HasInterfaces[i];
                        bool HasGeneric = facename.Contains("<") && facename.Contains(">");
                        if (HasGeneric)
                        {
                            if (t.Value.InterfaceFullName.Contains(facename))
                                typeDefinitions.Add(t.Key);
                        }
                        else
                        {
                            if (t.Value.InterfaceName.Contains(facename))
                                typeDefinitions.Add(t.Key);
                        }
                    }
                }
            }
            return typeDefinitions;
        }
    }
}
