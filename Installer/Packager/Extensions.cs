using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.Release
{
    class ExecuteResult
    {
        public int ExitCode { get; set; }
        public List<string> Output { get; set; }
        public List<string> Error { get; set; }
    }
}
/*
 * 	<table name="File">
		<col key="yes" def="s72">File</col>
		<col def="s72">Component_</col>
		<col def="s255">FileName</col>
		<col def="i4">FileSize</col>
		<col def="S72">Version</col>
		<col def="S20">Language</col>
		<col def="I2">Attributes</col>
		<col def="i2">Sequence</col>
		<col def="S255">ISBuildSourcePath</col>
		<col def="I4">ISAttributes</col> - 17 = Always overwrite
		<col def="S72">ISComponentSubFolder_</col>
 * 
 * 
 * 
	<table name="Property">
		<col key="yes" def="s72">Property</col>
		<col def="L0">Value</col>
		<col def="S255">ISComments</col>
 * 
 * 		<row><td>ProductCode</td><td>{BB7287C9-58D0-4DEB-AD0E-065EF0AE49E0}</td><td/></row>
		<row><td>ProductName</td><td>SharpKit v4</td><td/></row>
		<row><td>ProductVersion</td><td>4.03.1000</td><td/></row>


*/

//private static void BuildSetupProject()
//{
//    Console.WriteLine("Building setup");
//    BuildProject(@"C:\Projects\SharpKit\trunk\SharpKitSetup.sln", "SingleImage", "SharpKitSetup2", "rebuild");
//    Console.WriteLine("Finished Building setup");
//}
//static XElement GetSetupVersionElement(XDocument doc)
//{
//    var props = doc.Descendants("table").Where(t => t.Attribute("name").Value == "Property").FirstOrDefault();
//    var rowProductVersion = props.Elements("row").Select(t => t.Elements().First()).Where(t => t.Value == "ProductVersion").First().ElementsAfterSelf().First();
//    return rowProductVersion;
//}

//static XElement GetSetupProductCodeElement(XDocument doc)
//{
//    var props = doc.Descendants("table").Where(t => t.Attribute("name").Value == "Property").FirstOrDefault();
//    var rowProductCode = props.Elements("row").Select(t => t.Elements().First()).Where(t => t.Value == "ProductCode").First().ElementsAfterSelf().First();
//    return rowProductCode;
//}

//private static void IncrementVersion(string setupProjectFile, string newVersion)
//{
//    var doc = XDocument.Load(setupProjectFile);
//    var rowProductVersion = GetSetupVersionElement(doc);
//    var rowProductCode = GetSetupProductCodeElement(doc);

//    var version = rowProductVersion.Value;
//    var productCode = rowProductCode.Value;

//    if (newVersion.IsNotNullOrEmpty() && newVersion != version)
//    {
//        UpdateAssemblyFileVersions(newVersion);
//        productCode = Guid.NewGuid().ToString("B");
//        rowProductVersion.Value = newVersion;// VersionToString(newVersion);
//        rowProductCode.Value = productCode;
//        doc.Save(setupProjectFile);
//        Console.WriteLine("Saved setup project, new version {0}, new product code: {1}", newVersion, productCode);
//    }
//}
//private static void ReleaseSharpKitSetup()
//{
//    if (SetupVersion == null)
//    {
//        Console.WriteLine("Detecting version");
//        var s = File.ReadAllText(@"C:\Projects\SharpKit\trunk\src\SharpKitSetup2\SharpKitSetup2.isl");
//        SetupVersion = s.SubstringBetween("<row><td>ProductVersion</td><td>", "</td>");
//    }
//    var filename = "SharpKitSetup_" + SetupVersion.Replace(".", "_") + ".exe";
//    var filepath = @"C:\Projects\SharpKit\trunk\setup\" + filename;
//    //Console.WriteLine("{0} {1}kb", filename, new FileInfo(filepath).Length / 1024);
//    Console.WriteLine("Copying file to " + filepath);
//    File.Copy(@"C:\Projects\SharpKit\trunk\src\SharpKitSetup2\SharpKitSetup2\Express\SingleImage\DiskImages\DISK1\SharpKitSetup.exe", filepath, true);

//}

