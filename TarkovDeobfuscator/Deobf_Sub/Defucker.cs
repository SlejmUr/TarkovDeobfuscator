using Microsoft.VisualBasic;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class Defucker
    {
        public static List<TypeDefinition> DEFUCK(List<TypeDefinition> InputTypes, List<TypeDefinition> ToCheckAgainst)
        {
            List<TypeDefinition> tmp = ToCheckAgainst;
            int foundThatMF = 0;
            foreach (var inputtype in InputTypes)
            {
                foreach (var tocheck in ToCheckAgainst)
                {
                    if (inputtype.MetadataToken.ToInt32() == tocheck.MetadataToken.ToInt32())
                    {
                        foundThatMF = 1;
                    }
                }
                if (ToCheckAgainst.Where(x=>inputtype.MetadataToken.ToInt32() == x.MetadataToken.ToInt32()).Any())
                {
                    foundThatMF = 1;
                }

                if (foundThatMF == 0)
                    if (!tmp.Contains(inputtype))
                    {
                        File.AppendAllText("DEFUCK.txt", inputtype.FullName + " " + inputtype.MetadataToken.ToInt32() + "\n");
                        tmp.Add(inputtype);

                    }
            }

            StackTrace stackTrace = new(); 
            var m = stackTrace.GetFrame(1).GetMethod();
            File.AppendAllText("DEFUCK.txt", m.Name + " " + m.ReflectedType.FullName  + "\n");
            File.AppendAllText("DEFUCK.txt", "-----\n");










            /*
            foreach (var item in ToCheckAgainst)
            {
                int foundThatMF = 0;
                foreach (var tmp_item in InputTypes)
                {
                    if (tmp_item.MetadataToken.ToInt32() == item.MetadataToken.ToInt32())
                    {
                        foundThatMF = 1;
                    }
                }
                if (foundThatMF == 0)
                    if (!tmp.Contains(item))
                    {
                        tmp.Add(item);
                    }
            }*/
            return tmp;
        }
    }
}
