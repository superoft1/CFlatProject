using System.Collections.Generic;

namespace Chiyoda.DB
{
  public abstract class TableBase
  {
    protected DB _referenceDB;

    internal void Init(DB db)
    {
      _referenceDB = db;
      Init();
    }

    protected abstract void Init();
  }
}