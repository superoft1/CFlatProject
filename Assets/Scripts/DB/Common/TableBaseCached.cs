using System.Collections.Generic;

namespace Chiyoda.DB
{
  public abstract class TableBaseCached : TableBase
  {
    protected IList<RecordBase> __cache;

    protected override void Init()
    {
      __cache = Query();
    }
    protected abstract IList<RecordBase> Query();
  }
}
