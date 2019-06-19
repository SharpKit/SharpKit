<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WritingNativeJavaScript.ascx.cs" Inherits="SharpKit.Website.Templates.Help.WritingNativeJavaScript" %>
<div>
    <h2>
        Writing native JavaScript</h2>
    <p>
        SharpKit allows you to write native JavaScript code, just as if you've written it yourself. Although C# and JavaScript share many similarities, there are differences as well. This section will explain you how to bridge the gap between the two worlds, and allow you to enjoy the freedom of JavaScript, while retaining the type-checking and validity of C#.</p>
    <div id="PrimitiveTypes">
        <h3>
            Primitive types</h3>
        <p>
            Every JavaScript primitive type has an equivalent C# type. 
            In order to avoid ambiguity with native C# types, a "Js" prefix is added to each JavaScript primitive type.
            For example the <code>String</code> object is named <code>JsString</code> in C#,
            Number is named JsNumber and so on...</p>
            <p>Although a class is named <code>JsString</code> in C#, when it is converted to JavaScript, 
            it will be written as <code>String</code>. 
            This is achieved by using the <code>JsTypeAttribute.Name</code> property:
        </p>
        <div class="TabControl">
        <a>JsString.cs</a>
        <pre class="CS">[JsType(JsMode.Prototype, Name="String", Export=false)]
public class JsString
{
}</pre>
</div>
        <p>
            JsObject</p>
        <p>
            JsArray</p>
        <p>
            JsString</p>
        <p>
            JsNumber</p>
        <p>
            JsDate</p>
    </div>
    <div>
        <h3>
            Keywords</h3>
        <p>
            All of JavaScript keywords are located in a single class called JsContext, you can either call the methods on it by qualifying them with the class name, e.g.: JsContext.@typeof(obj); Or, you can simply inherit from the JsContext class and use the methods directly without any prefix.</p>
        <p>
            The verbatim (@) literal is used in C# to disambiguate between reserved C# keywords and other member / variable names. The 'typeof' method for example is a keyword in C#, so in order to disamibuate it with the JavaScript 'typeof' function, the verbatim (@) literal is needed.</p>
        <p>
            @this</p>
        <p>
            @typeof</p>
        <p>
            @return</p>
        <p>
            arguments</p>
        <p>
            eval</p>
        <p>
            delete</p>
        <p>
            instanceof (not implemented)</p>
    </div>
    <div>
        <h3>
            Casting between Types</h3>
        <p>
            The As&lt;T&gt;() extension method, located in the SharpKit.JavaScript namespace, helps you to cast from one type to another without affecting the generated JavaScript code. for example:</p>
            <div class="TabControl">
            <a>MyPage.cs</a>
        <pre class="CS">
using SharpKit.JavaScript;
using SharpKit.Html4;

[JsType(JsMode.Global, Filename="MyPage.js")]
class MyPage : HtmlContext
{
    public static void Main()
    {
        var input = document.getElementById("input1").As&lt;HtmlInput&gt;();
        input.value = "MyValue";
    }
}
</pre>
<a>MyPage.js</a>
        <pre class="JS">
function Main()
{
    var input = document.getElementById("input1");
    input.value = "MyValue";
}
</pre>
</div>
    </div>
    <div id="TODO" style="display: none">
        <h3>
            JavaScript functions vs. C# delegates</h3>
        <h3>
            Using the HTML DOM</h3>
        <h3>
            Mobile development</h3>
        <h3>
            .NET Classes (Clr mode)</h3>
        <h3>
            Converting JavaScript code to C#</h3>
    </div>
</div>
