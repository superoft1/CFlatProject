using System.Collections.Generic;

namespace IDF
{
  public class Record
    {
      public Record()
      {
      }

      public Record(IDFEntityImporter imp) : this()
      {
        Body = imp;
      }

      public Record(IDFEntityImporter imp, Record parent, Record prev) : this(imp)
      {
        ParentBranch = parent;
        PrevRecord = prev;
      }

      public IDFEntityImporter Body { get; }
      public List<List<Record>> Children { get ; } = new List<List<Record>>() ;
      public Record ParentBranch { get; }
      public Record PrevRecord { get; }
      
      /// <summary>
      /// 分岐が複数ある場合は未対応なのでnullを設定しているので注意
      /// </summary>
      public Record NextRecord { get; set; }
      
      public bool BuildSuccess { get ; set ; } = true ;
    }
}