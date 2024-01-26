using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using IDF ;
using NUnit.Framework ;
using UnityEngine ;
using Component = Chiyoda.CAD.Model.Component ;


namespace Tests.EditorMode.Editor.Importer.IDF
{
  public class IdfDeserializerTest
  {
    [Test]
    public void IdfDeserializerSimplePasses()
    {
      var idfFilesFolder = Path.Combine( Application.dataPath, @"Tests\EditorMode\Editor\Importer\IDF\IdfFiles" ) ;
      var expectedFolder = Path.Combine( Application.dataPath, @"Tests\EditorMode\Editor\Importer\IDF\Expected" ) ;
      var dumpFolder = Path.Combine( Application.dataPath, @"Tests\EditorMode\Editor\Importer\IDF\Dump" ) ;
      var fileList = new List<string>() ;
      ImportManager.GetFiles( idfFilesFolder, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" },
        fileList ) ;
      
      var compareFiles = new List<(string dump, string expected)>();
      foreach ( var file in fileList ) {
        // 各ファイル毎にフリーバーテックスの確認を行いために毎回documentを作成する
        // TODO 複数ファイル読み込んで全体でのフリーバーテックスの確認は別途必要
        var doc = DocumentCollection.Instance.Current ;
        if ( doc != null ) {
          DocumentCollection.Instance.Close( doc ) ;
        }
        doc = DocumentCollection.Instance.CreateNew() ;
        var deserializer = new IDFDeserializer() ;
        var bp = doc.CreateEntity<Chiyoda.CAD.Topology.BlockPattern>() ;
        var dumpText = Path.Combine( dumpFolder, Path.GetFileNameWithoutExtension( file ) + ".txt" ) ;
        var freeVertexTotal = 0 ;
        using ( var writer = new StreamWriter( dumpText, false, new UTF8Encoding( true ) ) ) {
          var grpInfo = new Chiyoda.Importer.GroupInfo(doc, bp, file, appendDirectlyToGroup:false) ;
          foreach ( var (leafEdge, lineIndex) in deserializer.ImportData( grpInfo, file ) ) {
            var freeVetexCount = leafEdge.GetFreeVertex().Count() ;
            freeVertexTotal += freeVetexCount ;
            var comp = (Component) leafEdge.PipingPiece ;
            writer.WriteLine(
              $"{comp.ComponentName}, {comp.StockNumber}, {(Vector3)leafEdge.GlobalCod.GlobalizeVector(comp.Axis)}, {(Vector3)leafEdge.GlobalCod.GlobalizePoint( comp.Origin)}, {comp.IsEndOfStream}" ) ;
            foreach ( var p in leafEdge.PipingPiece.ConnectPoints ) {
              writer.WriteLine( $"{(Vector3)p.GlobalPoint}" ) ;
            }
            writer.WriteLine( $"FreeVertex: {freeVetexCount}" ) ;
          }
          writer.WriteLine( $"FreeVertexTotal: {freeVertexTotal}" ) ;
        }
        DocumentCollection.Instance.Close( doc ) ;
        compareFiles.Add( ( dumpText,
          Path.Combine( expectedFolder, Path.GetFileNameWithoutExtension( file ) + ".txt" ) ) ) ;
      }

      foreach ( var compareFile in compareFiles ) {
        using ( var fileStream1 = File.OpenRead( compareFile.dump ) ) {
          using ( var fileStream2 = File.OpenRead( compareFile.expected ) )
            Assert.AreEqual( (Stream) fileStream1, (Stream) fileStream2, Path.GetFileNameWithoutExtension( compareFile.dump ), (object[]) null ) ;
        }
      }
    }

    public void CheckMultipleFilesVertex()
    {
      // TODO 複数ファイル読み込んで全体でのフリーバーテックスの確認は別途必要
    }
  }
}