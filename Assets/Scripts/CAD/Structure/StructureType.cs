using System;

namespace Chiyoda.CAD.BP
{
  public class StructureType
  {
    public enum Type
    {
      unknown,
      piperack,
    }

    public static Type Parse(string typeStr)
    {
      var lower = typeStr.ToLower();
      foreach (Type val in Enum.GetValues(typeof(Type)))
      {
        if (val.ToString() == lower) return val;
      }
      return Type.unknown;
    }
  }

}