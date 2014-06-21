using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Mirrored.SharpKit.JavaScript
{
    partial class JsTypeAttribute : Attribute, ISupportSharpKitVersion
    {
        public IType TargetType { get; set; }
    }
    partial class JsMethodAttribute : Attribute, ISupportSharpKitVersion
    {
        public IType TargetType { get; set; }
    }
    partial class JsPropertyAttribute : Attribute
    {
        public IType TargetType { get; set; }
    }
    partial class JsEnumAttribute : Attribute
    {
        public IType TargetType { get; set; }
    }

    partial class JsEmbeddedResourceAttribute : ISupportSourceAttribute
    {
        public IAttribute SourceAttribute { get; set; }
    }

    interface ISupportSourceAttribute
    {
        IAttribute SourceAttribute { get; set; }

    }

    interface ISupportSharpKitVersion
    {
        string SharpKitVersion { get; set; }

    }

}
