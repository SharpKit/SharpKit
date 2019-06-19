<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Register TagPrefix="uc" Src="~/Templates/Help/IntegratingSharpKit.ascx" TagName="IntegratingSharpKit" %>
<%@ Register TagPrefix="uc" Src="~/Templates/Help/UsingSharpKit.ascx" TagName="UsingSharpKit" %>
<%@ Register TagPrefix="uc" Src="~/Templates/Help/UsingWebFrameworks.ascx" TagName="UsingWebFrameworks" %>
<%@ Register TagPrefix="uc" Src="~/Templates/Help/WritingNativeJavaScript.ascx" TagName="WritingNativeJavaScript" %>
<div>
    <h1>
        SharpKit Help Guide</h1>
    <p>
        Welcome to SharpKit help guide, this guide will give you an overview on SharpKit, and explain the different concepts you need to start using it.</p>
    <uc:IntegratingSharpKit runat="server" />
    <uc:UsingSharpKit runat="server" />
    <uc:UsingWebFrameworks runat="server" />
    <uc:WritingNativeJavaScript runat="server" />
</div>
