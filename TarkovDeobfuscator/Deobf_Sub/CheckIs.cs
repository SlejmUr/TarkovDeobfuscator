using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckIs
    {
        internal static List<TypeDefinition> RemapIsNested(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsNested.HasValue && config.IsNested.Value)
            {
                return types.Where(x=>x.IsNested).ToList();
            }
            return types;
        }

        internal static List<TypeDefinition> RemapIsClass(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsClass.HasValue && config.IsClass.Value)
            {
                return types.Where(x => x.IsClass && !x.IsEnum && !x.IsInterface).ToList();
            }
            return types;
        }

        internal static List<TypeDefinition> RemapIsInterface(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsInterface.HasValue && config.IsInterface.Value)
            {
                return types.Where(x => x.IsInterface && !x.IsEnum && !x.IsClass).ToList();
            }
            return types;
        }

        internal static List<TypeDefinition> RemapIsAbstract(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsAbstract.HasValue && config.IsAbstract.Value)
            {
                return types.Where(x => x.IsAbstract).ToList();
            }
            return types;
        }

        internal static List<TypeDefinition> RemapIsSealed(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsSealed.HasValue && config.IsSealed.Value)
            {
                return types.Where(x => x.IsSealed).ToList();
            }
            return types;
        }

        internal static List<TypeDefinition> RemapIsStruct(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsStruct.HasValue && config.IsStruct.Value)
            {
                return types.Where(x => x.IsValueType && !x.IsEnum).ToList();
            }
            return types;
        }

        internal static List<TypeDefinition> RemapIsEmpty(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.IsEmpty.HasValue && config.IsEmpty.Value)
            {
                return types.Where(x => !x.HasProperties && !x.HasEvents && !x.HasMethods && !x.HasFields).ToList();
            }
            return types;
        }
    }
}
