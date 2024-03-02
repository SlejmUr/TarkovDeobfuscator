using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckProperties
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.HasProperties != null && config.HasProperties.Count() > 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var field in config.HasProperties)
                {
                    var filteredType = types.Where(x => x.HasProperties && x.Properties.Where(y => y.Name == field).Count() >= 1).ToList();
                    returner = Defucker.DEFUCK(filteredType, returner, field);
                }
                return returner;
            }
            return types;
        }
    }
}
