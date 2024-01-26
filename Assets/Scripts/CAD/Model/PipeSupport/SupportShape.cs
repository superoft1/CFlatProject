using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Model
{
  public abstract class SupportShape : Entity
  {
    private readonly Memento<string> _stockNumber ;

    public string StockNumber
    {
      get => _stockNumber.Value ;
      set => _stockNumber.Value = value ;
    }

    private SupportShape( Document document ) : base( document )
    {
      _stockNumber = CreateMemento<string>() ;
    }
  }
}