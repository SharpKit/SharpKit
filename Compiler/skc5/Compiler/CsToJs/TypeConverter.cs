using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using System.IO;
using System.Collections;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Extensions;
using SharpKit.Compiler.CsToJs;
using SharpKit.TypeScript;

namespace SharpKit.Compiler.CsToJs
{
    class TypeConverter
    {
        public bool ExportComments { get; set; }
        public CompilerLogger Log { get; set; }
        public CsExternalMetadata ExternalMetadata { get; set; }
        public SkProject Project { get { return Compiler.Project; } }
        public List<JsFile> JsFiles { get; set; }
        public string AssemblyName { get; set; }
        public bool LongFunctionNames { get; set; }
        public Action<Dictionary<JsFile, List<ITypeDefinition>>> BeforeExportTypes;
        public CompilerTool Compiler { get; set; }
        public MemberConverter_Clr ClrConverter;
        public event Action<MemberConverter> ConfigureMemberConverter;

        MemberConverter_Global GlobalConverter;
        MemberConverter_Native NativeConverter;
        MemberConverter_ExtJs ExtJsConverter;

        bool ShouldExportType(ITypeDefinition ce)
        {
            return Sk.IsJsExported(ce);
        }

        int GetOrderInFile(ITypeDefinition ce)
        {
            var att = ce.GetJsTypeAttribute();
            if (att != null)
                return att.OrderInFile;
            return 0;
        }
        bool CanExportExternalType(ITypeDefinition ce)
        {
            return ce.IsEnum() || ce.IsInterface();
        }

        List<ITypeDefinition> GetExternalTypesToExport()
        {
            if (ExternalMetadata == null)
                return null;
            var list2 = ExternalMetadata.TypesWithExternalAttributes.Where(ShouldExportType).ToList();
            var list3 = new List<ITypeDefinition>();
            foreach (var ce in list2)
            {
                if (!CanExportExternalType(ce))
                {
                    var att = ce.Attributes.FindByType<JsTypeAttribute>().Where(t => t.GetDeclaration() != null).FirstOrDefault();
                    if (att != null) //do not give warnings on reference assembly attributes.
                        Log.Warn(att.GetDeclaration(), "Only enums and interfaces can be exported externally, set Export=false on this JsTypeAttribute");
                }
                else
                {
                    list3.Add(ce);
                }
            }
            return list3;
        }

        public void Process()
        {
            List<ITypeDefinition> allTypesToExport = GetAllTypesToExport();

            var byFile = allTypesToExport.GroupBy(ce => Compiler.PathMerger.ConvertRelativePath(Sk.GetExportPath(ce))).ToDictionary();

            byFile.ForEach(t => SortByNativeInheritance(t.Value));
            foreach (var f in byFile)
            {
                var customOrder = f.Value.Where(t => GetOrderInFile(t) != 0).ToList();
                if (customOrder.Count > 0)
                {
                    f.Value.RemoveAll(t => customOrder.Contains(t));
                    customOrder.Sort((x, y) => GetOrderInFile(x) - GetOrderInFile(y));
                    f.Value.InsertRange(0, customOrder.Where(t => GetOrderInFile(t) < 0));
                    f.Value.AddRange(customOrder.Where(t => GetOrderInFile(t) > 0));
                }
            }
            //sort types by OrderInFile if needed:
            //byFile.Where(k => k.Value.Where(t => GetOrderInFile(t) != 0).FirstOrDefault() != null).ForEach(t => t.Value.Sort((x, y) => GetOrderInFile(x) - GetOrderInFile(y)));

            var byFile2 = new Dictionary<JsFile, List<ITypeDefinition>>();
            foreach (var pair in byFile)
            {
                var file = new JsFile { Filename = pair.Key, Units = new List<JsUnit> { new JsUnit { Statements = new List<JsStatement>() } } };
                byFile2.Add(file, pair.Value);
            }
            if (BeforeExportTypes != null)
                BeforeExportTypes(byFile2);
            //export by filenames and order
            byFile2.ForEachParallel(ExportTypesInFile);

            JsFiles = byFile2.Keys.ToList();

            if (Sk.ExportTsHeaders(Compiler.Project.Compilation))
            {
                ExportTsHeaders(allTypesToExport);
            }
        }

        private void ExportTsHeaders(List<ITypeDefinition> types)
        {
            var mc = new TsMemberConverter { TypeConverter = this };
            var list2 = mc.Visit(types);
            using (var writer = new StreamWriter(@"test.ts"))
            {
                var tsw = new TsWriter { Writer = writer };
                list2.ForEach(tsw.Visit);
                writer.Flush();
                writer.Close();
            }



        }

        private List<ITypeDefinition> GetAllTypesToExport()
        {
            //Test();
            var list = Project.Compilation.MainAssembly.GetTypes().Where(ShouldExportType).ToList();
            var withNested = new List<ITypeDefinition>();
            foreach (var ce in list)
            {
                withNested.Add(ce);
                withNested.AddRange(ce.NestedTypes.Where(ShouldExportType));
            }
            list = withNested;
            var list2 = GetExternalTypesToExport();
            if (list2 != null)
                list.AddRange(list2);

            var allTypesToExport = list;
            return allTypesToExport;
        }

        void ExportTypesInFile(KeyValuePair<JsFile, List<ITypeDefinition>> p)
        {
            p.Value.ForEach(t => ConvertTypeDefinition(t, p.Key));
        }

        void SortByNativeInheritance(List<ITypeDefinition> list)
        {
            var list2 = list.Where(t => Sk.IsNativeType(t) && t.GetBaseTypeDefinition() != null && Sk.IsNativeType(t.GetBaseTypeDefinition())).ToList();
            foreach (var ce in list2)
            {
                var ce3 = ce;
                while (true)
                {
                    var baseCe = ce3.GetBaseTypeDefinition();
                    if (baseCe == null)
                        break;
                    MoveBefore(list, ce3, baseCe);
                    ce3 = baseCe;
                }
            }
        }

        static void MoveBefore<T>(List<T> list, T ce, T baseCe)
        {
            var ceIndex = list.IndexOf(ce);
            var baseCeIndex = list.IndexOf(baseCe);
            if (baseCeIndex >= 0 && ceIndex >= 0)
            {
                if (baseCeIndex > ceIndex)
                {
                    list.RemoveAt(baseCeIndex);
                    list.Insert(ceIndex, baseCe);
                }
            }
        }

        void ConvertTypeDefinition(ITypeDefinition ce, JsFile jsFile)
        {
            var unit = ConvertTypeDefinition(ce);
            jsFile.Units[0].Statements.AddRange(unit.Statements);

        }

        MemberConverter GetMemberConverter(ITypeDefinition ce)
        {
            MemberConverter export;
            var isExtJs = Sk.IsExtJsType(ce);
            var isGlobal = Sk.IsGlobalType(ce) && !isExtJs;
            var isNative = Sk.IsNativeType(ce) && !isExtJs;
            if (isGlobal)
            {
                if (GlobalConverter == null)
                    GlobalConverter = new MemberConverter_Global();
                export = GlobalConverter;
            }
            else if (isNative)
            {
                if (NativeConverter == null)
                    NativeConverter = new MemberConverter_Native();
                export = NativeConverter;
            }
            else if (isExtJs)
            {
                if (ExtJsConverter == null)
                    ExtJsConverter = new MemberConverter_ExtJs();
                export = ExtJsConverter;
            }
            else
            {
                if (ClrConverter == null)
                    ClrConverter = new MemberConverter_Clr();
                export = ClrConverter;
            }
            OnConfigureMemberConverter(export);
            return export;
        }

        void OnConfigureMemberConverter(MemberConverter mc)
        {
            if (mc.AstNodeConverter != null)
                return;
            mc.Compiler = Compiler;
            mc.AssemblyName = AssemblyName;
            mc.AstNodeConverter = new AstNodeConverter
            {
                Log = Log,
                ExportComments = ExportComments,
                Compiler = Compiler
            };
            mc.AstNodeConverter.Init();
            mc.LongFunctionNames = LongFunctionNames;
            mc.Log = Log;
            if (ConfigureMemberConverter != null)
                ConfigureMemberConverter(mc);
        }


        JsUnit ConvertTypeDefinition(ITypeDefinition ce)
        {
            var unit = new JsUnit { Statements = new List<JsStatement>() };

            var att = ce.GetJsTypeAttribute();
            if (att != null && att.PreCode != null)
                unit.Statements.Add(Js.CodeStatement(att.PreCode));

            var isGlobal = att != null && att.GlobalObject;
            var isNative = att != null && att.Native;
            var isClr = !isGlobal && !isNative;


            var mc = GetMemberConverter(ce);
            var unit2 = (JsUnit)mc.Visit(ce);
            if (unit2 != null && unit2.Statements.IsNotNullOrEmpty())
                unit.Statements.AddRange(unit2.Statements);
            else
                Log.Warn(ce, "No code was generated for type: " + ce.FullName);

            if (att != null && att.PostCode != null)
                unit.Statements.Add(Js.CodeStatement(att.PostCode));
            return unit;
        }

    }


}
