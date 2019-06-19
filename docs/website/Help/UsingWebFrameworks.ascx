<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UsingWebFrameworks.ascx.cs" Inherits="SharpKit.Website.Templates.Help.UsingWebFrameworks" %>
<div>
    <h2>
        Using Web Libraries with SharpKit</h2>
    <p>
        SharpKit supports any JavaScript based library, in order to use a library with SharpKit, you must obtain a C# 'header' file of the library. A C# header file contains all the classes and methods that a certain library offers. The methods in this file have no implementation, and are not converted into JavaScript, they simply give you the ability use them from C#. You can use these files by adding them to your project, or by referencing compiled assembly version of them.</p>
    <p>
        To make things easy, we have created a googlecode project called
        <a href="http://sharpkit.googlecode.com">
            SharpKit SDK</a>, which contains open-source C# header files for all popular web libraries such as: jQuery, ASP.NET Ajax, ExtJS, jQTouch and more... These libraries are maintained by us and by members of the SharpKit community, if you'd like to contribute,
        <a href="mailto://contact@sharpkit.net">
            let us know</a>. You should know that all of these libraries are also included in the SharpKit installer, and are available on your SharpKit Program Files folder.
    </p>
    <div>
        <h3>
            Using jQuery</h3>
        <p>
            In order to use jQuery, you'll need to add a reference to SharpKit.jQuery assembly, or include the SharpKit.jQuery cs file in your project. There's a header file and an assembly for each version of jQuery. You'll also have to add jQuery js file to pages that use it.</p>
            <div class="TabControl">
        <a>HelloWorld.htm</a>
        <pre class="Html">&lt;html&gt;
    &lt;head&gt;
        &lt;title&gt;Hello World&lt;/title&gt;
        &lt;script src="res/jquery-1.4.4.min.js" type="text/javascript"&gt;&lt;/script&gt;
        &lt;script src="res/HelloWorld.js" type="text/javascript"&gt;&lt;/script&gt;
        &lt;script&gt;            $(HelloWorldClient_Load);&lt;/script&gt;
    &lt;/head&gt;
    &lt;body&gt;
        &lt;button&gt;Click me&lt;/button&gt;
    &lt;/body&gt;
&lt;/html&gt;
</pre>
        <a>HelloWorld.cs</a>
        <pre class="CS">using SharpKit.JavaScript;
using SharpKit.Html4;
using SharpKit.jQuery;

namespace SharpKitWebApp
{
    [JsType(JsMode.Global, Filename = "~/res/HelloWorld.js")]
    public class HelloWorldClient : jQueryContext
    {
        static void HelloWorldClient_Load()
        {
            //J() is instead of $() which is not allowed in C#
            J("button").click(t =&gt; alert("Hello world"));
        }
    }
}
</pre>
        <a>HelloWorld.js</a>
        <pre class="JS">function HelloWorld_Load()
{
	$("button").click(function(t){ return alert("Hello world")});
}
</pre></div>
    </div>
    <div>
        <h3>
            Writing your own Header Files</h3>
        <p>
            There are situations in which you will have to write your own header files, this happens in two common scenarios:</p>
        <ul>
            <li>
                You use a custom library that is not available in SharpKit SDK</li>
            <li>
                You haven't converted all of your current JavaScript code into C#, but you still need to use this code from C#.</li></ul>
        <p>
            In order to create a header file, all you have to do is to create empty implementation of the needed classes and members, decorate them with a JsTypeAttribute in the proper mode, and prevent these classes of being exported.</p>
        <p>
            To prevent C# class with JsTypeAttribute from being exported into JavaScript, use the JsTypeAttribute.Export property.
            </p>
            <div class="TabControl">
            <a>MyExternalLib.cs</a>
        <pre class="CS">[JsType(JsMode.Prototype, Name="MyExternalLibrary", Export=false)]
class MyExternalLibrary
{
    public static void DoSomething(){}
}

[JsType(JsMode.Global, Export=false)]
class MyExternalFunctions
{
    public static void DoSomethingElse(){}
}
</pre>
<a>MyPage.cs</a>
<pre class="CS">[JsType(JsMode.Global, Filename="MyPage.js")]
class MyPage
{
    public static void Main()
    {
        MyExternalLibrary.DoSomething();
        MyExternalFunctions.DoSomethingElse();
    }
}
</pre>
<a>MyPage.js</a>
        <pre class="JS">
function Main()
{
    MyExternalLibrary.DoSomething();
    DoSomethingElse();
}
</pre>
</div>
<p>Please note that the JsTypeAttribute.Mode and Name properties are very important, even if you're not exporting this code. 
These attributes are used by SharpKit compiler to generate the proper code when you refer to these classes. The JsType(Name="MyExternalLibrary") is very important,
since it tells SharpKit to ignore this type's namespace and name, and use the specified name instead.</p>
    </div>
</div>
