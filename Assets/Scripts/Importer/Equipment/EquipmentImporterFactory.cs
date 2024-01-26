using System;
using Chiyoda.CAD.BP;

public class EquipmentImporterFactory
{
  public static EquipmentImporter CreateImporter( BlockPatternType.Type type)
  {
    //switch ( type )
    //{
    //  case BlockPatternType.Type.EndTopTypePump: return new CSVEndTopTypePumpImporter();
    //}
    throw new InvalidOperationException();
  }
}
