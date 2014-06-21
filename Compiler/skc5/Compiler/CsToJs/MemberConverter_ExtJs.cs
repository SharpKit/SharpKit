using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.JavaScript.Ast;

namespace SharpKit.Compiler.CsToJs
{
    class MemberConverter_ExtJs : MemberConverter_Clr
    {
        protected override JsUnit OnAfterExportType(ITypeDefinition ce, JsClrType jsType)
        {
            var extJsType = new ExtJsType
            {
                extend = jsType.baseTypeName,
                statics = jsType.staticDefinition,
            };
            //if (extJsType.extend == "System.Object")
            //    extJsType.extend = null;
            var json = (JsJsonObjectExpression)Serialize(extJsType);
            //var ctor = json.NamesValues.Where(t => t.Name.Name == "ctor").FirstOrDefault();
            if (jsType.definition == null)
                jsType.definition = new JsObject();
            var ctor = jsType.definition.TryGetValue("ctor");
            if (ctor != null)
            {
                jsType.definition.Remove("ctor");
                jsType.definition.Add("constructor", ctor);
                //ctor.Name.Name = "constructor";
                //var func = ctor.Value as JsFunction;
                //if (func != null)
                //{
                //    func.Block.Statements.Insert(0, Js.This().Member("callSuper").Invoke(Js.Member("arguments")).Statement());
                //}
            }
            foreach (var me in jsType.definition)
            {
                var name = me.Key;
                var value = (JsExpression)me.Value;
                if (json.NamesValues == null)
                    json.NamesValues = new List<JsJsonNameValue>();
                json.NamesValues.Add(new JsJsonNameValue { Name = new JsJsonMember { Name = name }, Value = value });
            }

            var define = Js.Members("Ext.define").Invoke(Js.String(jsType.fullname), json).Statement();
            var unit = new JsUnit { Statements = new List<JsStatement> { define } };
            return unit;// base.OnAfterExportType(ce, jsType);
        }

        protected override JsNode ExportPropertyInfo(IProperty pe)
        {
            return null;
        }
        protected override IType GetBaseClassIfValid(ITypeDefinition ce, bool recursive)
        {
            var baseCe = base.GetBaseClassIfValid(ce, recursive);
            if (baseCe == null)
                return null;
            if (baseCe.FullName == "System.Object")
                return null;
            return baseCe;
        }
    }

    class ExtJsType
    {
        /// <summary>
        /// List of short aliases for class names. ...
        /// </summary>
        public string alias { get; set; }
        /// <summary>
        /// Defines alternate names for this class. ...
        /// </summary>
        public object alternateClassName { get; set; }
        /// <summary>
        /// List of configuration options with their default values, for which automatically
        /// accessor methods are generated. ...
        /// </summary>
        public object config { get; set; }
        /// <summary>
        /// The parent class that this class extends. ...
        /// </summary>
        public string extend { get; set; }
        /// <summary>
        /// List of inheritable static methods for this class. ...
        /// </summary>
        public object inheritableStatics { get; set; }
        /// <summary>
        /// List of classes to mix into this class. ...
        /// </summary>
        public object mixins { get; set; }
        /// <summary>
        /// List of classes that have to be loaded before instanciating this class. ...
        /// </summary>
        public string requires { get; set; }
        /// <summary>
        /// When set to true, the class will be instanciated as singleton. ...
        /// </summary>
        public bool? singleton { get; set; }
        /// <summary>
        /// List of static methods for this class. ...
        /// </summary>
        public object statics { get; set; }
        /// <summary>
        /// List of classes to load together with this class. ...
        /// </summary>
        public string uses { get; set; }
    }

}
