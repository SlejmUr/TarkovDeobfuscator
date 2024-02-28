using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class ClassNameMatching
    {
        internal static List<TypeDefinition> RemapFullName(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ClassNameFull != null && config.ClassNameFull.Length > 0)
            {
                List<TypeDefinition> returner = new();
                var filteredType = types.Where(x => x.FullName == config.ClassNameFull).ToList();
                returner.AddRange(filteredType);
                return returner;
            }
            return types;
        }

        internal static List<TypeDefinition> RemapPartialName(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ClassName != null && config.ClassName.Length > 0)
            {
                List<TypeDefinition> returner = new();
                var filteredType = types.Where(x => x.FullName.Contains(config.ClassName)).ToList();
                returner.AddRange(filteredType);
                return returner;
            }
            return types;
        }
    }
}
