using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Chiyoda.CAD.Util;

namespace IDF
{
  public class RecordManager
  {
    public List<Record> Records { get ; set ; } = new List<Record>() ;
    public List<Record> Current { get ; set ; }

    public List<Record> Support { get ; set ; } = new List<Record>() ;

    public Line Line { get ; set ; }

    public RecordManager( Line line )
    {
      Current = Records ;
      Line = line ;
    }

    public void Add( IDFEntityImporter importer )
    {
      if ( importer.FittingType == IDFRecordType.FittingType.PipeHanger ) {
        if ( Current.Count > 0 ) {
          Support.Add( new Record( importer, Current[ 0 ].ParentBranch, Current.Last() ) ) ;
        }

        return ;
      }

      if ( Current.Count > 0 ) {
        var last = Current.Last() ;
        Current.Add( new Record( importer, Current[ 0 ].ParentBranch, last ) ) ;
        last.NextRecord = last.NextRecord == null ? Current.Last() : null ;
      }
      else {
        Current.Add( new Record( importer ) ) ;
      }
    }

    public void AddAsBranch( IDFEntityImporter importer )
    {
      if ( Current.Count < 1 ) {
        Add( importer ) ;
        return ;
      }

      var parent = Current.Last() ;

      parent.Children.Add( new List<Record> { new Record( importer, parent, parent ) }) ;
      parent.NextRecord = parent.NextRecord == null ? parent.Children.Last().Last() : null ;

      Current = parent.Children.Last() ;
    }

    public IDFEntityImporter FrontOfCurrentBranch()
    {
      if ( Current.Any() ) {
        return Current[ 0 ].Body ;
      }

      return null ;
    }

    public IDFEntityImporter LastOfCurrentBranch()
    {
      if ( Current.Any() ) {
        return Current.Last().Body ;
      }

      return null ;
    }


    public IEnumerable<Record> CurrentBranchRecords()
    {
      foreach ( var record in Current ) {
        yield return record ;
      }
    }

    public void TerminateCurrentBranch()
    {
      if ( ! Current.Any() ) return ;
      var parent = Current.First().ParentBranch ;
      if ( parent == null ) {
        return ;
      }

      var recordsQue = new Queue<List<Record>>() ;
      recordsQue.Enqueue( Records ) ;
      while ( recordsQue.Count > 0 ) {
        var records = recordsQue.Dequeue() ;
        if ( records.Contains( parent ) ) {
          Current = records ;
          return ;
        }

        foreach ( var record in records ) {
          record.Children.ForEach(r=>recordsQue.Enqueue(r));
        }
      }
    }

    public IEnumerable<IDFEntityImporter> AllImporter()
    {
      foreach ( var records in AllRecords() ) {
        foreach ( var record in records ) {
          if ( record.BuildSuccess ) {
            yield return record.Body ;
          }
        }
      }
    }

    public IEnumerable<List<Record>> AllRecords()
    {
      var recordsQue = new Queue<List<Record>>() ;
      recordsQue.Enqueue( Records ) ;
      while ( recordsQue.Count > 0 ) {
        var records = recordsQue.Dequeue() ;
        yield return records ;
        foreach ( var record in records ) {
          record.Children.ForEach(r=>recordsQue.Enqueue(r));
        }
      }
    }
  }
}