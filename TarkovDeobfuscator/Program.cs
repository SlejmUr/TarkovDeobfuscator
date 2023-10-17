namespace TarkovDeobfuscator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Deobf.OnLog += Deobf_OnLog;


            if (args.Length != 0)
            {
                if (args[0].Contains("-remap"))
                {
                    Deobf.DeobfuscateAssembly("EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll", "EscapeFromTarkov_Data/Managed", true, false, true);
                    return;
                }
                if (args[0].Contains("-override"))
                {
                    Deobf.DeobfuscateAssembly("EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll", "EscapeFromTarkov_Data/Managed", true, true, false);
                    return;
                }
                if (args[0].Contains("-both"))
                {
                    Deobf.DeobfuscateAssembly("EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll", "EscapeFromTarkov_Data/Managed", true, true, true);
                    return;
                }
            }

            Deobf.DeobfuscateAssembly("EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll", "EscapeFromTarkov_Data/Managed", true, true, true);
        }

        private static void Deobf_OnLog(string text)
        {
            if (!File.Exists("deobf_log.txt"))
            {
                File.WriteAllText("deobf_log.txt", text);
            }
            else
            {
                File.AppendAllText("deobf_log.txt", text + "\n");
            }
        }
    }
}