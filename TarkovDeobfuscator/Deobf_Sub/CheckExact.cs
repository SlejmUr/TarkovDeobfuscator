using Mono.Cecil;

namespace TarkovDeobfuscator.Deobf_Sub
{
    internal class CheckExact
    {
        internal static List<TypeDefinition> RemapExactProperties(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ExactProperties != null && config.ExactProperties.Length != 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Properties.Count == config.ExactProperties.Length)
                    {
                        int okField = 0;
                        for (int i = 0; i < config.ExactProperties.Length; i++)
                        {
                            if (t.Properties[i].Name == config.ExactProperties[i])
                            {
                                Deobf.WriteLog(i + " " + t.Name + " " + t.Properties[i].Name + " == " + config.ExactProperties[i].ToString());
                                okField++;
                            }
                        }
                        if (okField == config.ExactProperties.Length)
                        {
                            Deobf.WriteLog(t.Name + " is found!");
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }

        internal static List<TypeDefinition> RemapExactFields(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ExactFields != null && config.ExactFields.Length != 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Fields.Count == config.ExactFields.Length)
                    {
                        int okField = 0;
                        for (int i = 0; i < config.ExactFields.Length; i++)
                        {
                            if (t.Fields[i].Name == config.ExactFields[i])
                            {
                                Deobf.WriteLog(i + " " + t.Name + " " + t.Fields[i].Name + " == " + config.ExactFields[i].ToString());
                                okField++;
                            }
                        }
                        if (okField == config.ExactFields.Length)
                        {
                            Deobf.WriteLog(t.Name + " is found!");
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }

        internal static List<TypeDefinition> RemapExactMethods(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ExactMethods != null && config.ExactMethods.Length != 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Methods.Count == config.ExactMethods.Length)
                    {
                        int okField = 0;
                        for (int i = 0; i < config.ExactMethods.Length; i++)
                        {
                            if (t.Methods[i].Name == config.ExactMethods[i])
                            {
                                Deobf.WriteLog(i + " " + t.Name + " " + t.Methods[i].Name + " == " + config.ExactMethods[i].ToString());
                                okField++;
                            }
                        }
                        if (okField == config.ExactMethods.Length)
                        {
                            Deobf.WriteLog(t.Name + " is found!");
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }

        internal static List<TypeDefinition> RemapExactEvents(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ExactEvents != null && config.ExactEvents.Length != 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Events.Count == config.ExactEvents.Length)
                    {
                        int okField = 0;
                        for (int i = 0; i < config.ExactEvents.Length; i++)
                        {
                            if (t.Events[i].Name == config.ExactEvents[i])
                            {
                                Deobf.WriteLog(i + " " + t.Name + " " + t.Events[i].Name + " == " + config.ExactEvents[i].ToString());
                                okField++;
                            }
                        }
                        if (okField == config.ExactEvents.Length)
                        {
                            Deobf.WriteLog(t.Name + " is found!");
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }

        internal static List<TypeDefinition> RemapExactInterfaces(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ExactInterfaces != null && config.ExactInterfaces.Length != 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.Interfaces.Count == config.ExactInterfaces.Length)
                    {
                        int okField = 0;
                        for (int i = 0; i < config.ExactInterfaces.Length; i++)
                        {
                            if (t.Interfaces[i].InterfaceType.Name == config.ExactInterfaces[i])
                            {
                                Deobf.WriteLog(i + " " + t.Name + " " + t.Interfaces[i].InterfaceType.Name + " == " + config.ExactInterfaces[i].ToString());
                                okField++;
                            }
                        }
                        if (okField == config.ExactInterfaces.Length)
                        {
                            Deobf.WriteLog(t.Name + " is found!");
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }

        internal static List<TypeDefinition> RemapExactNestedTypes(List<TypeDefinition> types, AutoRemapperInfo config)
        {
            if (config.ExactNestedTypes != null && config.ExactNestedTypes.Length != 0)
            {
                List<TypeDefinition> returner = new();
                foreach (var t in types)
                {
                    if (t.NestedTypes.Count == config.ExactNestedTypes.Length)
                    {
                        int okField = 0;
                        for (int i = 0; i < config.ExactNestedTypes.Length; i++)
                        {
                            if (t.NestedTypes[i].Name == config.ExactNestedTypes[i])
                            {
                                Deobf.WriteLog(i + " " + t.Name + " " + t.NestedTypes[i].Name + " == " + config.ExactNestedTypes[i].ToString());
                                okField++;
                            }
                        }
                        if (okField == config.ExactNestedTypes.Length)
                        {
                            Deobf.WriteLog(t.Name + " is found!");
                            returner.Add(t);
                        }
                    }
                }
                return returner;
            }
            return types;
        }
    }
}
