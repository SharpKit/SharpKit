<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IntegratingSharpKit.ascx.cs" Inherits="SharpKit.Website.Templates.Help.IntegratingSharpKit" %>
<div>
    <h2>Integrating SharpKit</h2>
    <p>This chapter will explain how to start using SharpKit in new or existing projects, SharpKit easily integrates into any project type, whether it's a web application, class library or even console application.</p>
    <div>
        <h3>Creating a new project with SharpKit</h3>
        <p>Open Visual Studio</p>
        <p>Click File -> New Project</p>
        <p>Select Visual C# -> SharpKit -> SharpKit Web Application and click Ok.</p>
        <p>SharpKit enabled ASP.NET web application will be created with an aspx page, and a C# code-behind with a class that is converted to JavaScript during compilation. </p>
    </div>
    <div>
        <h3>Integrating SharpKit into an existing project</h3>
        <p>Add the following line on your .csproj file: (add it right after all 'Import' sections in your project file)</p>
        <div class="TabControl">
            <a>WebApp.csproj</a>
            <pre class="Xml">&lt;!--This line is in any .csproj file: --&gt;
&lt;Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" /&gt;
&lt;!--Add this line after all Import sections: --&gt;
&lt;Import Project="$(MSBuildBinPath)\SharpKit\4\SharpKit.Build.targets" /&gt;</pre>
        </div>
        <p>Add the following references into your project:</p>
        <ul>
            <li>SharpKit.JavaScript.dll (you
                <b>must</b>
                add this reference)</li>
            <li>SharpKit.Html4.dll</li>
        </ul>
        <p>SharpKit supports any C# type project, including web, console (.exe) and class library (.dll) project types.</p>
    </div>
    <div>
        <h3>Upgrading from SharpKit v2 to SharpKit v4</h3>
        <p>Edit your .csproj file, replace the SharpKit v2 import line, with the SharpKit v4 import line:</p>
        <div class="TabControl">
            <a>WebApp.csproj</a>
            <pre class="Xml">&lt;!--Replace this line:--&gt;
&lt;Import Project="$(MSBuildBinPath)\SharpKit\SharpKit.Build.targets" /&gt;
&lt;!--With this line:--&gt;
&lt;Import Project="$(MSBuildBinPath)\SharpKit\4\SharpKit.Build.targets" /&gt;</pre>
        </div>
        <p>Update assembly references, make sure to update SharpKit.JavaScript.dll, never reference both assemblies.
        SharpKit v4 assemblies are located in your Program Files\SharpKit\4\ folder.</p>
    </div>
</div>
