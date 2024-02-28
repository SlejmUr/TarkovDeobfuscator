using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckMethodsVirtual
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.HasMethodsVirtual != null && config.HasMethodsVirtual.Count() > 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var field in config.HasMethodsVirtual)
                {
                    var filteredType = types.Where(x => x.HasMethods && x.Methods.Where(y => y.Name == field && y.IsVirtual).Count() >= 1).ToList();
                    returner = Defucker.DEFUCK(filteredType, returner);
                }
                return returner;
            }
            return types;
        }
    }
}
