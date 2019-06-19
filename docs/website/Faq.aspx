<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Faq.aspx.cs" Inherits="SharpKit.Website.Faq" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="Header3">
        <div class="Panel">
            <h1>
                Frequently Asked Questions</h1>
        </div>
    </div>
    <div class="Panel">
        <h3>
            What is SharpKit?
        </h3>
        <p>
            SharpKit is a Web Toolkit that enables you to write C# and convert it to JavaScript during compilation.
        </p>
        <h3>
            Who is SharpKit for?
        </h3>
        <p>
            SharpKit was designed for web development teams that maintain C# and JavaScript code, most commonly within the ASP.NET platform and Visual Studio.
        </p>
        <h3>
            Why should I use SharpKit?
        </h3>
        <p>
            Writing and maintaining JavaScript code can be very expensive.
            <br />
            Migrating from JavaScript to C# enables you to:
        </p>
        <ul>
            <li>
                <b>
                    Leverage Visual Studio C# productivity</b><br />
                Harness native C# features in Visual Studio such as compile-time syntax verification, code-completion, XML documentation and refactoring
            </li>
            <li>
                <b>
                    Maximize team work</b><br />
                You can now easily work on large projects in a team, without worrying about breaking your team members code.</li>
            <li>
                <b>
                    Streamline client-side code review</b><br />
                With C#, you can leverage code metrics, code analysis and perfomance profiling functionality for your client-side code
            </li>
        </ul>
        <h3>Is SharpKit free?
        </h3>
        <p>SharpKit is free under GPL, that means that if you're open source you can use it freely, for commercial purposes please refer to our licensing/pricing section.</p>
        <h3>Do I have to install SharpKit on my deployment web servers</h3>
        <p>No, SharpKit converts code during compilation, when you deploy you web app, you deploy it with the generated JavaScript files, 
        in fact, SharpKit also helps you optimize your JavaScript files using minification and consolidation.</p>
        <h3>
            Do I have to keep using SharpKit once I've started?
        </h3>
        <p>
            <b>No.</b><br />
            SharpKit was designed to be a nonintrusive, pure compile-time solution with minimal impact during development and zero impact during production.
        </p>
        <ul>
            <li>SharpKit does not change native JavaScript syntax</li>
            <li>SharpKit does not require any change to your server-side code</li>
            <li>SharpKit does not affect your existing file structure</li>
            <li>SharpKit does not require custom JavaScript includes</li>
        </ul>
        <p>
            This non-lock-in model enables you to stop using SharpKit at any time, simply by excluding the client-side C# files and treating the JavaScript files as the source.
        </p>
        <h3>
            How do I get started?
        </h3>
        <p>
            Simply <a href="Download.aspx">download</a> SharpKit and install it on your development machine.<br />
            Then visit our <a href="Wiki/">learning center</a> to help you get started quickly.
        </p>
        <h3>
            What are the system requirement for using SharpKit?</h3>
        <ul>
            <li>
                Windows XP / Vista/ 7 / 2003 Server / 2008 Server (32bit or 64bit)
            </li>
            <li>
                Microsoft .NET framework 2.0 / 3.5 / 4.0</li>
            <li>
                1GB RAM</li>
        </ul>
        <h3>
            Do I need to have Visual Studio in order to use to use SharpKit?
        </h3>
        <p>
            No. SharpKit compiler is an executable that can be used with or without Visual Studio.
        </p>
        <h3>
            Do I have to include any custom JavaScript files in my project?
        </h3>
        <p>
            It is up to you. SharpKit provides standard support for many C# language features without including any additional files. Advanced support for features such as reflection, LINQ and generics require a small include of the SharpKit JS Kernel (about 60K compressed).
        </p>
        <h3>
            Do I need to reference SharpKit assemblies from my code?
        </h3>
        <p>You can, but you don't have to. </p><p>SharpKit assemblies contain many interfaces for JavaScript, HTML, jQuery and other web libraries. 
        In order to use them you can either add them as a reference, or integrate the .cs header file directly into your project.
        All of SharpKit header files and assemblies are completely open-source, and you can modify them anytime.
        </p>
        <h3>
            Will my application's JavaScript performance be impacted if I use SharpKit?
        </h3>
        <p>
            No. Since SharpKit generates native JavaScript that is identical to hand-written code, there is no performance penalty for the automation process.
        </p>
        <h3>
            Is there any JavaScript code that SharpKit cannot compile from C#?
        </h3>
        <p>
            No. SharpKit is fully compliant with all language features of JavaScript.
        </p>
        <h3>
            Are there any C# features that SharpKit does not compile into JavaScript?
        </h3>
        <p>
            Yes. SharpKit has a limited support for yield, and currently doesn't support explicit interface implementation and custom (non-native) structs.
        </p>
        <h3>
            What happens to my project if I stop using SharpKit?
        </h3>
        <p>
            Nothing. You can simply exclude the C# files used for client-side code generation and treat the generated JavaScript files as the source files.
        </p>
        <h3>
            Can SharpKit dynamically compile C# to JavaScript during runtime?
        </h3>
        <p>
            Yes. You can feed the SharpKit compiler with C# code during runtime, and generate JavaScript on the fly.
        </p>
        <h3>
            Can I use SharpKit to develop iPhone and smartphone applications in C#?
        </h3>
        <p>
            Yes. Most modern mobile phones support HTML5 so you can use SharpKit's HTML5 interface to develop mobile browser applications in C#, and compile them into JavaScript.
        </p>
   </div>
</asp:Content>
