using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using System.Linq;
using System;

using Chiyoda.CAD.Model;

public class XMLData {

  private EntityType.Type entityType;
  private List<XML_XMPData> xmlDataList = new List<XML_XMPData>();

  public void Add(EntityType.Type type, string xmp, string path)
  {
    var result = xmlDataList.Find((x) => x.XmpType == xmp);
    if (result == null) {
      var data = new XML_XMPData();
      data.XmpType = xmp;
      data.Number = 1;
      data.FileList.Add(path);
      xmlDataList.Add(data);
    } else {
      result.Number += 1;
      if (result.FileList.Count < 2) {
        if (!result.FileList.Contains(path))
        {
          result.FileList.Add(path);
        }
      }
    }
  }

  public void Sort()
  {
    xmlDataList.Sort(new XML_XMPDataCompare());
  }

  public int Number()
  {
    return xmlDataList.Sum((arg1) => arg1.Number);
  }

  public EntityType.Type EntityType
  {
    get
    {
      return entityType;
    }

    set
    {
      entityType = value;
    }
  }

  public List<XML_XMPData> XmlDataList
  {
    get
    {
      return xmlDataList;
    }

    set
    {
      xmlDataList = value;
    }
  }
}

public class XMLDataCompare : System.Collections.Generic.IComparer<XMLData>
{
  public int Compare(XMLData a, XMLData b)
  {
    if (a.Number() > b.Number()) return -1;
    if (a.Number() < b.Number()) return 1;
      return 0;
  }

}