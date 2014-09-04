using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirrored.SharpKit.JavaScript
{
    #region JsTypeAttribute
    ///<summary>
    /// Sets the rules that SharpKit will use when converting this .NET type into JavaScript.
    /// This attribute can be used on any type, and as assembly attribute, when used as an assembly attribute, and TargetType is not supplied, it will affect ALL types in the assembly
    ///</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Assembly, AllowMultiple = true)]
    public partial class JsTypeAttribute : Attribute
    {
        /// <summary>
        /// Creates an instance of a JsTypeAttribute
        /// </summary>
        public JsTypeAttribute()
        {
        }
        /// <summary>
        /// Creates an instance of a JsTypeAttribute in the specified JsMode
        /// </summary>
        /// <param name="mode"></param>
        public JsTypeAttribute(JsMode mode)
        {
            Mode = mode;
        }
        /// <summary>
        /// Creates an instance of a JsTypeAttribute in the specified JsMode, and exported to the specified filename
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="filename"></param>
        public JsTypeAttribute(JsMode mode, string filename)
        {
            Mode = mode;
            Filename = filename;
        }

        /// <summary>
        /// Js code that will be written before exporting the type
        /// </summary>
        public string PreCode { get; set; }
        /// <summary>
        /// Js code that will be written after exporting the type
        /// </summary>
        public string PostCode { get; set; }
        /// <summary>
        /// Precendece between JsTypes in the same file, negative values will put the type before other types, and positive value will put it after other types
        /// </summary>
        public int OrderInFile { get; set; }
        ///// <summary>
        ///// When used as assembly attribute, indicates the type for which to apply this attribute on.
        ///// This feature should be used when trying to describe classes on external assemblies that has no SharpKit support
        ///// </summary>
        //public IType TargetType { get; set; }
        /// <summary>
        /// When used as assembly attribute, indicates the type for which to apply this attribute on.
        /// This feature should be used when trying to describe classes on external assemblies that has no SharpKit support
        /// </summary>
        public string TargetTypeName { get; set; }
        /// <summary>
        /// When set to true - SharpKit will ignore all params[] keywords in all methods
        /// </summary>
        public bool NativeParams { get { return _NativeParams.GetValueOrDefault(); } set { _NativeParams = value; } } public bool? _NativeParams;

        ///<summary>
        ///Indicates that all delegate parameters in all members are native javascript functions
        ///</summary>
        public bool NativeDelegates { get { return _NativeDelegates.GetValueOrDefault(); } set { _NativeDelegates = value; } } public bool? _NativeDelegates;
        ///<summary>
        ///When true, omits all casts to this type
        ///</summary>
        public bool OmitCasts { get { return _OmitCasts.GetValueOrDefault(); } set { _OmitCasts = value; } } public bool? _OmitCasts;
        /// <summary>
        /// Forces all properties to be treated as JavaScript native fields without any getter and setter methods
        /// </summary>
        public bool PropertiesAsFields { get { return _PropertiesAsFields.GetValueOrDefault(); } set { _PropertiesAsFields = value; } } public bool? _PropertiesAsFields;
        /// <summary>
        /// Forces all automatic properties to be treated as JavaScript native fields without any getter and setter methods
        /// </summary>
        public bool AutomaticPropertiesAsFields { get { return _AutomaticPropertiesAsFields.GetValueOrDefault(); } set { _AutomaticPropertiesAsFields = value; } } public bool? _AutomaticPropertiesAsFields;
        ///<summary>
        ///When true, foreach statements will use the for..in syntax of Javascript
        ///</summary>
        public bool NativeEnumerator { get { return _NativeEnumerator.GetValueOrDefault(); } set { _NativeEnumerator = value; } } public bool? _NativeEnumerator;
        ///<summary>
        ///When true, foreach statements will use the for loop syntax of Javascript
        ///</summary>
        public bool NativeArrayEnumerator { get { return _NativeArrayEnumerator.GetValueOrDefault(); } set { _NativeArrayEnumerator = value; } } public bool? _NativeArrayEnumerator;
        ///<summary>
        ///When true, instanciations of this class will use the native Javascript method, rather than calling a constructor
        ///</summary>
        public bool NativeConstructors { get { return _NativeConstructors.GetValueOrDefault(); } set { _NativeConstructors = value; } } public bool? _NativeConstructors;
        ///<summary>
        ///When true, instanciations of this class will use the native Javascript method, rather than calling a constructor
        ///</summary>
        public bool NativeOverloads { get { return _NativeOverloads.GetValueOrDefault(); } set { _NativeOverloads = value; } } public bool? _NativeOverloads;
        ///<summary>
        ///When true, operator overloads will be exported as native js binary operations:
        ///if System.Int32 has a '/' operator overload then:
        ///x / y -> System.Int32.op_div(x, y);   //NativeOperatorOverloads=false
        ///x / y -> x / y;              //NativeOperatorOverloads=true
        ///</summary>
        public bool NativeOperatorOverloads { get { return _NativeOperatorOverloads.GetValueOrDefault(); } set { _NativeOperatorOverloads = value; } } public bool? _NativeOperatorOverloads;
        ///<summary>
        ///Indicates that this type will be exported as native js type, 
        ///only one constructor is allowed, 
        ///all instance members will be exported to the constructor's prototype
        ///all static members will be exported to the constructor's members
        ///</summary>
        public bool Native { get { return _Native.GetValueOrDefault(); } set { _Native = value; } } public bool? _Native;
        ///<summary>
        ///When set, the class methods and properties will be declared on the window object, instead of a class.
        ///</summary>
        public bool GlobalObject { get { return _GlobalObject.GetValueOrDefault(); } set { _GlobalObject = value; } } public bool? _GlobalObject;
        ///<summary>
        ///Any anonymous delegate creation will be exported as a native inline function in javascript
        ///</summary>
        public bool NativeFunctions { get { return _NativeFunctions.GetValueOrDefault(); } set { _NativeFunctions = value; } } public bool? _NativeFunctions;
        ///<summary>
        ///Anonymous objects will be created and treated as Json objects
        ///</summary>
        public bool NativeJsons { get { return _NativeJsons.GetValueOrDefault(); } set { _NativeJsons = value; } } public bool? _NativeJsons;
        /// <summary>
        ///	Indicates that SharpKit compiler will generate javascript code for this type / member
        /// This property is inherited and applied to all derived types. Default value is true
        /// </summary>
        public bool Export { get { return _Export.GetValueOrDefault(true); } set { _Export = value; } } public bool? _Export;
        ///<summary>
        ///When set, changes the type name in the client code
        ///</summary>
        public string Name { get; set; }
        ///<summary>
        ///The target filename to generate the javascript code into, when using a relative path, it will be relative to the current cs file,
        ///You may use the ~  (tilda) operator to designate the project directory
        ///</summary>
        public string Filename { get; set; }
        ///<summary>
        ///The type of js exporter to use
        ///</summary>
        public string Exporter { get; set; }
        /// <summary>
        /// Omits and ignores any generic type argument
        /// </summary>
        public bool IgnoreGenericTypeArguments { get { return _IgnoreGenericTypeArguments.GetValueOrDefault(); } set { _IgnoreGenericTypeArguments = value; } } public bool? _IgnoreGenericTypeArguments;
        /// <summary>
        /// Omits and ignores any generic argument in any method inside the class (Equivalent to JsMethod(IgnoreGenericArguments=true) on each method)
        /// </summary>
        public bool IgnoreGenericMethodArguments { get { return _IgnoreGenericMethodArguments.GetValueOrDefault(); } set { _IgnoreGenericMethodArguments = value; } } public bool? _IgnoreGenericMethodArguments;
        /// <summary>
        /// Controls the JavaScript mode of this type, a mode is a combination of several boolean settings of the JsTypeAttribute
        /// </summary>
        public JsMode Mode { get { return _Mode.GetValueOrDefault(); } set { _Mode = value; ApplyJsMode(); } } public JsMode? _Mode;
        /// <summary>
        /// Forces any delegate used in the current type to be created a native js function without an instance context
        /// </summary>
        public bool ForceDelegatesAsNativeFunctions { get { return _ForceDelegatesAsNativeFunctions.GetValueOrDefault(); } set { _ForceDelegatesAsNativeFunctions = value; } } public bool? _ForceDelegatesAsNativeFunctions;
        /// <summary>
        /// Declares fields and PropertiesAsFields in object definition, and does not move them to the constructor
        /// </summary>
        public bool InlineFields { get { return _InlineFields.GetValueOrDefault(); } set { _InlineFields = value; } } public bool? _InlineFields;
        /// <summary>
        /// Omits optional parameters in methods, when not sent by caller
        /// </summary>
        public bool OmitOptionalParameters { get { return _OmitOptionalParameters.GetValueOrDefault(); } set { _OmitOptionalParameters = value; } } public bool? _OmitOptionalParameters;
        /// <summary>
        /// Specifies whether or not to declare inheritance of a class to a base class
        /// </summary>
        public bool OmitInheritance { get { return _OmitInheritance.GetValueOrDefault(); } set { _OmitInheritance = value; } } public bool? _OmitInheritance;
        /// <summary>
        /// When set to true, if no instance constructor was defined in this class, no constructor will be generated.
        /// This is helpful when extending existing classes, and avoiding overwriting existing constructors
        /// </summary>
        public bool OmitDefaultConstructor { get { return _OmitDefaultConstructor.GetValueOrDefault(); } set { _OmitDefaultConstructor = value; } } public bool? _OmitDefaultConstructor;
        /// <summary>
        /// Targets the attribute for a specific SharpKit version, this attribute will be ignored if the current 
        /// SharpKit version isn't in the range of the value specified
        /// </summary>
        public string SharpKitVersion { get; set; }
        /// <summary>
        /// Allows to customize the 'prototype' token for instance method, by default, this value is: "prototype"
        /// This property is useful for creating jQuery plugins
        /// class MyClass
        /// {
        ///     public void foo()
        /// }
        /// 
        /// MyClass.prototype.foo = function()
        /// {
        /// }
        /// </summary>
        public string PrototypeName { get; set; }

        /// <summary>
        /// Treats the class as a native "Error" object and prevents smart exception throwing
        /// </summary>
        public bool NativeError { get { return _NativeError.GetValueOrDefault(); } set { _NativeError = value; } } public bool? _NativeError;
        /// <summary>
        /// When set to true, casting/as/is will be generated using the native "instanceof" keyword
        /// </summary>
        public bool NativeCasts { get { return _NativeCasts.GetValueOrDefault(); } set { _NativeCasts = value; } } public bool? _NativeCasts;

        /// <summary>
        /// When true, Object.defineProperty is used for properties. The browser needs to support native property.
        /// </summary>
        public bool NativeProperties { get { return _NativeProperties.GetValueOrDefault(); } set { _NativeProperties = value; } } public bool? _NativeProperties;

        /// <summary>
        /// The NativePropertyEnumerable property attribute defines whether the property shows up in a for...in loop and Object.keys() or not.
        /// </summary>
        public bool NativePropertiesEnumerable { get { return _NativePropertiesEnumerable.GetValueOrDefault(); } set { _NativePropertiesEnumerable = value; } } public bool? _NativePropertiesEnumerable;

        /// <summary>
        /// The JavaScript code that will be generated when this type is passed as a generic argument to a generic method or generic type
        /// </summary>
        public string GenericArgumentJsCode { get { return _GenericArgumentJsCode; } set { _GenericArgumentJsCode = value; } } public string _GenericArgumentJsCode;

        public string JsonTypeFieldName { get; set; }

        public bool ForceMethodSuffix { get { return _ForceMethodSuffix.GetValueOrDefault(); } set { _ForceMethodSuffix = value; } } public bool? _ForceMethodSuffix;

        private void GoNative()
        {
            if (_NativeOverloads == null)
                NativeOverloads = true;
            if (_NativeDelegates == null)
                NativeDelegates = true;
            if (_AutomaticPropertiesAsFields == null)
                AutomaticPropertiesAsFields = true;
            if (_NativeConstructors == null)
                NativeConstructors = true;
            if (_NativeEnumerator == null)
                NativeEnumerator = true;
            if (_NativeFunctions == null)
                NativeFunctions = true;
            if (_NativeJsons == null)
                NativeJsons = true;
            if (_IgnoreGenericTypeArguments == null)
                _IgnoreGenericTypeArguments = true;
            if (_IgnoreGenericMethodArguments == null)
                IgnoreGenericMethodArguments = true;
            if (_InlineFields == null)
                InlineFields = false;
        }
        private void ApplyJsMode()
        {
            if (_Mode == JsMode.Global)
            {
                GlobalObject = true;
                GoNative();
            }
            else if (_Mode == JsMode.Prototype)
            {
                Native = true;
                GoNative();
                if (_NativeCasts == null)
                    _NativeCasts = true;
            }
            else if (_Mode == JsMode.Clr)
            {

            }
            else if (_Mode == JsMode.ExtJs)
            {
                if (_Native == null)
                    Native = true;
                if (_NativeConstructors == null)
                    NativeConstructors = true;
                if (_InlineFields == null)
                    InlineFields = false;
                if (_OmitDefaultConstructor == null)
                    _OmitDefaultConstructor = true;
            }
            else if (_Mode == JsMode.Json)
            {
                Native = true;
                Export = false;
                PropertiesAsFields = true;
                GoNative();

                //if (_OmitCasts == null)
                //    OmitCasts = true;
            }
        }

    }
    #endregion

    #region JsMode
    ///<summary>
    ///Specifies the export and interoperability mode of a C# type in JavaScript
    ///</summary>
    public enum JsMode
    {
        /// <summary>
        /// Specifies a global function export mode, in which only static members are allowed,
        /// static methods become global functions
        /// static fields become global variables
        /// static constrctor becomes global code
        /// </summary>
        Global,
        /// <summary>
        /// Specifies a prototype object export mode, in which a single constructor is allowed, and both static and instance members.
        /// constructor becomes a constructor function
        /// instance members become the equivalent members on the constructor function's prototype.
        /// static members become members on the constructor function itself.
        /// </summary>
        Prototype,
        /// <summary>
        /// Specifies a .NET style class, in which all C# elements are supported,
        /// this mode requires JsClr library to be included on the client at runtime.
        /// </summary>
        Clr,
        /// <summary>
        /// Specifies an invisible unexported json type, this class will not be exported, 
        /// instantiation and usage of classes in this mode, will be exported to simple json elements.
        /// </summary>
        Json,
        ExtJs,
    }

    #endregion

    #region JsExportAttribute
    /// <summary>
    /// Provides information regarding how SharpKit will export JavaScript code
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public partial class JsExportAttribute : Attribute
    {
        /// <summary>
        /// Specifies whether to include C# comments in the JavaScript code
        /// </summary>
        public bool ExportComments { get { return _ExportComments.GetValueOrDefault(); } set { _ExportComments = value; } } public bool? _ExportComments;
        /// <summary>
        /// Specifies whether SharpKit should minify the exported files
        /// </summary>
        public bool Minify { get { return _Minify.GetValueOrDefault(); } set { _Minify = value; } } public bool? _Minify;
        /// <summary>
        /// Instructs SharpKit to provide any exported function with a global unique name, to help debugging of client side code.
        /// </summary>
        public bool LongFunctionNames { get { return _LongFunctionNames.GetValueOrDefault(); } set { _LongFunctionNames = value; } } public bool? _LongFunctionNames;
        /// <summary>
        /// Injects profiling code into JavaScript functions to enable performance profiling.
        /// </summary>
        public bool EnableProfiler { get { return _EnableProfiler.GetValueOrDefault(); } set { _EnableProfiler = value; } } public bool? _EnableProfiler;

        /// <summary>
        /// Specifies prefix / suffix to any filename exported by SharpKit, e.g.: FilenameFormat="js/{0}"
        /// </summary>
        public string FilenameFormat { get; set; }

        /// <summary>
        /// Specifies default js filename for all types that didn't specify a specific js file for export
        /// If not specified, default file will be res/[AssemblyName].js
        /// </summary>
        public string DefaultFilename { get; set; }

        /// <summary>
        /// Specifies to use the .cs filename for all types that didn't specify a specific js file for export,
        /// MyTextBox.cs -> MyTextBox.js
        /// </summary>
        public bool DefaultFilenameAsCsFilename { get { return _DefaultFilenameAsCsFilename.GetValueOrDefault(); } set { _DefaultFilenameAsCsFilename = value; } } public bool? _DefaultFilenameAsCsFilename;

        /// <summary>
        /// Specifies to inject all available helper methods generated by SharpKit into a single separate file, this feature is supported by SharpKit 5 only.
        /// </summary>
        public string CodeInjectionFilename { get; set; }

        /// <summary>
        /// Specifies to generate chrome source maps, which enables debugging of JavaScript in C#.
        /// To enable this feature you must add a handler in your web.config file:
        /// <code>
        /// &lt;add name="SourceMapsHandler" type="SharpKit.Web.Server.Handlers.SourceMapsHandler, SharpKit.Web" verb="*" path="SourceMaps.ashx" /&gt;
        /// </code>
        /// And enable source maps in chrome: show development bar, click options wheel, check enable source maps.
        /// </summary>
        public bool GenerateSourceMaps { get { return _GenerateSourceMaps.GetValueOrDefault(); } set { _GenerateSourceMaps = value; } } public bool? _GenerateSourceMaps;
        /// <summary>
        /// Instructs SharpKit to use '===' and '!==' (exact equals/not equals) instead of '==' / '!=' when checking equality between objects.
        /// Custom exact equality can be achieved by using the extension method JsContext.ExactEquals
        /// <example>
        /// <code>
        /// var x = "true";
        /// var y = true;
        /// var b = x.ExactEquals(y);
        /// </code>
        /// </example>
        /// </summary>
        public bool UseExactEquals { get { return _UseExactEquals.GetValueOrDefault(); } set { _UseExactEquals = value; } } public bool? _UseExactEquals;

        /// <summary>
        /// Uses the ECMAScript5 strict mode.
        /// </summary>
        public bool UseStrict { get { return _UseStrict.GetValueOrDefault(); } set { _UseStrict = value; } } public bool? _UseStrict;

        /// <summary>
        /// Removes the 'Generated by SharpKit 5 v...' header comment in js files
        /// </summary>
        public bool OmitSharpKitHeaderComment { get { return _OmitSharpKitHeaderComment.GetValueOrDefault(); } set { _OmitSharpKitHeaderComment = value; } } public bool? _OmitSharpKitHeaderComment;

        /// <summary>
        /// Add the generation time to the sharpkit header comment.
        /// CAUTION: This will change your file's content on every compilation. This could occur unnecessary conflics with version control software!
        /// </summary>
        public bool AddTimeStampInSharpKitHeaderComment { get { return _AddTimeStampInSharpKitHeaderComment.GetValueOrDefault(); } set { _AddTimeStampInSharpKitHeaderComment = value; } } public bool? _AddTimeStampInSharpKitHeaderComment;

        /// <summary>
        /// Overrides all OmitCasts definitions in the assembly, and skips any code generation for casting
        /// </summary>
        public bool ForceOmitCasts { get { return _ForceOmitCasts.GetValueOrDefault(); } set { _ForceOmitCasts = value; } } public bool? _ForceOmitCasts;

        public bool ForceIntegers { get { return _ForceIntegers.GetValueOrDefault(); } set { _ForceIntegers = value; } } public bool? _ForceIntegers;

        public bool ExportTsHeaders { get { return _ExportTsHeaders.GetValueOrDefault(); } set { _ExportTsHeaders = value; } } public bool? _ExportTsHeaders;

        public string JsCodeFormat { get; set; }
    }
    #endregion

    #region JsMergedFileAttribute
    ///<summary>
    ///Instructs SharpKit Compiler to create a merged file from specified sources
    ///</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public partial class JsMergedFileAttribute : Attribute
    {
        ///<summary>
        ///The source files to merge
        ///</summary>
        public string[] Sources { get; set; }
        ///<summary>
        ///The target merged file name
        ///</summary>
        public string Filename { get; set; }

        /// <summary>
        /// Specifies whether to minify the js file using js or css minification
        /// minification type will be determined by file extension (.js/.css)
        /// </summary>
        public bool Minify { get { return _Minify.GetValueOrDefault(); } set { _Minify = value; } } public bool? _Minify;
    }
    #endregion

    #region JsMethodAttribute
    ///<summary>
    /// Specifies custom instructions for SharpKit for a single method, this information is used when exporting the member, and when using it.
    ///</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public partial class JsMethodAttribute : Attribute
    {
        /// <summary>
        /// When set to true, SharpKit will ignore the params[] keyword on the method parameter
        /// </summary>
        public bool NativeParams { get { return _NativeParams.GetValueOrDefault(); } set { _NativeParams = value; } } public bool? _NativeParams;
        /// <summary>
        /// Applies the attribute externally on a method, if the method has overloads, attribute will be applied on all of them
        /// </summary>
        public string TargetMethod { get; set; }
        ///// <summary>
        ///// Applies the attribute externally on a type
        ///// </summary>
        //public Type TargetType { get; set; }
        /// <summary>
        /// Applies the attribute externally on a type
        /// </summary>
        public string TargetTypeName { get; set; }
        ///<summary>
        ///Tells the compiler to omit calls to this method and assume that it was invoked
        ///Extension methods:  s.DoSomething() ==> s
        ///</summary>
        public bool OmitCalls { get { return _OmitCalls.GetValueOrDefault(); } set { _OmitCalls = value; } } public bool? _OmitCalls;
        /// <summary>
        /// Instructs SharpKit to use a custom name for a method, SharpKit will use this name when exporting the method, and when invoking it.
        /// </summary>
        public string Name { get; set; }
        ///<summary>
        ///Tells the compiler to drop the method call and write the inline code instead.
        ///Only available for extention methods.
        ///object.SomeExtentionMethod(param) with InlineCode="object==param" ==> object==param
        ///</summary>
        public string InlineCode { get; set; }
        ///<summary>
        ///Custom javascript code implementation for this method
        ///</summary>
        public string Code { get; set; }
        ///<summary>
        ///When set to true - disables the overloading mechanism 
        ///and assumes that the overloads are implemented in a single javascript method with this name
        ///</summary>
        public bool NativeOverloads { get { return _NativeOverloads.GetValueOrDefault(); } set { _NativeOverloads = value; } } public bool? _NativeOverloads;
        ///<summary>
        ///Ignores the generic arguments passed to a method when invoking it in JavaScript
        ///</summary>
        public bool IgnoreGenericArguments { get { return _IgnoreGenericArguments.GetValueOrDefault(); } set { _IgnoreGenericArguments = value; } } public bool? _IgnoreGenericArguments;
        ///<summary>
        ///Marks this extension method as an instance method in javascript
        ///</summary>
        public bool ExtensionImplementedInInstance { get { return _ExtensionImplementedInInstance.GetValueOrDefault(); } set { _ExtensionImplementedInInstance = value; } } public bool? _ExtensionImplementedInInstance;
        /// <summary>
        /// Treats delegates inside this method as native javascript functions.
        /// </summary>
        public bool NativeDelegates { get { return _NativeDelegates.GetValueOrDefault(); } set { _NativeDelegates = value; } } public bool? _NativeDelegates;
        /// <summary>
        /// Forces any delegate used in the current method to be created a native js function without an instance context
        /// </summary>
        public bool ForceDelegatesAsNativeFunctions { get { return _ForceDelegatesAsNativeFunctions.GetValueOrDefault(); } set { _ForceDelegatesAsNativeFunctions = value; } } public bool? _ForceDelegatesAsNativeFunctions;
        ///<summary>
        ///Generates the method code as global JavaScript code, without any function
        ///</summary>
        public bool GlobalCode { get { return _GlobalCode.GetValueOrDefault(); } set { _GlobalCode = value; } } public bool? _GlobalCode;
        ///<summary>
        ///Indicates that this method is global, if exported, it will be exported as a global function, and when invoked, it will be invoked without a class name prefix
        ///</summary>
        public bool Global { get { return _Global.GetValueOrDefault(); } set { _Global = value; } } public bool? _Global;
        /// <summary>
        ///	Indicates that SharpKit compiler will generate javascript code for this type / member
        /// This property is inherited and applied to all derived types. Default value is true
        /// </summary>
        public bool Export { get { return _Export.GetValueOrDefault(true); } set { _Export = value; } } public bool? _Export;
        /// <summary>
        /// Omits the paranthesis () when invoking this method, used for javascript keywords (e.g.: delete)
        /// </summary>
        public bool OmitParanthesis { get { return _OmitParanthesis.GetValueOrDefault(); } set { _OmitParanthesis = value; } } public bool? _OmitParanthesis;
        /// <summary>
        /// Omits the dot (.) operator before the instance name, when invoking this method, used for javascript keywords (e.g.: instanceof)
        /// </summary>
        public bool OmitDotOperator { get { return _OmitDotOperator.GetValueOrDefault(); } set { _OmitDotOperator = value; } } public bool? _OmitDotOperator;
        /// <summary>
        /// Omits optional parameters, when not sent by caller
        /// </summary>
        public bool OmitOptionalParameters { get { return _OmitOptionalParameters.GetValueOrDefault(); } set { _OmitOptionalParameters = value; } } public bool? _OmitOptionalParameters;
        /// <summary>
        /// Omits the new operator when creating new instances on a consutrctor
        /// </summary>
        public bool OmitNewOperator { get { return _OmitNewOperator.GetValueOrDefault(); } set { _OmitNewOperator = value; } } public bool? _OmitNewOperator;
        /// <summary>
        /// Allows an instance method to be invoke like an extension method
        /// x.Call() -> Call(x)
        /// </summary>
        public bool InstanceImplementedAsExtension { get { return _InstanceImplementedAsExtension.GetValueOrDefault(); } set { _InstanceImplementedAsExtension = value; } } public bool? _InstanceImplementedAsExtension;
        /// <summary>
        /// Sets a custom string to be inserted before arguments are written when invoking a function
        /// This is useful for method optimizations
        /// x.Call(a,b,c) -> x.Call("MyCustomCode",a,b,c);
        /// </summary>
        public string ArgumentsPrefix { get; set; }
        /// <summary>
        /// Sets a custom string to be appended after arguments are written when invoking a function
        /// This is useful for method optimizations
        /// x.Call(a,b,c) -> x.Call(a,b,c,"MyCustomCode");
        /// </summary>
        public string ArgumentsSuffix { get; set; }
        /// <summary>
        /// Inserts a custom argument when invoking the function
        /// </summary>
        public object InsertArg0 { get; set; }
        /// <summary>
        /// Inserts a custom argument when invoking the function
        /// </summary>
        public object InsertArg1 { get; set; }
        /// <summary>
        /// Inserts a custom argument when invoking the function
        /// </summary>
        public object InsertArg2 { get; set; }
        /// <summary>
        /// Causes the function to be invoked without any commas between arguments:
        /// x.Call(a,b,c) -> x.Call(abc);
        /// </summary>
        public bool OmitCommas { get { return _OmitCommas.GetValueOrDefault(); } set { _OmitCommas = value; } } public bool? _OmitCommas;
        /// <summary>
        /// Sends initializers to constructor as a parameter:
        /// Collection initializers as Json arrays
        /// Object initializers as a Json object
        /// Important note: a new json / array object will ALWAYS be passed to the constructor, even if no object initializers are passed.
        /// </summary>
        public bool JsonInitializers { get { return _JsonInitializers.GetValueOrDefault(); } set { _JsonInitializers = value; } } public bool? _JsonInitializers;
        /// <summary>
        /// Targets the attribute for a specific SharpKit version, this attribute will be ignored if the current 
        /// SharpKit version isn't in the range of the value specified
        /// </summary>
        public string SharpKitVersion { get; set; }

        public string InlineCodeExpression { get; set; }

        public bool ForceMethodSuffix { get { return _ForceMethodSuffix.GetValueOrDefault(); } set { _ForceMethodSuffix = value; } } public bool? _ForceMethodSuffix;
    }
    #endregion

    #region JsPropertyAttribute
    ///<summary>
    /// Specifies custom instructions for SharpKit for a property, this information is used when exporting the member, and when using it.
    ///</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Assembly, AllowMultiple = true)]
    public partial class JsPropertyAttribute : Attribute
    {
        ///<summary>
        ///When set, all references will not use getters and setter, but will treat it as a field instead
        ///</summary>
        ///<remarks>Default value is false</remarks>
        public bool NativeField { get { return _NativeField.GetValueOrDefault(); } set { _NativeField = value; } } public bool? _NativeField;
        ///<summary>
        ///When applied to an indexer property, all references will not use getters and setter, but will treat it as a native indexer instead
        ///</summary>
        ///<remarks>Default value is false</remarks>
        public bool NativeIndexer { get { return _NativeIndexer.GetValueOrDefault(); } set { _NativeIndexer = value; } } public bool? _NativeIndexer;
        /// <summary>
        ///	Indicates that SharpKit compiler will generate javascript code for this type / member
        /// This property is inherited and applied to all derived types. Default value is true
        /// </summary>
        public bool Export { get { return _Export.GetValueOrDefault(true); } set { _Export = value; } } public bool? _Export;
        public string Name { get; set; }
        /// <summary>
        /// Applies the attribute externally on a property
        /// </summary>
        public string TargetProperty { get; set; }
        ///// <summary>
        ///// Applies the attribute externally on a type
        ///// </summary>
        //public Type TargetType { get; set; }
        /// <summary>
        /// Applies the attribute externally on a type
        /// </summary>
        public string TargetTypeName { get; set; }
        /// <summary>
        /// Sets a static property as a global property - omits the class name when declaring and invoking the property
        /// </summary>
        public bool Global { get { return _Global.GetValueOrDefault(); } set { _Global = value; } } public bool? _Global;

        /// <summary>
        /// When true, Object.defineProperty is used for properties. The browser needs to support native property.
        /// </summary>
        public bool NativeProperty { get { return _NativeProperty.GetValueOrDefault(); } set { _NativeProperty = value; } } public bool? _NativeProperty;

        /// <summary>
        /// The NativePropertyEnumerable property attribute defines whether the property shows up in a for...in loop and Object.keys() or not.
        /// </summary>
        public bool NativePropertyEnumerable { get { return _NativePropertyEnumerable.GetValueOrDefault(); } set { _NativePropertyEnumerable = value; } } public bool? _NativePropertyEnumerable;
    }
    #endregion

    #region JsEventAttribute
    ///<summary>
    /// Specifies custom instructions for SharpKit for an event, this information is used when exporting the member, and when using it.
    ///</summary>
    [AttributeUsage(AttributeTargets.Event)]
    public partial class JsEventAttribute : Attribute
    {
        ///<summary>
        ///Causes an event to be treated as a native JavaScript function
        ///</summary>
        public bool NativeDelegates { get { return _NativeDelegates.GetValueOrDefault(); } set { _NativeDelegates = value; } } public bool? _NativeDelegates;


        public bool Export { get { return _Export.GetValueOrDefault(true); } set { _Export = value; } }
        public bool? _Export;

    }
    #endregion

    #region JsDelegateAttribute
    ///<summary>
    /// Specifies custom instructions for SharpKit for a delegate, this information is used when exporting the member, and when using it.
    ///</summary>
    [AttributeUsage(AttributeTargets.Delegate)]
    public partial class JsDelegateAttribute : Attribute
    {
        ///<summary>
        ///Causes a delegate to be treated as a native JavaScript function
        ///</summary>
        public bool NativeDelegates { get { return _NativeDelegates.GetValueOrDefault(); } set { _NativeDelegates = value; } } public bool? _NativeDelegates;
        ///<summary>
        ///Causes a delegate to be treated as a native JavaScript function without instance context support, 
        ///this means that any instance context using the 'this' keyword will be lost
        ///</summary>
        public bool NativeFunction { get { return _NativeFunction.GetValueOrDefault(); } set { _NativeFunction = value; } } public bool? _NativeFunction;
    }
    #endregion

    #region JsEnumAttribute
    ///<summary>
    ///Controls the interoperability and conversion of a .NET enum type into JavaScript.
    ///</summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Assembly)]
    public partial class JsEnumAttribute : Attribute
    {
        /// <summary>
        /// When type is exported, the value of every enum member will be its string name representation, and not its numeric value
        /// </summary>
        public bool ValuesAsNames { get { return _ValuesAsNames.GetValueOrDefault(); } set { _ValuesAsNames = value; } } public bool? _ValuesAsNames;
        ///// <summary>
        ///// When used as assembly attribute, indicates the type for which to apply this attribute on.
        ///// This feature should be used when trying to describe classes on external assemblies that has no SharpKit support
        ///// </summary>
        //public Type TargetType { get; set; }
        /// <summary>
        /// When used as assembly attribute, indicates the type for which to apply this attribute on.
        /// This feature should be used when trying to describe classes on external assemblies that has no SharpKit support
        /// </summary>
        public string TargetTypeName { get; set; }
    }

    #endregion

    #region JsFieldAttribute
    ///<summary>
    /// Specifies custom instructions for SharpKit for a property, this information is used when exporting the member, and when using it.
    ///</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public partial class JsFieldAttribute : Attribute
    {
        /// <summary>
        ///	Indicates that SharpKit compiler will generate javascript code for this field
        /// This property is inherited and applied to all derived types. Default value is true
        /// </summary>
        public bool Export { get { return _Export.GetValueOrDefault(true); } set { _Export = value; } } public bool? _Export;
        /// <summary>
        /// Instructs SharpKit to use a custom name for this field, SharpKit will use this name when exporting the field, and when using it in code.
        /// </summary>
        public string Name { get; set; }
    }
    #endregion

    #region JsNamespaceAttribute

    /// <summary>
    /// Allows mapping and replacement between C# and JavaScript namespace. Useful for creating shorter namespaces in JavaScript
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    class JsNamespaceAttribute : Attribute
    {
        /// <summary>
        /// The C# namespace to be mapped (cannot be null or empty)
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// The JavaScript namespace that should be replaced (can be empty, not recommended)
        /// </summary>
        public string JsNamespace { get; set; }
    }

    #endregion

    #region JsStructAttribute
    //[AttributeUsage(AttributeTargets.Struct)]
    //public class JsStructAttribute : Attribute
    //{
    //    public bool IsClass { get; set; }
    //}
    #endregion

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public partial class JsEmbeddedResourceAttribute : Attribute
    {
        public JsEmbeddedResourceAttribute()
        {
        }
        public JsEmbeddedResourceAttribute(string filename)
        {
            Filename = filename;
        }
        public JsEmbeddedResourceAttribute(string filename, string resourceName)
        {
            ResourceName = resourceName;
        }
        public string Filename { get; set; }
        public string ResourceName { get; set; }
    }

}
