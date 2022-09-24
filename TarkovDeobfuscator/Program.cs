using TarkovDeobfuscator;

namespace TarkovDeobf
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if (args[0].Contains("-remap"))
            {
                Deobf.DeobfuscateAssembly("Assembly-CSharp.dll", "Managed", true, false, true);
                return;
            }
            if (args[0].Contains("-override"))
            {
                Deobf.DeobfuscateAssembly("Assembly-CSharp.dll", "Managed", true, true, false);
                return;
            }
            if (args[0].Contains("-both"))
            {
                Deobf.DeobfuscateAssembly("Assembly-CSharp.dll", "Managed", true, true, true);
                return;
            }
            Deobf.DeobfuscateAssembly("Assembly-CSharp.dll", "Managed");
        }
    }
}