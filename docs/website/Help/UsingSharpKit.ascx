<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UsingSharpKit.ascx.cs" Inherits="SharpKit.Website.Templates.Help.UsingSharpKit" %>
<div>
    <h2>Using SharpKit</h2>
    <p>In this chapter you will learn about the different SharpKit C# to JavaScript conversion modes. Each mode has a unique set of rules designed to help you achieve any type of JavaScript code, while remaining in a fully typed, valid C#.</p>
    <div>
        <h3>Writing your first class</h3>
        <p>To convert a class into JavaScript, all you have to do is to decorate your class with a
            <code>JsType</code>
            attribute. There are several modes in which SharpKit can convert C# to JavaScript, by setting the
            <code>JsMode</code>
            parameter on the
            <code>JsType</code>
            attribute. </p>
        <p>Create a new class and add the following code:</p>
        <div class="TabControl">
            <a>Site.cs</a>
            <pre class="CS">[JsType(JsMode.Global, Filename="Site.js")]
class Site : HtmlContext
{
    public static void btnHello_click(HtmlDomEventArgs e)
    {
           document.body.appendChild(document.createTextNode("Hello SharpKit!"));
    }
}</pre>
<a>Site.js</a>
<pre class="JS">function btnHello_click(e)
{
    document.body.appendChild(document.createTextNode("Hello SharpKit!"));
}
</pre>
            <a>Site.htm</a>
            <pre class="Html">&lt;html&gt;
    &lt;head&gt;
        &lt;script src="Site.js"&gt;&lt;/script&gt;
    &lt;/head&gt;
    &lt;body&gt;
        &lt;button onclick="btnHello_click(event);"&gt;Click me&lt;/button&gt;
    &lt;/body&gt;
&lt;/html&gt;
    </pre>
        </div>
        <p>The
            <code>JsType</code>
            attribute will cause SharpKit to convert the class Site into the file Site.js, you can see the js file included in the htm file.</p>
    </div>
    <div>
        <h3>Global Mode</h3>
        <p>A class decorated with a JsType(JsMode.Global) attribute will be converted in the following rules:</p>
        <ul>
            <li>Static methods are converted to global functions (named functions)</li>
            <li>Static fields are converted to global variables</li>
            <li>Static constructor is converted to global code</li>
        </ul>
        <div class="TabControl"><a>Global.cs</a>
        <pre class="CS">[JsType(JsMode.Global, Filename="Global.js")]
class Global
{
    static JsNumber MyNumber = 0;
    static Global()
    {
    	HelloWorld();
    }
    public static void HelloWorld()
    {
        document.body.appendChild(document.createTextNode("Hello SharpKit!"));
    }
}</pre>
<a>Global.js</a>
        <pre class="JS">var MyNumber = 0;
HelloWorld();
function HelloWorld()
{
    document.body.appendChild(document.createTextNode("Hello SharpKit!"));
}</pre></div>
        <p>Although it seems that the C# and JavaScript code is similiar, notice that it's fully typed. Write this code in a visual studio project, and hover with your mouse over the token 'document' or 'createTextNode' and see the documentation. Any attempt to modify the code to an unexisting method will result in compilation error.</p>
    </div>
    <div id="PrototypeMode">
        <h3>Prototype Mode </h3>
        <p>A class decorated with a JsType(JsMode.Prototype) attribute will be converted in the following rules:</p>
        <ul>
            <li>Constructor is converted into a JavaScript constructor function</li>
            <li>Instance methods are converted into prototype functions</li>
            <li>Static methods are converted into functions on the constructor function</li>
        </ul>
        <div class="TabControl">
        <a>Contact.cs</a>
        <pre class="CS">[JsType(JsMode.Prototype, Filename="Contact.js")]
class Contact
{
    public void Call()
    {
    }
    public static Contact Load()
    {
         return null;
    }
}</pre>
<a>Contact.js</a>
        <pre class="JS">Contact = function()
{
}
Contact.prototype.Call = function()
{
}
Contact.Load = function()
{
    return null;
}
</pre></div>
    </div>
    <div id="Json Mode">
        <h3>Json Mode </h3>
        <p>A class decorated with a JsType(JsMode.Json) attribute will not be exported at all, it will give you the ability to use JSON notation on typed classes.</p>
        <div class="TabControl">
        <a>MyOptions.cs</a>
        <pre class="CS">[JsType(JsMode.Json)]
class MyOptions
{
    public JsString Name { get; set; }
    public bool IsCool { get; set; }
}</pre>
<a>MyPage.cs</a>
<pre class="CS">[JsType(JsMode.Global, Filename="MyPage.js")]
class MyPage
{
    public static void Main()
    {
        var options = new MyOptions { Name="MyName", IsCool=true };
    }
}
</pre>
<a>MyPage.js</a>
        <pre class="JS">
function Main()
{
    var options = {Name:"MyName, IsCool:true};
}
</pre>

</div>
        <p>This technique is very useful on constructor Options class, for example, jQuery.ajax(options) function. It also gives you the ability to share web service data contracts between client and server. Simply mark your data contract classes with the JsType(JsMode.Json) and you'll be able to use them in a C# to JavaScript context.</p>
    </div>
</div>
