using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;

public class XML_XMPData {

  private string xmpType;
  private List<string> fileList = new List<string>();
  private int number;

  public int Number
  {
    get
    {
      return number;
    }

    set
    {
      number = value;
    }
  }

  public string XmpType
  {
    get
    {
      return xmpType;
    }

    set
    {
      xmpType = value;
    }
  }

  public List<string> FileList
  {
    get
    {
      return fileList;
    }

    set
    {
      fileList = value;
    }
  }
}

public class XML_XMPDataCompare : System.Collections.Generic.IComparer<XML_XMPData>
{
  public int Compare(XML_XMPData a, XML_XMPData b)
  {
    if (a.Number > b.Number) return -1;
    if (a.Number < b.Number) return 1;
    return 0;
  }

}