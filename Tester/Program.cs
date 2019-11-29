﻿using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using zenonApi.Logic;

namespace Tester
{
  class Program
  {
    static void Main(string[] args)
    {
      XDocument demoProject = XDocument.Load($@"C:\Users\{Environment.UserName}\Desktop\DemoProject.xml");
    
      // Import the project from the XML
      LogicProject project = LogicProject.Import(demoProject.Element("K5project"));

      var varGrp = project.GlobalVariables.VariableGroups.FirstOrDefault();
      for (int i = 0; i < 50; i++)
      {
        var asdf = new LogicVariable()
        {
          InitialValue = "5",
          MaxStringLength = "255",
          Type = "STRING",
          Name = "MyVariable" + i,
        };
        asdf.VariableInfos.Add(new LogicVariableInfo()
        {
          Data = "<syb>",
          Type = LogicVariableInformationTypeKind.Embed
        });

        asdf.VariableInfos.Add(new LogicVariableInfo()
        {
          Data = "STRATON",
          Type = LogicVariableInformationTypeKind.Profile
        });

        varGrp.Variables.Add(asdf);
      }

      LogicFolder folder = project.ApplicationTree
        .Folders.FirstOrDefault();

      folder.Name = "RenamedTestFolder";

      LogicProgram program = folder.Programs.FirstOrDefault();
      program.Name = "RenamedMyProgram";
      program.SourceCode += "\n// Second Comment";

      // Navigate to the application tree
      var folderAgain = program.Parent;

      // Change the cycle timing
      project.Settings.TriggerTime.CycleTime = 12345;
      project.Settings.CompilerSettings.CompilerOptions["warniserr"] = "OFF";

      // Modify variables
      var variable = program.VariableGroups.FirstOrDefault()?.Variables.FirstOrDefault();
      variable.Name = "RenamedVariable";
      variable.Attributes.In = true;
      variable.Attributes.Out = true;
      variable.VariableInfos.Add(new LogicVariableInfo() { Type = LogicVariableInformationTypeKind.Embed, Data = "<syb>" });

      // Remove a folder
      project.ApplicationTree.Folders.Where(x => x.Name == "Signals").FirstOrDefault()?.Remove();

      // Export and save the project again
      XElement modifiedProject = project.ExportAsXElement();
      XDocument document = new XDocument
      {
        Declaration = new XDeclaration("1.0", "utf-8", "yes")
      };

      document.Add(modifiedProject);
      using (XmlTextWriter writer = new XmlTextWriter($@"C:\Users\{Environment.UserName}\Desktop\DemoProjectModified.xml",
        Encoding.GetEncoding("utf-8")))
      {
        writer.Indentation = 3;
        writer.Formatting = Formatting.Indented;
        document.Save(writer);
      }
    }
  }
}
