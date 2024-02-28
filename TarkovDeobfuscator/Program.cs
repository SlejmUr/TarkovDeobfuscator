using static System.Net.Mime.MediaTypeNames;

namespace TarkovDeobfuscator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllText("deobf_log.txt", ""); // make sure shit is empty!!!
            Deobf.OnLog += Deobf_OnLog;
            var Path = "EscapeFromTarkov_Data/Managed";
            if(args.Length > 1)
                if (args[1] != "")
                    Path = args[1];

            if (args.Length != 0)
            {
                if (args[0].Contains("-remap"))
                {
                    Deobf.DeobfuscateAssembly($"{Path}/Assembly-CSharp.dll", $"{Path}", true, false, true);
                    return;
                }
                if (args[0].Contains("-override"))
                {
                    Deobf.DeobfuscateAssembly($"{Path}/Assembly-CSharp.dll", $"{Path}", true, true, false);
                    return;
                }
                if (args[0].Contains("-both"))
                {
                    Deobf.DeobfuscateAssembly($"{Path}/Assembly-CSharp.dll", $"{Path}", true, true, true);
                    return;
                }
                if (args[0].Contains("-fromcleaned"))
                {
                    Console.WriteLine("Remapping from prev cleaned assembly!");
                    Deobf.RemapFromCleanedAssembly($"{Path}/Assembly-CSharp.dll", $"{Path}");
                    return;
                }
            }

            Deobf.DeobfuscateAssembly($"{Path}/Assembly-CSharp.dll", $"{Path}", true, true, true);
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