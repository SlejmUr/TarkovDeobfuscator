using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckNestedTypes
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.HasNestedTypes != null && config.HasNestedTypes.Count() > 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var field in config.HasNestedTypes)
                {
                    var filteredType = types.Where(x => x.HasNestedTypes && x.NestedTypes.Where(y => y.Name == field).Count() >= 1).ToList();
                    returner = Defucker.DEFUCK(filteredType, returner, field);
                }
                return returner;
            }
            return types;
        }
    }
}
