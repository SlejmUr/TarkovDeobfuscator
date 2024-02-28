using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class MustBeGClass
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.MustBeGClass.HasValue && config.MustBeGClass.Value)
            {
                return types.Where(x=>x.Name.StartsWith("GClass")).ToList();
            }
            return types;
        }
    }
}
