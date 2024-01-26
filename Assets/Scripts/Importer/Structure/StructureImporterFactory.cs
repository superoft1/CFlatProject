using Chiyoda.CAD.BP;
using System;

public class StructureImporterFactory
{
  public static StructureImporter CreateImporter(StructureType.Type type)
  {
    switch (type)
    {
      case StructureType.Type.piperack: return new CSVPipeRackImporter();
    }
    throw new InvalidOperationException();
  }
}