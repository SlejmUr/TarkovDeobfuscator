using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TarkovDeobfuscator.Deobf_Sub;

namespace TarkovDeobfuscator
{
    public class Deobf
    {
        public delegate void LogHandler(string text);
        public static event LogHandler OnLog;
        public static List<string> Logged = new List<string>();

        internal static void Log(string text)
        {
            if (OnLog != null)
            {
                OnLog(text);
            }
            else
            {
                Debug.WriteLine(text);
                Console.WriteLine(text);
                Logged.Add(text);
            }
        }
        public static bool DeobfuscateAssembly(string assemblyPath, string managedPath, bool createBackup = true, bool overwriteExisting = false, bool doRemapping = false)
        {
            var de4dotLocation = Path.Combine(Directory.GetCurrentDirectory(), "Deobfuscator", "de4dot.exe");

            string token;

            using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
            {
                var potentialStringDelegates = new List<MethodDefinition>();

                foreach (var type in assemblyDefinition.MainModule.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.ReturnType.FullName != "System.String"
                            || method.Parameters.Count != 1
                            || method.Parameters[0].ParameterType.FullName != "System.Int32"
                            || method.Body == null
                            || !method.IsStatic)
                        {
                            continue;
                        }

                        if (!method.Body.Instructions.Any(x =>
                            x.OpCode.Code == Code.Callvirt &&
                            ((MethodReference)x.Operand).FullName == "System.Object System.AppDomain::GetData(System.String)"))
                        {
                            continue;
                        }

                        potentialStringDelegates.Add(method);
                    }
                }

                var deobfRid = potentialStringDelegates[0].MetadataToken;

                token = $"0x{((uint)deobfRid.TokenType | deobfRid.RID):x4}";

                Console.WriteLine($"Deobfuscation token: {token}");
            }

            var process = Process.Start(de4dotLocation,
                $"--un-name \"!^<>[a-z0-9]$&!^<>[a-z0-9]__.*$&![A-Z][A-Z]\\$<>.*$&^[a-zA-Z_<{{$][a-zA-Z_0-9<>{{}}$.`-]*$\" \"{assemblyPath}\" --strtyp delegate --strtok \"{token}\"");

            process.WaitForExit();


            // Fixes "ResolutionScope is null" by rewriting the assembly
            var cleanedDllPath = Path.Combine(Path.GetDirectoryName(assemblyPath), Path.GetFileNameWithoutExtension(assemblyPath) + "-cleaned.dll");

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedPath);

            using (var memoryStream = new MemoryStream(File.ReadAllBytes(cleanedDllPath)))
            using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(memoryStream, new ReaderParameters(){ AssemblyResolver = resolver }))
            {
                assemblyDefinition.Write(cleanedDllPath);
            }


            if (doRemapping)
                RemapKnownClasses(managedPath, cleanedDllPath);
            if (createBackup)
                BackupExistingAssembly(assemblyPath);
            if (overwriteExisting)
                OverwriteExistingAssembly(assemblyPath, cleanedDllPath);

            Log($"DeObfuscation complete!");

            return true;
        }




        public static void RemapFromCleanedAssembly(string assemblyPath, string managedPath)
        {
            var cleanedDllPath = Path.Combine(Path.GetDirectoryName(assemblyPath), Path.GetFileNameWithoutExtension(assemblyPath) + "-cleaned.dll");
            RemapKnownClasses(managedPath, cleanedDllPath);
        }



        private static void OverwriteExistingAssembly(string assemblyPath, string cleanedDllPath, bool deleteCleaned = false)
        {
            // Do final copy to Assembly
            File.Copy(cleanedDllPath, assemblyPath, true);
            // Delete -cleaned
            if (deleteCleaned)
                File.Delete(cleanedDllPath);
        }
        private static void RemapKnownClasses(string managedPath, string assemblyPath)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedPath);

            File.Copy(assemblyPath, assemblyPath + ".backup", true);

            var readerParameters = new ReaderParameters { AssemblyResolver = resolver };
            using (var fsAssembly = new FileStream(assemblyPath, FileMode.Open))
            {
                using (var oldAssembly = AssemblyDefinition.ReadAssembly(fsAssembly, readerParameters))
                {
                    if (oldAssembly != null)
                    {
                        try
                        {
                            RemapperConfig autoRemapperConfig = JsonConvert.DeserializeObject<RemapperConfig>(File.ReadAllText(Directory.GetCurrentDirectory() + "//Deobfuscator/AutoRemapperConfig.json"));
                            RemapByAutoConfiguration(oldAssembly, autoRemapperConfig);
                            DefineRemap.RemapByDefinedConfiguration(oldAssembly, autoRemapperConfig);
                            //RemapByDefinedConfiguration(oldAssembly, autoRemapperConfig);
                            RemapAfterEverything(oldAssembly, autoRemapperConfig);
                            oldAssembly.Write(assemblyPath.Replace(".dll", "-remapped.dll"));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }

                    }
                }
            }
            File.Copy(assemblyPath.Replace(".dll", "-remapped.dll"), assemblyPath, true);

        }
        static void RemapByAutoConfiguration(AssemblyDefinition oldAssembly, RemapperConfig autoRemapperConfig)
        {
            if (!autoRemapperConfig.EnableAutomaticRemapping)
                return;

            var gclasses = oldAssembly.MainModule.GetTypes().Where(x =>
                x.Name.StartsWith("GClass"));
            var gclassToNameCounts = new Dictionary<string, int>();

            //foreach (var t in oldAssembly.MainModule.GetTypes().Where(x => !x.Name.StartsWith("GClass") && !x.Name.StartsWith("Class")))
            foreach (var t in oldAssembly.MainModule.GetTypes())
            {
                // --------------------------------------------------------
                // Renaming by the classes being in methods
                foreach (var m in t.Methods.Where(x => x.HasParameters
                    && x.Parameters.Any(p =>
                    p.ParameterType.Name.StartsWith("GClass")
                    || p.ParameterType.Name.StartsWith("GStruct")
                    || p.ParameterType.Name.StartsWith("GInterface"))))
                {
                    // --------------------------------------------------------
                    // Renaming by the classes being used as Parameters in methods
                    foreach (var p in m.Parameters
                        .Where(x =>
                        x.ParameterType.Name.StartsWith("GClass")
                        || x.ParameterType.Name.StartsWith("GStruct")
                        || x.ParameterType.Name.StartsWith("GInterface")
                        //|| x.ParameterType.Name.StartsWith("Class")
                        ))
                    {
                        var n = p.ParameterType.Name
                            .Replace("[]", "")
                            .Replace("`1", "")
                            .Replace("&", "")
                            .Replace(" ", "")
                            + "." + p.Name;
                        if (!gclassToNameCounts.ContainsKey(n))
                        {
                            gclassToNameCounts.Add(n, 0);

                        }

                        gclassToNameCounts[n]++;
                    }

                }

                // --------------------------------------------------------
                // Renaming by the classes being used as Members/Properties/Fields in other classes
                foreach (var prop in t.Properties.Where(p =>
                    p.PropertyType.Name.StartsWith("GClass")
                    || p.PropertyType.Name.StartsWith("GStruct")
                    || p.PropertyType.Name.StartsWith("GInterface")
                    ))
                {
                    // if the property name includes "gclass" or whatever, then ignore it as its useless to us
                    if (prop.Name.StartsWith("GClass", StringComparison.OrdinalIgnoreCase)
                        || prop.Name.StartsWith("GStruct", StringComparison.OrdinalIgnoreCase)
                        || prop.Name.StartsWith("GInterface", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var n = prop.PropertyType.Name
                        .Replace("[]", "")
                        .Replace("`1", "")
                        .Replace("&", "")
                        .Replace(" ", "")
                        + "." + prop.Name;
                    if (!gclassToNameCounts.ContainsKey(n))
                        gclassToNameCounts.Add(n, 0);

                    gclassToNameCounts[n]++;
                    // this is shit and needs fixing
                    //if (gclassToNameCounts[n] > 1)
                    //{
                    //    gclassToNameCounts[n] = 0;
                    //}
                }

                foreach (var prop in t.Fields.Where(p =>
                    p.FieldType.Name.StartsWith("GClass")
                    || p.FieldType.Name.StartsWith("GStruct")
                    || p.FieldType.Name.StartsWith("GInterface")
                    ))
                {
                    if (prop.Name.StartsWith("GClass", StringComparison.OrdinalIgnoreCase)
                    || prop.Name.StartsWith("GStruct", StringComparison.OrdinalIgnoreCase)
                    || prop.Name.StartsWith("GInterface", StringComparison.OrdinalIgnoreCase)
                    || prop.Name.StartsWith("_")
                    || prop.Name.Contains("_")
                    || prop.Name.Contains("/")
                    )
                        continue;

                    var n = prop.FieldType.Name
                        .Replace("[]", "")
                        .Replace("`1", "")
                        .Replace("&", "")
                        .Replace(" ", "")
                        + "." + prop.Name;
                    if (!gclassToNameCounts.ContainsKey(n))
                        gclassToNameCounts.Add(n, 0);

                    gclassToNameCounts[n]++;
                }


            }

            var autoRemappedClassCount = 0;

            // ----------------------------------------------------------------------------------------
            // Rename classes based on discovery above
            var orderedGClassCounts = gclassToNameCounts.Where(x => x.Value > 0 && !x.Key.Contains("`")).OrderByDescending(x => x.Value);
            var usedNamesCount = new Dictionary<string, int>();
            var renamedClasses = new Dictionary<string, string>();
            foreach (var g in orderedGClassCounts)
            {
                var keySplit = g.Key.Split('.');
                var gclassName = keySplit[0];
                var gclassNameNew = keySplit[1];
                if (gclassNameNew.Length <= 3
                    || gclassNameNew.StartsWith("Value", StringComparison.OrdinalIgnoreCase)
                    || gclassNameNew.StartsWith("Attribute", StringComparison.OrdinalIgnoreCase)
                    || gclassNameNew.StartsWith("Instance", StringComparison.OrdinalIgnoreCase)
                    || gclassNameNew.StartsWith("_", StringComparison.OrdinalIgnoreCase)
                    || gclassNameNew.StartsWith("<", StringComparison.OrdinalIgnoreCase)
                    || Assembly.GetAssembly(typeof(Attribute)).GetTypes().Any(x => x.Name.StartsWith(gclassNameNew, StringComparison.OrdinalIgnoreCase))
                    || oldAssembly.MainModule.GetTypes().Any(x => x.Name.Equals(gclassNameNew, StringComparison.OrdinalIgnoreCase))
                    )
                    continue;

                var t = oldAssembly.MainModule.GetTypes().FirstOrDefault(x => x.Name == gclassName);
                if (t == null)
                    continue;

                // Follow standard naming convention, PascalCase all class names
                var newClassName = char.ToUpper(gclassNameNew[0]) + gclassNameNew.Substring(1);

                // Following BSG naming convention, begin Abstract classes names with "Abstract"
                if (t.IsAbstract && !t.IsInterface)
                    newClassName = "Abstract" + newClassName;
                // Follow standard naming convention, Interface names begin with "I"
                else if (t.IsInterface)
                    newClassName = "I" + newClassName;

                if (!usedNamesCount.ContainsKey(newClassName))
                    usedNamesCount.Add(newClassName, 0);

                usedNamesCount[newClassName]++;

                if (usedNamesCount[newClassName] > 1)
                    newClassName += usedNamesCount[newClassName];

                if (!oldAssembly.MainModule.GetTypes().Any(x => x.Name == newClassName)
                    && !Assembly.GetAssembly(typeof(Attribute)).GetTypes().Any(x => x.Name.StartsWith(newClassName, StringComparison.OrdinalIgnoreCase))
                    && !oldAssembly.MainModule.GetTypes().Any(x => x.Name.Equals(newClassName, StringComparison.OrdinalIgnoreCase))
                    )
                {
                    var oldClassName = t.Name;
                    t.Name = newClassName;
                    renamedClasses.Add(oldClassName, newClassName);
                    Log($"Remapper: Auto Remapped {oldClassName} to {newClassName}");
                }
            }
            // end of renaming based on discovery
            // ---------------------------------------------------------------------------------------

            // ------------------------------------------------
            // Auto rename FirearmController sub classes
            foreach (var t in oldAssembly.MainModule.GetTypes().Where(x
                =>
                    x.FullName.StartsWith("EFT.Player.FirearmController")
                    && x.Name.StartsWith("GClass")

                ))
            {
                t.Name.Replace("GClass", "FirearmController");
            }

            // ------------------------------------------------
            // Auto rename descriptors
            foreach (var t in oldAssembly.MainModule.GetTypes())
            {
                foreach (var m in t.Methods.Where(x => x.Name.StartsWith("ReadEFT")))
                {
                    if (m.ReturnType.Name.StartsWith("GClass"))
                    {
                        var rT = oldAssembly.MainModule.GetTypes().FirstOrDefault(x => x == m.ReturnType);
                        if (rT != null)
                        {
                            var oldTypeName = rT.Name;
                            rT.Name = m.Name.Replace("ReadEFT", "");
                            Log($"Remapper: Auto Remapped {oldTypeName} to {rT.Name}");

                        }
                    }
                }
            }

            // Testing stuff here.
            // Quick hack to name properties properly in EFT.Player
            foreach (var playerProp in oldAssembly.MainModule.GetTypes().FirstOrDefault(x => x.FullName == "EFT.Player").Properties)
            {
                if (playerProp.Name.StartsWith("GClass", StringComparison.OrdinalIgnoreCase))
                {
                    playerProp.Name = playerProp.PropertyType.Name.Replace("Abstract", "");
                }
            }

            Log($"Remapper: Ensuring EFT classes are public");
            foreach (var t in oldAssembly.MainModule.GetTypes())
            {
                if (t.IsClass && t.IsDefinition && t.BaseType != null && t.BaseType.FullName != "System.Object")
                {
                    if (!Assembly.GetAssembly(typeof(Attribute))
                        .GetTypes()
                        .Any(x => x.Name.StartsWith(t.Name, StringComparison.OrdinalIgnoreCase)))
                        t.IsPublic = true;
                }
            }

            Log($"Remapper: Setting EFT methods to public");
            foreach (var ctf in autoRemapperConfig.TypesToForceAllPublicMethods)
            {
                var foundTypes = oldAssembly.MainModule.GetTypes()
                    .Where(x => x.Namespace.Contains("EFT", StringComparison.OrdinalIgnoreCase))
                    .Where(x => x.Name.Contains(ctf, StringComparison.OrdinalIgnoreCase));
                foreach (var t in foundTypes)
                {
                    foreach (var m in t.Methods)
                    {
                        if (!m.IsPublic)
                            m.IsPublic = true;
                    }
                }
            }

            Log($"Remapper: Setting EFT fields/properties to public");
            foreach (var ctf in autoRemapperConfig.TypesToForceAllPublicFieldsAndProperties)
            {
                var foundTypes = oldAssembly.MainModule.GetTypes()
                    .Where(x => x.Namespace.Contains("EFT", StringComparison.OrdinalIgnoreCase))
                    .Where(x => x.Name.Contains(ctf, StringComparison.OrdinalIgnoreCase));
                foreach (var t in foundTypes)
                {
                    foreach (var m in t.Fields)
                    {
                        if (!m.IsPublic)
                            m.IsPublic = true;
                    }
                }
            }
            if (autoRemapperConfig.TypesToConvertConstructorsToPublic != null)
            {
                Log($"Remapper: Setting EFT Types Cons to Public to public");
                foreach (var ctf in autoRemapperConfig.TypesToConvertConstructorsToPublic)
                {
                    var foundTypes = oldAssembly.MainModule.GetTypes()
                        .Where(x => x.FullName.StartsWith(ctf, StringComparison.OrdinalIgnoreCase) || x.FullName.EndsWith(ctf));
                    foreach (var t in foundTypes)
                    {
                        foreach (var c in t.GetConstructors())
                        {
                            c.IsPublic = true;
                        }
                        //t.Resolve();
                    }
                }

            }


            Log($"Remapper: Setting All Types to public");
            if (autoRemapperConfig.ForceAllToPublic)
            {
                var foundTypes = oldAssembly.MainModule.GetTypes();

                foreach (var t in foundTypes)
                {
                    t.IsPublic = true;
                    foreach (var m in t.Fields)
                    {
                        if (!m.IsPublic)
                            m.IsPublic = true;
                    }
                    foreach (var m in t.Methods)
                    {
                        if (!m.IsPublic)
                            m.IsPublic = true;
                    }
                    foreach (var m in t.NestedTypes)
                    {
                        if (!m.IsPublic)
                            m.IsPublic = true;
                    }
                    foreach (var m in t.Events)
                    {
                        if (!m.DeclaringType.IsPublic)
                            m.DeclaringType.IsPublic = true;
                    }
                    foreach (var m in t.Properties)
                    {
                        if (!m.DeclaringType.IsPublic)
                            m.DeclaringType.IsPublic = true;
                    }
                }

            }


            autoRemappedClassCount = renamedClasses.Count;
            Log($"Remapper: Auto Remapped {autoRemappedClassCount} classes");
        }


        public static Dictionary<TypeDefinition, (string InterfaceFullName, string InterfaceName)> InterfaceTypes = new();

        static void TypeHelper(List<TypeDefinition> types)
        {
            foreach (var t in types)
            {
                foreach (var interfaceImplementation in t.Interfaces)
                {
                    string InterfaceName = interfaceImplementation.InterfaceType.Name;
                    string InterfaceFullName = interfaceImplementation.InterfaceType.FullName;

                    if (interfaceImplementation.InterfaceType.Name.Contains("`"))
                    {
                        var tmpname = interfaceImplementation.InterfaceType.Name.Split("`");
                        var tmp = tmpname[0] + tmpname[1][1..];
                        InterfaceName = tmp;
                    }

                    if (interfaceImplementation.InterfaceType.FullName.Contains("`"))
                    {
                        var tmpname = interfaceImplementation.InterfaceType.FullName.Split("`");
                        var tmp = tmpname[0] + tmpname[1][1..];
                        InterfaceFullName = tmp;
                    }

                    InterfaceTypes.TryAdd(t, (InterfaceFullName, InterfaceName));
                }
            }
        }
        private static void RemapByDefinedConfiguration(AssemblyDefinition oldAssembly, RemapperConfig autoRemapperConfig)
        {
            if (!autoRemapperConfig.EnableDefinedRemapping)
                return;

            TypeHelper(oldAssembly.MainModule.GetTypes().OrderBy(x => x.Name).ToList());

            int countOfDefinedMappingSucceeded = 0;
            int countOfDefinedMappingFailed = 0;
            bool x = false;
            foreach (var config in autoRemapperConfig.DefinedRemapping.Where(x => !string.IsNullOrEmpty(x.RenameClassNameTo)))
            {

                try
                {
                    List<TypeDefinition> typeDefinitions = new();
                    var findTypes
                        = oldAssembly.MainModule.GetTypes().OrderBy(x=>x.Name).ToList();

                    if (config.HasInterfaces != null && config.HasInterfaces.Length != 0)
                    {
                        foreach (var t in InterfaceTypes)
                        {
                            for (int i = 0; i < config.HasInterfaces.Length; i++)
                            {
                                var facename = config.HasInterfaces[i];
                                bool HasGeneric = facename.Contains("<") && facename.Contains(">");
                                if (HasGeneric)
                                {
                                    if (t.Value.InterfaceFullName.Contains(facename))
                                        typeDefinitions.Add(t.Key);
                                }
                                else
                                {
                                    if (t.Value.InterfaceName.Contains(facename))
                                        typeDefinitions.Add(t.Key);
                                }
                            }
                        }
                    }
                    if (typeDefinitions.Count() > 0)
                    { findTypes = typeDefinitions; }

                    // Filter Types by Must Be GClass
                    findTypes = findTypes.Where(
                        x =>
                            (
                                   !config.MustBeGClass.HasValue || (config.MustBeGClass.Value && x.Name.StartsWith("GClass"))
                            )
                        ).ToList();

                    // Filter Type by IsNestedInClass
                    findTypes = findTypes.Where(
                       x =>
                           (
                               string.IsNullOrEmpty(config.IsNestedInClass)
                               || (!string.IsNullOrEmpty(config.IsNestedInClass) && x.FullName.Contains(config.IsNestedInClass + "+", StringComparison.OrdinalIgnoreCase))
                               || (!string.IsNullOrEmpty(config.IsNestedInClass) && x.FullName.Contains(config.IsNestedInClass + ".", StringComparison.OrdinalIgnoreCase))
                               || (!string.IsNullOrEmpty(config.IsNestedInClass) && x.FullName.Contains(config.IsNestedInClass + "/", StringComparison.OrdinalIgnoreCase))
                           )
                       ).ToList();

                    // Filter Types by Inherits Class
                    findTypes = findTypes.Where(
                        x =>
                            (
                                config.InheritsClass == null || config.InheritsClass.Length == 0
                                || (x.BaseType != null && x.BaseType.Name == config.InheritsClass)
                            )
                        ).ToList();

                    // Filter Types by Class Name Matching
                    findTypes = findTypes.Where(
                        x =>
                            (
                                config.ClassName == null || config.ClassName.Length == 0 || (x.FullName.Contains(config.ClassName))
                            )
                        ).ToList();

                    // Filter Types by Class Name Matching
                    findTypes = findTypes.Where(
                        x =>
                            (
                                config.ClassNameFull == null || config.ClassNameFull.Length == 0 || x.FullName == config.ClassNameFull
                            )
                        ).ToList();

                    // Filter Types by Methods
                    findTypes = findTypes.Where(x
                            =>
                                (config.HasMethods == null || config.HasMethods.Length == 0
                                    || (x.Methods.Select(y => y.Name.Split('.')[y.Name.Split('.').Length - 1]).Count(y => config.HasMethods.Contains(y)) >= config.HasMethods.Length))

                            ).ToList();

                    // Filter Types by Virtual Methods
                    if (config.HasMethodsVirtual != null && config.HasMethodsVirtual.Length > 0)
                    {
                        findTypes = findTypes.Where(x
                               =>
                                 (x.Methods.Count(y => y.IsVirtual) > 0
                                    && x.Methods.Where(y => y.IsVirtual).Count(y => config.HasMethodsVirtual.Contains(y.Name)) >= config.HasMethodsVirtual.Length
                                    )
                               ).ToList();
                    }

                    // Filter Types by Static Methods
                    findTypes = findTypes.Where(x
                            =>
                                (config.HasMethodsStatic == null || config.HasMethodsStatic.Length == 0
                                    || (x.Methods.Where(x => x.IsStatic).Select(y => y.Name.Split('.')[y.Name.Split('.').Length - 1]).Count(y => config.HasMethodsStatic.Contains(y)) >= config.HasMethodsStatic.Length))

                            ).ToList();

                    // Filter Types by Events
                    findTypes = findTypes.Where(x
                           =>
                               (config.HasEvents == null || config.HasEvents.Length == 0
                                   || (x.Events.Select(y => y.Name.Split('.')[y.Name.Split('.').Length - 1]).Count(y => config.HasEvents.Contains(y)) >= config.HasEvents.Length))

                           ).ToList();

                    // Filter Types by Field/Properties
                    findTypes = findTypes.Where(
                        x =>
                                (
                                    // fields
                                    (
                                    config.HasFields == null || config.HasFields.Length == 0
                                    || (x.Fields.Count(y => y.IsDefinition && config.HasFields.Contains(y.Name)) >= config.HasFields.Length)
                                    )
                                    ||
                                    // properties
                                    (
                                    config.HasProperties == null || config.HasProperties.Length == 0
                                    || (x.Properties.Count(y => y.IsDefinition && config.HasProperties.Contains(y.Name)) >= config.HasProperties.Length)

                                    )
                                )).ToList();

                    // Filter Types by Class/Interface
                    findTypes = findTypes.Where(
                        x =>
                            (
                                (!config.IsClass.HasValue     || (config.IsClass.HasValue     && config.IsClass.Value     && x.IsClass && !x.IsEnum && !x.IsInterface))
                                && 
                                (!config.IsInterface.HasValue || (config.IsInterface.HasValue && config.IsInterface.Value && (x.IsInterface && !x.IsEnum && !x.IsClass)))
                            )
                        ).ToList();

                    //Filter by IsNested
                    findTypes = findTypes.Where(
                        x =>
                            (
                                !config.IsNested.HasValue || (config.IsNested.HasValue && config.IsNested.Value && x.IsNested)
                            )
                        ).ToList();

                    //Filter by IsAbstract
                    findTypes = findTypes.Where(
                        x =>
                            (
                                !config.IsAbstract.HasValue || (config.IsAbstract.HasValue && config.IsAbstract.Value && x.IsAbstract)
                            )
                        ).ToList();

                    /*
                    // Filter by Interfaces
                    findTypes = findTypes.Where(x
                        =>
                            (config.HasInterfaces == null || config.HasInterfaces.Length == 0
                                || (x.Interfaces.Select(y => y.InterfaceType.Name.Split('.')[y.InterfaceType.Name.Split('.').Length - 1]).Count(y => config.HasInterfaces.Contains(y)) >= config.HasInterfaces.Length))

                        ).ToList();
                    */

                    // Filter by Nested Types
                    findTypes = findTypes.Where(x
                        =>
                            (config.HasNestedTypes == null || config.HasNestedTypes.Length == 0
                                || (x.NestedTypes.Select(y => y.Name.Split('.')[y.Name.Split('.').Length - 1]).Count(y => config.HasNestedTypes.Contains(y)) >= config.HasNestedTypes.Length))

                        ).ToList();

                    // Filter by Properties
                    findTypes = findTypes.Where(x
                        =>
                            (config.HasProperties == null || config.HasProperties.Length == 0
                                || (x.Properties.Select(y => y.Name.Split('.')[y.Name.Split('.').Length - 1]).Count(y => config.HasProperties.Contains(y)) >= config.HasProperties.Length))

                        ).ToList();

                    // Filter Types by Constructor
                    if (config.HasConstructorArgs != null)
                        findTypes = findTypes.Where(t => t.Methods.Any(x => x.IsConstructor && x.Parameters.Count == config.HasConstructorArgs.Length)).ToList();

                    // Filter Types by Is Structure
                    findTypes = findTypes.Where(
                        x =>
                           (
                                (!config.IsStruct.HasValue || (config.IsStruct.HasValue && config.IsStruct.Value && (x.IsValueType)))
                           )
                        ).ToList();

                    //Filter by BaseType
                    if (!string.IsNullOrEmpty(config.BaseType))
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.BaseType != null && t.BaseType.Name != "Object")
                            {
                                //File.AppendAllText("BaseTypeSearch.txt", t.BaseType.Name + " | " + t.BaseType.Name + "\n");
                                if (t.BaseType.Name.Contains(config.BaseType))
                                {
                                    typeDefinitions.Add(t);
                                }
                            }
                        
                        }
                    }
                    
                    
                    // Filter with ExactProperties
                    if (config.ExactProperties != null && config.ExactProperties.Length != 0)
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.Properties.Count == config.ExactProperties.Length)
                            {
                                int okField = 0;
                                for (int i = 0; i < config.ExactProperties.Length; i++)
                                {
                                    if (t.Properties[i].Name == config.ExactProperties[i])
                                    {
                                        WriteLog(i + " " + t.Name + " " + t.Properties[i].Name + " == " + config.ExactProperties[i].ToString());
                                        okField++;
                                    }
                                }
                                if (okField == config.ExactProperties.Length)
                                {
                                    WriteLog(t.Name + " is found!");
                                    typeDefinitions.Add(t);
                                }
                            }
                        }
                    }

                    // Filter with ExactFields
                    if (config.ExactFields != null && config.ExactFields.Length != 0)
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.Fields.Count == config.ExactFields.Length)
                            {
                                int okField = 0;
                                for (int i = 0; i < config.ExactFields.Length; i++)
                                {
                                    if (t.Fields[i].Name == config.ExactFields[i])
                                    {
                                        WriteLog(i + " " + t.Name + " " + t.Fields[i].Name + " == " + config.ExactFields[i].ToString());
                                        okField++;
                                    }
                                }
                                if (okField == config.ExactFields.Length)
                                {
                                    WriteLog(t.Name + " is found!");
                                    typeDefinitions.Add(t);
                                }
                            }
                        }
                    }

                    // Filter with ExactMethods
                    if (config.ExactMethods != null && config.ExactMethods.Length != 0)
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.Methods.Count == config.ExactMethods.Length)
                            {
                                int okField = 0;
                                for (int i = 0; i < config.ExactMethods.Length; i++)
                                {
                                    if (t.Methods[i].Name == config.ExactMethods[i])
                                    {
                                        WriteLog(i + " " + t.Name + " " + t.Methods[i].Name + " == " + config.ExactMethods[i].ToString());
                                        okField++;
                                    }
                                }
                                if (okField == config.ExactMethods.Length)
                                {
                                    WriteLog(t.Name + " is found!");
                                    typeDefinitions.Add(t);
                                }
                            }
                        }
                    }

                    // Filter with ExactEvents
                    if (config.ExactEvents != null && config.ExactEvents.Length != 0)
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.Events.Count == config.ExactEvents.Length)
                            {
                                int okField = 0;
                                for (int i = 0; i < config.ExactEvents.Length; i++)
                                {
                                    if (t.Events[i].Name == config.ExactEvents[i])
                                    {
                                        WriteLog(i + " " + t.Name + " " + t.Events[i].Name + " == " + config.ExactEvents[i].ToString());
                                        okField++;
                                    }
                                }
                                if (okField == config.ExactEvents.Length)
                                {
                                    WriteLog(t.Name + " is found!");
                                    typeDefinitions.Add(t);
                                }
                            }
                        }
                    }

                    // Filter with ExactInterfaces
                    if (config.ExactInterfaces != null && config.ExactInterfaces.Length != 0)
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.Interfaces.Count == config.ExactInterfaces.Length)
                            {
                                int okField = 0;
                                for (int i = 0; i < config.ExactInterfaces.Length; i++)
                                {
                                    if (t.Interfaces[i].InterfaceType.Name == config.ExactInterfaces[i])
                                    {
                                        WriteLog(i + " " + t.Name + " " + t.Interfaces[i].InterfaceType.Name + " == " + config.ExactInterfaces[i].ToString());
                                        okField++;
                                    }
                                }
                                if (okField == config.ExactInterfaces.Length)
                                {
                                    WriteLog(t.Name + " is found!");
                                    typeDefinitions.Add(t);
                                }
                            }
                        }
                    }

                    // Filter with ExactNestedTypes
                    if (config.ExactNestedTypes != null && config.ExactNestedTypes.Length != 0)
                    {
                        foreach (var t in findTypes)
                        {
                            if (t.NestedTypes.Count == config.ExactNestedTypes.Length)
                            {
                                int okField = 0;
                                for (int i = 0; i < config.ExactNestedTypes.Length; i++)
                                {
                                    if (t.NestedTypes[i].Name == config.ExactNestedTypes[i])
                                    {
                                        WriteLog(i + " " + t.Name + " " + t.NestedTypes[i].Name + " == " + config.ExactNestedTypes[i].ToString());
                                        okField++;
                                    }
                                }
                                if (okField == config.ExactNestedTypes.Length)
                                {
                                    WriteLog(t.Name + " is found!");
                                    typeDefinitions.Add(t);
                                }
                            }
                        }
                    }


                    if (typeDefinitions.Count() > 0)
                    { findTypes = typeDefinitions; }

                    if (findTypes.Any())
                    {
                        var onlyRemapFirstFoundType = config.OnlyRemapFirstFoundType.HasValue && config.OnlyRemapFirstFoundType.Value;
                        if (findTypes.Count() > 1 && !onlyRemapFirstFoundType)
                        {
                            findTypes = findTypes
                                .OrderBy(x => !x.Name.StartsWith("GClass") && !x.Name.StartsWith("GInterface"))
                                .ThenBy(x => x.Name.StartsWith("GInterface"))
                                .ToList();

                            var numberOfChangedIndexes = 0;
                            for (var index = 0; index < findTypes.Count(); index++)
                            {
                                var newClassName = config.RenameClassNameTo;
                                var t = findTypes[index];
                                var oldClassName = t.Name;
                                if (t.IsInterface && !newClassName.StartsWith("I"))
                                {
                                    newClassName = newClassName.Insert(0, "I");
                                }

                                newClassName = newClassName + (!t.IsInterface && numberOfChangedIndexes > 0 ? numberOfChangedIndexes.ToString() : "");

                                

                                t.Name = newClassName;
                                if (!t.IsInterface)
                                    numberOfChangedIndexes++;

                                Log($"Remapper: Remapped {oldClassName} to {newClassName}");
                                countOfDefinedMappingSucceeded++;

                                if (config.ConvertInternalMethodsToPublic ?? true)
                                {
                                    foreach (var m in t.Methods)
                                    {
                                        m.IsPublic = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var newClassName = config.RenameClassNameTo;
                            var t = findTypes.First();

                            if (t == null)
                            {
                                Log($"Remapper: Failed to remap {config.RenameClassNameTo} (Default is Null)");
                                countOfDefinedMappingFailed++;
                                continue;
                            }
                            var oldClassName = t.Name;
                            if (t.IsInterface && !newClassName.StartsWith("I"))
                                newClassName = newClassName.Insert(0, "I");

                            t.Name = newClassName;

                            Log($"Remapper: Remapped {oldClassName} to {newClassName}");
                            countOfDefinedMappingSucceeded++;
                        }

                        if (config.RemoveAbstract.HasValue && config.RemoveAbstract.Value)
                        {
                            foreach (var type in findTypes)
                            {
                                if (type.IsAbstract)
                                {
                                    type.IsAbstract = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        Log($"Remapper: Failed to remap {config.RenameClassNameTo}");
                        countOfDefinedMappingFailed++;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Log($"Defined Remapper: SUCCESS: {countOfDefinedMappingSucceeded}");
            Log($"Defined Remapper: FAILED: {countOfDefinedMappingFailed}");
        }

        static void RemapAfterEverything(AssemblyDefinition oldAssembly, RemapperConfig autoRemapperConfig)
        {
            Log($"Remapper: Setting Types to public");
            foreach (var ctf in autoRemapperConfig.ForceTypeToPublic)
            {
                var foundTypes = oldAssembly.MainModule.GetTypes()
                    .Where(x => x.Name.Contains(ctf, StringComparison.OrdinalIgnoreCase));
                foreach (var t in foundTypes)
                {
                    Log(t.FullName + " is now Public");
                    if (!t.IsPublic)
                        t.IsPublic = true;
                }
            }

            if (autoRemapperConfig.RenameEmptyToACS)
            {
                Log($"Remapper: Setting No Namespace to ACS.");
                var emptynamespace = oldAssembly.MainModule.GetTypes()
                       .Where(x => !x.FullName.Contains("."));
                foreach (var t in emptynamespace)
                {
                    if (t.FullName.Contains("<Module>"))
                        continue;
                    t.Namespace = "ACS";
                    foreach (var tn in t.NestedTypes)
                    {
                        tn.Namespace = "ACS";
                    }

                }
            }
            RemapperVoid(oldAssembly, autoRemapperConfig);
            RemapAddSPTUsecAndBear(oldAssembly, autoRemapperConfig);
        }


        static void RemapperVoid(AssemblyDefinition oldAssembly, RemapperConfig config)
        {
            if (config == null)
                return;

            if (config.EnableRemapBrainAndItems == null)
                return;

            if (!config.EnableRemapBrainAndItems.Value)
                return;

            var TypeDefs = oldAssembly.MainModule.GetTypes().ToList();
            if (TypeDefs == null)
                return;
            var brains = TypeDefs.Where(t=> !t.IsAbstract && t.BaseType != null && t.BaseType.Name != "Object" && t.BaseType.Name == "BaseBrain").ToList();
            if (brains.Count != 0)
            {
                foreach (var brain in brains)
                {
                    var method = brain.Methods.Where(y => y.Name == "ShortName").FirstOrDefault();
                    if (method != null)
                    {
                        var name = method.Body.Instructions[0];
                        if (name.Operand == null)
                            continue;
                        if (name.Operand.ToString() == string.Empty)
                            continue;
                        Log(brain.Name + " remapped to " + name.Operand.ToString().Replace(" ", "") + "BotBrain");
                        brain.Name = name.Operand.ToString().Replace(" ", "") + "BotBrain";
                    }
                }
            }

            var biglist = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("Deobfuscator/biglist.json"));

           
            var TemplateTable = TypeDefs.Where(t => t.Name == "TemplateTables").FirstOrDefault();
            if (TemplateTable != null)
            {
                bool IsTemplates = false;
                bool Customization = false;
                var cctor = TemplateTable.Methods.Where(t => t.Name == ".cctor").FirstOrDefault();
                if (cctor == null)
                    return;

                foreach (var instruction in cctor.Body.Instructions)
                {

                    if (instruction.OpCode.Code == Code.Ldstr)
                    {
                        string Id = (string)instruction.Operand;
                        
                        string tpye_from_list = "";
                        if (biglist.ContainsKey(Id))
                        {
                            tpye_from_list = biglist[Id];
                        }
                        else
                        {
                            Log("ERROR, no ID: " + Id);
                            continue;
                        }

                        var next = instruction.Next;
                        if (next.OpCode.Code == Code.Ldtoken)
                        {
                            Log("ID: " + Id);
                            object typ = next.Operand;
                            string type_thing = typ.ToString();
                            
                            if (type_thing.Contains("."))
                            {
                                var t = type_thing.Split(".");
                                type_thing = t.Last();
                            }
                            Log("OldType: " + type_thing);
                            if (type_thing.Contains("Template") && !IsTemplates)
                            {
                                IsTemplates = true;
                                Customization = false;
                            }
                               

                            if (IsTemplates)
                                tpye_from_list = tpye_from_list + "Template";

                            if (type_thing.Contains("Customization"))
                            {
                                Customization = true;
                                continue;
                            }

                            if (Customization)
                                tpye_from_list =  "Customization" + tpye_from_list;

                            Log("NewType: " + tpye_from_list);

                            var renameType = TypeDefs.Where(t => t.Name.Contains(type_thing)).FirstOrDefault();
                            if (renameType != null)
                            {
                                renameType.Name = tpye_from_list;
                            }
                            Log("--------------------------");
                        }
                    }
                }
            }
        }

        static void RemapAddSPTUsecAndBear(AssemblyDefinition assembly, RemapperConfig config)
        {
            if (config == null)
                return;

            if (config.EnableAddSPTUsecBearToDll == null)
                return;


            if (!config.EnableAddSPTUsecBearToDll.Value)
                return;

            long sptUsecValue = 0x29;
            long sptBearValue = 0x2A;

            var botEnums = assembly.MainModule.GetType("EFT.WildSpawnType");

            if (botEnums.Fields.Any(x => x.Name == "sptUsec"))
                return;

            var sptUsec = new FieldDefinition("sptUsec",
                    Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Literal | Mono.Cecil.FieldAttributes.HasDefault,
                    botEnums)
            { Constant = sptUsecValue };

            var sptBear = new FieldDefinition("sptBear",
                    Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Literal | Mono.Cecil.FieldAttributes.HasDefault,
                    botEnums)
            { Constant = sptBearValue };

            botEnums.Fields.Add(sptUsec);
            botEnums.Fields.Add(sptBear);

            Log($"Remapper: Added SPTUsec and SPTBear to EFT.WildSpawnType");
        }

        public static string[] SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "(?<=[a-z])([A-Z])", ",", System.Text.RegularExpressions.RegexOptions.Compiled)
                .Trim().Split(',');
        }
        private static void BackupExistingAssembly(string assemblyPath)
        {
            if (!File.Exists(assemblyPath + ".backup"))
                File.Copy(assemblyPath, assemblyPath + ".backup", false);
        }
        public static void WriteLog(string strLog)
        {
            FileInfo logFileInfo = new FileInfo("log.txt");
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using FileStream fileStream = new FileStream("log.txt", FileMode.Append);
            using StreamWriter log = new StreamWriter(fileStream);
            log.WriteLine(DateTime.Now + " | " + strLog);
        }
    }
}
