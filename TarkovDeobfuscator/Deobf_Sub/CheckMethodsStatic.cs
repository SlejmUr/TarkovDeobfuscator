using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckMethodsStatic
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.HasMethodsStatic != null && config.HasMethodsStatic.Count() > 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var field in config.HasMethodsStatic)
                {
                    var filteredType = types.Where(x => x.HasMethods && x.Methods.Where(y => y.Name == field && y.IsStatic).Count() >= 1).ToList();
                    returner = Defucker.DEFUCK(filteredType, returner, field);
                }
                return returner;
            }
            return types;
        }
    }
}
