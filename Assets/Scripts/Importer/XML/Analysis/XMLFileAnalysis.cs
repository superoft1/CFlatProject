using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using System.IO;
using Chiyoda.CAD.Model;
using System.Linq;
using Chiyoda.CAD.Core ;

public class XMLFileAnalysis : MonoBehaviour {

  List<XMLData> xmlDataList = new List<XMLData>();
  string currentPath = "";
  List<double> bendRates = new List<double>();
  List<double> concentricPipingReducerRates = new List<double>();
  List<double> eccentricPipingReducerRates = new List<double>();

  public void ImportData(string path)
  {
    //Debug.Log("ImportData : " + path);
    currentPath = path;
    string content = System.IO.File.ReadAllText(path);
    var xml = new System.Xml.XmlDocument();
    xml.LoadXml(content);

    ImportData(xml.DocumentElement);
  }

  void ImportData(System.Xml.XmlElement i_element)
  {
    var element = i_element["PipingNetworkSystem"];

    foreach (var child in element.ChildNodes)
    {
      var childElement = child as System.Xml.XmlElement;
      if (childElement != null)
      {
        if (childElement.Name == "PipingNetworkSegment")
        {
          ImportSegment(childElement);
        }
      }
    }
  }

  void ImportSegment(System.Xml.XmlElement i_element)
  {
    var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

    //currentRotation = Quaternion.identity;
    foreach (var child in i_element.ChildNodes)
    {
      var childElement = child as System.Xml.XmlElement;
      if (childElement != null)
      {
        if (childElement.Name == "PipingComponent" || childElement.Name == "Pipe" || childElement.Name == "ProcessInstrument")
        {
          var componentName = childElement.GetAttribute("ComponentClass");
          var id = childElement.GetAttribute("StockNumber");
          var entityType = XMLEntityType.GetType(componentName);
          if (entityType == EntityType.Type.PipingTee)
          {
            //! TeeとLateralTeeの判別
            if (XMLImporterUtility.IsPipingLateralTee(childElement))
            {
              entityType = EntityType.Type.PipingLateralTee;
            }
          }

          var result = xmlDataList.Find((x) => x.EntityType == entityType);
          if (result == null) {
            var data = new XMLData();
            data.EntityType = entityType;
            data.Add(entityType, id, currentPath);
            xmlDataList.Add(data);
          } else {
            result.Add(entityType, id, currentPath);
          }

          if (entityType == EntityType.Type.PipingElbow90 || entityType == EntityType.Type.PipeBend) {
            var points = XMLEntityImporter.GetConnectionPoints(childElement);
            var diameter = XMLEntityImporter.NominalDiameter(points[1]).OutsideMeter;
            var diff = (XMLEntityImporter.GetPosition(points[0]) - XMLEntityImporter.GetPosition(points[1])).magnitude;
            var rate = diff / diameter;
            bendRates.Add(rate);
          } else if (entityType == EntityType.Type.ConcentricPipingReducerCombination) {
            var points = XMLEntityImporter.GetConnectionPoints(childElement);
            var diameter1 = XMLEntityImporter.NominalDiameter(points[1]).OutsideMeter;
            var diameter2 = XMLEntityImporter.NominalDiameter(points[2]).OutsideMeter;
            concentricPipingReducerRates.Add(diameter1 / diameter2);
          } else if (entityType == EntityType.Type.EccentricPipingReducerCombination) {
            var points = XMLEntityImporter.GetConnectionPoints(childElement);
            var diameter1 = XMLEntityImporter.NominalDiameter(points[1]).OutsideMeter;
            var diameter2 = XMLEntityImporter.NominalDiameter(points[2]).OutsideMeter;
            eccentricPipingReducerRates.Add(diameter1 / diameter2);
          }
        } else if (childElement.Name == "ProcessInstrument") {
          //Debug.Log("childElement : " + childElement.Name);          
          var componentName = childElement.GetAttribute("ComponentClass");
          var id = childElement.GetAttribute("ID");
          Debug.Log("ProcessInstrument : " + componentName + "," + id);
        } else {

        }
      }
    }
  }

  public void Output()
  {
    foreach (var d in xmlDataList) {
      d.Sort();
    }
    xmlDataList.Sort(new XMLDataCompare());

    OutputDetail();
    OutputEntity();

    bendRates.Sort();
    bendRates.Distinct();
    OutputBendElbow90();

    concentricPipingReducerRates.Sort();
    concentricPipingReducerRates.Distinct();
    OutputConcentricPipingReducer();

    eccentricPipingReducerRates.Sort();
    eccentricPipingReducerRates.Distinct();
    OutputEccentricPipingReducer();
  }

  void OutputEccentricPipingReducer()
  {
    var path = Application.dataPath + "/Outputs/EccentricPipingReducer.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    StreamWriter sw;
    FileInfo fi;
    fi = new FileInfo(path);
    sw = fi.AppendText();

    string s = "";
    foreach (var d in eccentricPipingReducerRates)
    {
      s += d;
      s += System.Environment.NewLine;
    }
    sw.Write(s);
    sw.Flush();
    sw.Close();
  }

  void OutputConcentricPipingReducer()
  {
    var path = Application.dataPath + "/Outputs/ConcentricPipingReducer.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    StreamWriter sw;
    FileInfo fi;
    fi = new FileInfo(path);
    sw = fi.AppendText();

    string s = "";
    foreach (var d in concentricPipingReducerRates)
    {
      s += d;
      s += System.Environment.NewLine;
    }
    sw.Write(s);
    sw.Flush();
    sw.Close();
  }

  void OutputBendElbow90()
  {
    var path = Application.dataPath + "/Outputs/Bend.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    StreamWriter sw;
    FileInfo fi;
    fi = new FileInfo(path);
    sw = fi.AppendText();

    string s = "";
    foreach (var d in bendRates)
    {
      s += d;
      s += System.Environment.NewLine;
    }
    sw.Write(s);
    sw.Flush();
    sw.Close();
  }

  void OutputEntity()
  {
    var path = Application.dataPath + "/Outputs/EntityAnalysis.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    StreamWriter sw;
    FileInfo fi;
    fi = new FileInfo(path);
    sw = fi.AppendText();

    string s = "";
    foreach (var d in xmlDataList)
    {
      s += d.EntityType;
      s += ",";
      s += d.Number();
      s += System.Environment.NewLine;
    }
    sw.Write(s);
    sw.Flush();
    sw.Close();
  }

  void OutputDetail()
  {
    var path = Application.dataPath + "/Outputs/FileAnalysis.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    StreamWriter sw;
    FileInfo fi;
    fi = new FileInfo(path);
    sw = fi.AppendText();

    string s = "";
    foreach (var d in xmlDataList)
    {
      s += d.EntityType;
      s += ",";
      s += d.Number();
      s += System.Environment.NewLine;

      foreach (var x in d.XmlDataList)
      {
        s += d.EntityType;
        s += ",";
        s += x.XmpType;
        s += ",";
        s += x.Number;
        if (x.FileList.Count > 0)
        {
          s += ",";
          s += x.FileList[0];
        }
        if (x.FileList.Count > 1)
        {
          s += ",";
          s += x.FileList[1];
        }
        s += System.Environment.NewLine;
      }
    }
    sw.Write(s);
    sw.Flush();
    sw.Close();
  }
}
