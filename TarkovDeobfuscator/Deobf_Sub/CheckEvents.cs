using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckEvents
    {
        internal static List<TypeDefinition> Remap(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.HasEvents != null && config.HasEvents.Count() > 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var field in config.HasEvents)
                {
                    var filteredType = types.Where(x => x.HasEvents && x.Events.Where(y => y.Name == field).Count() >= 1).ToList();
                    returner = Defucker.DEFUCK(filteredType, returner);
                }
                return returner;
            }
            return types;
        }
    }
}
