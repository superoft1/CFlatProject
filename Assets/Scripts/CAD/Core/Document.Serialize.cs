using System ;
using System.Text ;
using System.Xml ;

namespace Chiyoda.CAD.Core
{
  partial class Document
  {
    [field: NonSerialized]
    public string SavePath { get ; private set ; }

    public void Save()
    {
      Save( SavePath ) ;
    }

    public void Save( string path )
    {
      var settings = new XmlWriterSettings
      {
        Encoding = Encoding.UTF8,
        Indent = true,
        IndentChars = "  ",
        NewLineChars = "\r\n",
        NewLineHandling = NewLineHandling.Replace,
        OmitXmlDeclaration = false,
        NewLineOnAttributes = false,
      } ;

      using ( var writer = XmlWriter.Create( path, settings ) ) {
        var serializer = new IO.SerializationContext( writer, this ) ;
        if ( serializer.WriteDocument() ) {
          SavePath = path ;
        }
      }
    }

    public void Load( string path )
    {
      using ( var reader = XmlReader.Create( path ) ) {
        var deserializer = new IO.DeserializationContext( reader, this ) ;
        deserializer.ReadDocument() ;
      }
    }
  }
}