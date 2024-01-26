using System.Collections.Generic;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Plotplan
{
  [System.Serializable]
  [Entity(EntityType.Type.CompositeUnit)] //暫定
  public class CompositeUnit : Unit
  {
    public enum Type
    {
      None,
      LNGTrain,
      Flare
    }

    protected List<Unit> _unitList; //要Memento化

    public CompositeUnit(Document document) : base(document)
    {
      // _unitList = CreateMementoListAndSetupChildrenEvents<Unit>();
      _unitList = new List<Unit>();
      // _unitList.AfterItemChanged += (sender, e) =>
      {
        // RefreshEndVerticesCache();
      }
      ;
    }

    // public IEnumerable<Unit> UnitList
    // {
    //     get { return _unitList; }
    // }

    //仮実装
    public bool RemoveUnit(Unit unit)
    {
      return _unitList.Remove(unit);
    }

    //仮実装
    public void AddUnit(Unit unit)
    {
      _unitList.Add(unit);
    }
  }
}