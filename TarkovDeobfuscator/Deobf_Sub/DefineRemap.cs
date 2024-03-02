using Mono.Cecil;
using System.Numerics;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class DefineRemap
    {
        static void WriteToDisk(AutoRemapperInfo config, List<TypeDefinition> definitions, int Phase)
        {
            return;
            if (config.RenameClassNameTo == null)
                return;

            if (definitions.Count == 1)
                return;

            if (definitions == StaticAllTypes)
                return;

            /*
            if (!Dict.TryGetValue(config.RenameClassNameTo, out Phase))
            {
                Phase = 0;
            }*/
            foreach (var item in definitions)
            {
                File.AppendAllText("remap_tmp/" + config.RenameClassNameTo + "_"  + Phase + ".txt", item.FullName+ " " + item.MetadataToken.ToInt32()+ "\n");
            }
            /*
            if (Dict.ContainsKey(config.RenameClassNameTo))
                Dict[config.RenameClassNameTo] = Phase;
            else
            {
                Dict.Add(config.RenameClassNameTo, Phase);
            }*/
        }
        static List<TypeDefinition> StaticAllTypes = new();
        internal static void RemapByDefinedConfiguration(AssemblyDefinition oldAssembly, RemapperConfig autoRemapperConfig)
        {
            foreach (var file in Directory.GetFiles("remap_tmp"))
            { 
                File.Delete(file);
            }
            //init for getting stuffs
            RemapInterface.Init(oldAssembly);
            int countOfDefinedMappingSucceeded = 0;
            int countOfDefinedMappingFailed = 0;
            List<TypeDefinition>  AllTypes = oldAssembly.MainModule.GetTypes().OrderBy(x => x.Name).ToList();


            //maybe filter if type is same dont repeat?
            List<TypeDefinition> tmp = new(); 
            foreach (var item in AllTypes)
            {
                int foundThatMF = 0;
                foreach (var tmp_item in tmp)
                {
                    if (tmp_item.MetadataToken.ToInt32() == item.MetadataToken.ToInt32())
                    {
                        foundThatMF += 1;
                    }
                }
                if (foundThatMF == 0)
                    if (!tmp.Contains(item))
                    {
                        //File.AppendAllText("item_added.txt", item.FullName + " " + item.MetadataToken.ToInt32() + "\n");
                        tmp.Add(item);
                    }
            }

            //oldAssembly.MainModule.CustomAttributes.ToList();
           
            AllTypes = tmp;
            StaticAllTypes = AllTypes;

            foreach (var config in autoRemapperConfig.DefinedRemapping.Where(x => !string.IsNullOrEmpty(x.RenameClassNameTo)))
            {

                List<TypeDefinition> typeDefinitions = AllTypes;

                try
                {
                    var ret_def = ClassNameMatching.RemapFullName(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 0);
                    }
                    ret_def = ClassNameMatching.RemapPartialName(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 1);
                    }

                    ret_def = AttributeStuff.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 25);
                    }

                    ret_def = BaseTypeOf.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 24);
                    }

                    ret_def = CheckIs.RemapIsEmpty(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 23);
                    }

                    ret_def = RemapInterface.Remap(config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 2);
                    }
                    ret_def = MustBeGClass.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 3);
                    }
                    ret_def = SearchBaseTypes.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 4);
                    }
                    ret_def = CheckIs.RemapIsInterface(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 5);
                    }
                    ret_def = CheckIs.RemapIsClass(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 6);
                    }
                    ret_def = CheckIs.RemapIsStruct(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 7);
                    }
                    ret_def = CheckIs.RemapIsNested(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 8);
                    }
                    ret_def = CheckIs.RemapIsSealed(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 9);
                    }
                    ret_def = CheckMethods.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 10);
                    }
                    ret_def = CheckMethodsVirtual.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 11);
                    }
                    ret_def = CheckMethodsStatic.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 12);
                    }
                    ret_def = CheckEvents.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 13);
                    }
                    ret_def = CheckField.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 14);
                    }
                    ret_def = CheckFieldStatic.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 15);
                    }
                    ret_def = CheckProperties.Remap(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 16);
                    }
                    ret_def = CheckExact.RemapExactFields(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 17);
                    }
                    ret_def = CheckExact.RemapExactMethods(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 18);
                    }
                    ret_def = CheckExact.RemapExactEvents(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 19);
                    }
                    ret_def = CheckExact.RemapExactInterfaces(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 20);
                    }
                    ret_def = CheckExact.RemapExactProperties(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 21);
                    }
                    ret_def = CheckExact.RemapExactNestedTypes(typeDefinitions, config);
                    if (ret_def.Count != 0)
                    {
                        typeDefinitions = ret_def;
                        WriteToDisk(config, typeDefinitions, 22);
                    }
                    WriteToDisk(config, typeDefinitions, 30);
                    if (typeDefinitions.Any())
                    {
                        
                        var onlyRemapFirstFoundType = config.OnlyRemapFirstFoundType.HasValue && config.OnlyRemapFirstFoundType.Value;
                        if (typeDefinitions.Count() > 1 && !onlyRemapFirstFoundType)
                        {
                            Console.WriteLine(config.RenameClassNameTo + " Has multiple classes to rename!");
                            Deobf.Log(config.RenameClassNameTo + " Has multiple classes to rename!");
                            typeDefinitions = typeDefinitions
                                .OrderBy(x => !x.Name.StartsWith("GClass") && !x.Name.StartsWith("GInterface"))
                                .ThenBy(x => x.Name.StartsWith("GInterface"))
                                .ToList();

                            var numberOfChangedIndexes = 0;
                            for (var index = 0; index < typeDefinitions.Count(); index++)
                            {
                                var newClassName = config.RenameClassNameTo;
                                var t = typeDefinitions[index];
                                var oldClassName = t.Name;
                                if (t.IsInterface && !newClassName.StartsWith("I"))
                                {
                                    newClassName = newClassName.Insert(0, "I");
                                }

                                newClassName = newClassName + (!t.IsInterface && numberOfChangedIndexes > 0 ? numberOfChangedIndexes.ToString() : "");



                                t.Name = newClassName;
                                if (!t.IsInterface)
                                    numberOfChangedIndexes++;

                                Deobf.Log($"Remapper: Remapped {oldClassName} to {newClassName}");
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
                            var t = typeDefinitions.First();

                            if (t == null)
                            {
                                Deobf.Log($"Remapper: Failed to remap {config.RenameClassNameTo} (Default is Null)");
                                countOfDefinedMappingFailed++;
                                continue;
                            }
                            var oldClassName = t.Name;
                            if (t.IsInterface && !newClassName.StartsWith("I"))
                                newClassName = newClassName.Insert(0, "I");

                            t.Name = newClassName;

                            Deobf.Log($"Remapper: Remapped {oldClassName} to {newClassName}");
                            countOfDefinedMappingSucceeded++;
                        }

                        if (config.RemoveAbstract.HasValue && config.RemoveAbstract.Value)
                        {
                            foreach (var type in typeDefinitions)
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
                        Deobf.Log($"Remapper: Failed to remap {config.RenameClassNameTo}");
                        countOfDefinedMappingFailed++;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                
            }
            Deobf.Log($"Defined Remapper: SUCCESS: {countOfDefinedMappingSucceeded}");
            Deobf.Log($"Defined Remapper: FAILED: {countOfDefinedMappingFailed}");
        }
    }
}
