namespace Importer.CSV.AutoRouting
{
  internal interface ILineListImportCondition
  {
    bool IsIgnoringLine( string lineID ) ;
  }
}