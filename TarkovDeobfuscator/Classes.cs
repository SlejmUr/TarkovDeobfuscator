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
        public string[] TypesToConvertConstructorsToPublic { get; set; }
        public string[] ForceTypeToPublic { get; set; }
        public bool ForceAllToPublic { get; set; }
        public bool RenameEmptyToACS { get; set; }
        public bool? EnableAddSPTUsecBearToDll { get; set; }
        public bool? EnableRemapBrainAndItems { get; set; }
    }
    public class AutoRemapperInfo
    {
        public string RenameClassNameTo { get; set; }
        public string ClassName { get; set; }
        public string ClassNameFull { get; set; }
        public bool? OnlyTargetInterface { get; set; } //not implemented
        public bool? IsClass { get; set; }
        public bool? IsInterface { get; set; }
        public bool? IsStruct { get; set; }
        public string[] HasFields { get; set; }
        public string[] ExactFields { get; set; }
        public string[] HasProperties { get; set; }
        public string[] HasFieldsStatic { get; set; }// AKA const
        public string[] ExactProperties { get; set; }
        public string[] HasMethods { get; set; }
        public string[] ExactMethods { get; set; }
        public string[] HasMethodsVirtual { get; set; }
        public string[] ExactMethodsVirtual { get; set; } //This will be a little bit more "fun" to implement //not implemented
        public string[] HasMethodsStatic { get; set; }
        public string[] HasEvents { get; set; }
        public string[] ExactEvents { get; set; }
        public string[] HasConstructorArgs { get; set; }
        public string[] HasInterfaces { get; set; }
        public string[] ExactInterfaces { get; set; }
        public string[] HasNestedTypes { get; set; }
        public string[] ExactNestedTypes { get; set; }
        public string? BaseType { get; set; }
        public bool? IsNested { get; set; }
        public bool? IsAbstract { get; set; }
        public bool? IsSealed { get; set; }

        // SIT
        public bool? ConvertInternalMethodsToPublic { get; set; }
        public bool? OnlyRemapFirstFoundType { get; set; }
        public bool? MustBeGClass { get; set; }
        public bool? RemoveAbstract { get; set; }
        public string? InheritsClass { get; set; }
        public string? IsNestedInClass { get; set; }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(RenameClassNameTo) ? RenameClassNameTo : base.ToString();
        }
    }
}
