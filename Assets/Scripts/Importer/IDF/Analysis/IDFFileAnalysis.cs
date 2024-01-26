using Chiyoda.CAD.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IDF;
using UnityEngine;

public static class EnumHelper
{
  /// <summary>
  /// Extension method to return an enum value of type T for the given string.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="value"></param>
  /// <returns></returns>
  public static T ToEnum<T>(this string value)
  {
      return (T) Enum.Parse(typeof(T), value, true);
  }
}


public class IDFFileAnalysis : MonoBehaviour {

  class SupportedContainer
  {
    public Dictionary<EntityType.Type, int> data = new Dictionary<EntityType.Type, int>();
  }

  class UnSupportedContainer
  {
    public Dictionary<IDF.IDFRecordType.FittingType, int> data = new Dictionary<IDF.IDFRecordType.FittingType, int>();
  }

  // 90: 0001
  // 91: 0010
  // 92: 0100
  // 93: 1000
  Dictionary<string, int> checkInstrument_90_91_92_93 = new Dictionary<string, int>();// bit演算で保存

  class CheckCol11_12
  {
    public void Add(EntityType.Type type, string col11_12)
    {
      if (supportedTypeCol11_12.ContainsKey(type))
      {
        if(supportedTypeCol11_12[type].ContainsKey(col11_12))
        {
          supportedTypeCol11_12[type][col11_12] += 1;
        }
        else
        {
          supportedTypeCol11_12[type].Add(col11_12, 1);
        }
      }
      else
      {
          supportedTypeCol11_12.Add(type, new Dictionary<string, int>{{col11_12, 1}});
      }
    }

    public void Add(IDF.IDFRecordType.FittingType type, string col11_12)
    {
      if (unsupportedTypeCol11_12.ContainsKey(type))
      {
        if(unsupportedTypeCol11_12[type].ContainsKey(col11_12))
        {
          unsupportedTypeCol11_12[type][col11_12] += 1;
        }
        else
        {
          unsupportedTypeCol11_12[type].Add(col11_12, 1);
        }
      }
      else
      {
          unsupportedTypeCol11_12.Add(type, new Dictionary<string, int>{{col11_12, 1}});
      }
    }

    public Dictionary<EntityType.Type, Dictionary<string, int>> supportedTypeCol11_12 = new Dictionary<EntityType.Type, Dictionary<string, int>>();
    public Dictionary<IDF.IDFRecordType.FittingType, Dictionary<string, int>> unsupportedTypeCol11_12 = new Dictionary<IDF.IDFRecordType.FittingType, Dictionary<string, int>>();
  }

  class AllTypeContainer
  {
    public string fileName;
    public SupportedContainer SupportedContainer = new SupportedContainer();
    public UnSupportedContainer UnSupportedContainer = new UnSupportedContainer();

    public override string ToString()
    {
      string s = fileName;
      foreach (var value in Enum.GetValues(typeof(EntityType.Type)))
      {
        var type = value.ToString().ToEnum<EntityType.Type>();
        s += ",";
        if (SupportedContainer.data.ContainsKey(type))
        {
          s += SupportedContainer.data[type];
        }
      }
      s += ",";

      foreach (var value in Enum.GetValues(typeof(IDF.IDFRecordType.FittingType)))
      {
        var type = value.ToString().ToEnum<IDF.IDFRecordType.FittingType>();
        s += ",";
        if (UnSupportedContainer.data.ContainsKey(type))
        {
          s += UnSupportedContainer.data[type];
        }
      }
      return s;
    }
  }

  List<AllTypeContainer> allType = new List<AllTypeContainer>();
  CheckCol11_12 checkCol11_12 = new CheckCol11_12();

  private bool XMLFileExists { get; set; }


  public void ImportData(string parentFolder, string path, bool xmlFileExists)
  {
    XMLFileExists = xmlFileExists;
    using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
    {
      AllTypeContainer container = new AllTypeContainer();
      Uri u1 = new Uri(parentFolder);
      Uri u2 = new Uri(path);

      //絶対Uriから相対Uriを取得する
      Uri relativeUri = u1.MakeRelativeUri(u2);
      //文字列に変換する
      string relativePath = relativeUri.ToString();
      container.fileName = relativePath;
      bool standardExist = false;
      string l;
      while ((l = sr.ReadLine()) != null)
      {
        var columns = IDFStringUtility.Split(l);
        if (columns.Length == 0)
        {
          continue;
        }
        var fittingType = IDFRecordType.GetFittingType(columns[0]);
        if(fittingType == IDF.IDFRecordType.FittingType.Unknown) { continue; }
        if(fittingType == IDFRecordType.FittingType.Standard)
        {
          standardExist = true;
          continue;
        }

        if (!standardExist)
        {
          continue;
        }

        if (fittingType == IDFRecordType.FittingType.Instrument)
        {
          if (columns[0] == "90")
          {
            if (!checkInstrument_90_91_92_93.ContainsKey(relativePath))
            {
              checkInstrument_90_91_92_93.Add(relativePath, Convert.ToInt32("0001", 2));
            }
            else
            {
              checkInstrument_90_91_92_93[relativePath] |= Convert.ToInt32("0001", 2);
            }
          }
          if (columns[0] == "91")
          {
            if (!checkInstrument_90_91_92_93.ContainsKey(relativePath))
            {
              checkInstrument_90_91_92_93.Add(relativePath, Convert.ToInt32("0010", 2));
            }
            else
            {
              checkInstrument_90_91_92_93[relativePath] |= Convert.ToInt32("0010", 2);
            }
          }

          if (columns[0] == "92")
          {
            if (!checkInstrument_90_91_92_93.ContainsKey(relativePath))
            {
              checkInstrument_90_91_92_93.Add(relativePath, Convert.ToInt32("0100", 2));
            }
            else
            {
              checkInstrument_90_91_92_93[relativePath] |= Convert.ToInt32("0100", 2);
            }
          }
          if (columns[0] == "93")
          {
            if (!checkInstrument_90_91_92_93.ContainsKey(relativePath))
            {
              checkInstrument_90_91_92_93.Add(relativePath, Convert.ToInt32("1000", 2));
            }
            else
            {
              checkInstrument_90_91_92_93[relativePath] |= Convert.ToInt32("1000", 2);
            }
          }
        }

        var type = IDF.IDFEntityType.GetType(fittingType);
        if(type == EntityType.Type.NoEntity)
        {
          if(!container.UnSupportedContainer.data.ContainsKey(fittingType))
          {
            container.UnSupportedContainer.data.Add(fittingType, 1);
          }
          else
          {
            container.UnSupportedContainer.data[fittingType] += 1;
          }

          string s = "";
          if (columns.Length > 11)
          {
            s += string.Format("[ {0} ]  ", columns[11]);
          }

          if (columns.Length > 12)
          {
            s += string.Format("[ {0} ]  ", columns[12]);
          }
          checkCol11_12.Add(fittingType, s);
        }
        else
        {
          if(!container.SupportedContainer.data.ContainsKey(type))
          {
            container.SupportedContainer.data.Add(type, 1);
          }
          else
          {
            container.SupportedContainer.data[type] += 1;
          }
          string s = "";
          if (columns.Length > 11)
          {
            s += string.Format("[ {0} ]  ", columns[11]);
          }

          if (columns.Length > 12)
          {
            s += string.Format("[ {0} ]  ", columns[12]);
          }
          checkCol11_12.Add(type, s);
        }
      }
      allType.Add(container);
    }
  }

  private void Sumup(SupportedContainer supported, UnSupportedContainer unSupported)
  {
    foreach (var container in allType)
    {
      foreach (var value in container.SupportedContainer.data)
      {
        if(!supported.data.ContainsKey(value.Key))
        {
          supported.data.Add(value.Key, value.Value);
        }
        else
        {
          supported.data[value.Key] += value.Value;
        }
      }
      foreach (var value in container.UnSupportedContainer.data)
      {
        if(!unSupported.data.ContainsKey(value.Key))
        {
          unSupported.data.Add(value.Key, value.Value);
        }
        else
        {
          unSupported.data[value.Key] += value.Value;
        }
      }
    }
  }


  public void Output()
  {
    OutputFileAnalyze();
    OutputInstrument();
    //OutputCol11_12();
  }

  private void OutputFileAnalyze()
  {
    var path = Application.dataPath + "/Outputs/IDFFileAnalysis.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    using (var writer = new StreamWriter((path), true))
    {
      var str = new System.Text.StringBuilder();
      str.Append("File,");
      foreach (var value in Enum.GetValues(typeof(EntityType.Type)))
      {
        str.Append(value + ",");
      }

      str.Append("*********,");
      foreach (var value in Enum.GetValues(typeof(IDF.IDFRecordType.FittingType)))
      {
        str.Append(value + ",");
      }

      str.Append("xml,");

      str.Append("\n,");
      SupportedContainer suppoted = new SupportedContainer();
      UnSupportedContainer unsuppoted = new UnSupportedContainer();
      Sumup(suppoted, unsuppoted);
      foreach (var value in Enum.GetValues(typeof(EntityType.Type)))
      {
        var type = value.ToString().ToEnum<EntityType.Type>();
        if (suppoted.data.ContainsKey(type))
        {
          str.Append(suppoted.data[type] + ",");
        }
        else
        {
          str.Append("0" + ",");
        }
      }

      str.Append(",");

      foreach (var value in Enum.GetValues(typeof(IDF.IDFRecordType.FittingType)))
      {
        var type = value.ToString().ToEnum<IDF.IDFRecordType.FittingType>();
        if (unsuppoted.data.ContainsKey(type))
        {
          str.Append(unsuppoted.data[type] + ",");
        }
        else
        {
          str.Append(0 + ",");
        }
      }

      str.Append("\n");


      foreach (var allTypeContainer in allType)
      {
        str.Append(allTypeContainer.ToString() + "," + XMLFileExists + "\n");
      }
      writer.Write(str);
    }

  }

  private void OutputInstrument()
  {
    var path = Application.dataPath + "/Outputs/IDFCheckInstrument.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }

    using (var writer = new StreamWriter((path), true, new UTF8Encoding(true)))
    {
      string str = "";
      foreach (var value in checkInstrument_90_91_92_93)
      {
        str += value.Key + "," + Convert.ToString(value.Value, 2) + "\n";
      }
      writer.Write(str);
    }
  }

  private void OutputCol11_12()
  {
    var path = Application.dataPath + "/Outputs/IDFCheckCol11_12.csv";
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }
    using (var writer = new StreamWriter((path), true))
    {
      string s="";
      foreach (var value in checkCol11_12.supportedTypeCol11_12)
      {
        s += value.Key + ",";
        foreach (var value2 in value.Value)
        {
          s += value2.Key + "," + value2.Value + ",";
        }

        s += "\n";
      }

      s += "*******\n";
      foreach (var value in checkCol11_12.unsupportedTypeCol11_12)
      {
        s += value.Key + ",";
        foreach (var value2 in value.Value)
        {
          s += value2.Key + "," + value2.Value + ",";
        }

        s += "\n";
      }
      writer.Write(s);
    }
  }

}
