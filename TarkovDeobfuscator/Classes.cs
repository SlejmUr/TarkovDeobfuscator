﻿namespace TarkovDeobfuscator
{
    public class RemapperConfig
    {
        public bool EnableAutomaticRemapping { get; set; }
        public bool EnableDefinedRemapping { get; set; }
        public AutoRemapperInfo[] DefinedRemapping { get; set; }
        public bool EnableForceAllTypesPublic { get; set; }
        public string[] DefinedTypesToForcePublic { get; set; }
        public string[] TypesToForceAllPublicMethods { get; set; }
        public string[] TypesToForceAllPublicFieldsAndProperties { get; set; }
    }
    public class AutoRemapperInfo
    {
        public string RenameClassNameTo { get; set; }
        public string ClassName { get; set; }
        public string ClassFullNameContains { get; set; }
        public bool? OnlyTargetInterface { get; set; }
        public bool? IsClass { get; set; }
        public bool? IsInterface { get; set; }
        public bool? IsStruct { get; set; }
        public bool HasExactFields { get; set; }
        public string[] HasFields { get; set; }
        public string[] HasProperties { get; set; }
        public string[] HasMethods { get; set; }
        public string[] HasMethodsVirtual { get; set; }
        public string[] HasEvents { get; set; }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(RenameClassNameTo) ? RenameClassNameTo : base.ToString();
        }
    }
}