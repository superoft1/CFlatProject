using System ;
using System.Collections;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;
using UnityEngine.UI ;

namespace Chiyoda.UI.PropertyUI
{
  class DiameterRangePropertyItemView : PropertyItemView
  {
    [SerializeField]
    public Dropdown dropdown;

    private List<Dropdown.OptionData> _defaultOptions;

    private DiameterRange _diameterRange;
    private readonly List<Tuple<int, int>> _diameterRangeNpsMms = new List<Tuple<int, int>>();

    private DiameterRange GetDiameterRange(object obj)
    {
      switch ( obj ) {
        case PipingPiece piece :
          return DiameterFactory.GetDiameterRangeFromLine(piece.LeafEdge.Line);
        case IDiameterRange range :
          return range.DiameterRange;
        default :
          throw new NullReferenceException();
      }
    }

    private void CreateOptionRange()
    {
      _diameterRangeNpsMms.Clear();
      if (PropertyViewPresenter.Target is object[])
      {
        foreach(var obj in (object[])PropertyViewPresenter.Target)
        {
          var range = GetDiameterRange(obj);
          _diameterRangeNpsMms.Add(new Tuple<int, int>(range.MinDiameterNpsMm, range.MaxDiameterNpsMm));
        }
      }
      else
      {
        var range = GetDiameterRange(PropertyViewPresenter.Target);
        _diameterRangeNpsMms.Add(new Tuple<int, int>(range.MinDiameterNpsMm, range.MaxDiameterNpsMm));
      }

      //_diameterRangeNpsMmsの要素が複数だったとしても、後に範囲が一つでも違っていたらドロップダウンリストのグレーアウト処理が入るので_diameterRange生成時に0番目の要素を用いる
      _diameterRange = new DiameterRange(_diameterRangeNpsMms[0].Item1, _diameterRangeNpsMms[0].Item2);
    }

    private void CreateOptions()
    {
      if(_diameterRange == null)
      {
        throw new NullReferenceException();
      }

      var minIndex = _diameterRange.GetMinDiameterIndex();
      var maxIndex = _diameterRange.GetMaxDiameterIndex();
      var list = new List<Dropdown.OptionData>(maxIndex - minIndex + 1);

      for (int i = minIndex; i <= maxIndex; ++i)
      {
        var data = new Dropdown.OptionData($"{_diameterRange.GetInchiDisp(i)} in");
        list.Add(data);
      }
      _defaultOptions = list;

      dropdown.ClearOptions();
      dropdown.AddOptions(_defaultOptions);
    }

    protected override void SetSpecificProperty()
    {
      if(_diameterRangeNpsMms.Count > 1)
      {
        var baseMinDiameterNpsMm = int.MaxValue;
        var baseMaxDiameterNpsMm = int.MaxValue;
        foreach(var rangeNpsMm in _diameterRangeNpsMms)
        {
          if (baseMinDiameterNpsMm == int.MaxValue && baseMaxDiameterNpsMm == int.MaxValue)
          {
            baseMinDiameterNpsMm = rangeNpsMm.Item1;
            baseMaxDiameterNpsMm = rangeNpsMm.Item2;
          }
          else
          {
            if ((rangeNpsMm.Item1 != baseMinDiameterNpsMm) || (rangeNpsMm.Item2 != baseMaxDiameterNpsMm))
            {
              //TargetのDiameterRangeの範囲が一つでも違ったらドロップダンリストをグレーアウト
              IsReadOnly = true;
              break;
            }
          }
        }

        if (IsReadOnly)
        {
          //このブロックでのIsReadOnly=trueの際は、複数選択されたドロップダンリストの値が違うかNPSの範囲が違うのでリストを空欄にする
          dropdown.ClearOptions();
        }
      }
    }

    protected override void OnPropertyChanged()
    {
      if ( Property is IUserDefinedRangedNamedProperty range ) {
        _diameterRange = new DiameterRange(range.MinValue.ToInteger(), range.MaxValue.ToInteger());
        CreateOptions();
      }
      else {
        CreateOptions();
      }
    }

    protected override void SetList(IEnumerable listData)
    {
      switch ( Property ) {
        case null :
          CreateOptionRange();
          CreateOptions();
          break ;
        case IUserDefinedRangedNamedProperty propRange :
        {
          var range = new DiameterRange(propRange.MinValue.ToInteger(), propRange.MaxValue.ToInteger());
          if ( _diameterRange.MinDiameterNpsMm != range.MinDiameterNpsMm || _diameterRange.MaxDiameterNpsMm != range.MaxDiameterNpsMm ) {
            // Miniflow付きポンプでDischargeの径を変更する事によりMiniflowで指定出来る範囲が動的に変わる場合にここに来る
            OnPropertyChanged() ;
          }
          break ;
        }
      }
    }

    public override object Value
    {
      get { return _diameterRange.FromIndex(dropdown.value).NpsMm; }
      protected set
      {
        int dlv = 0 ;

        if ( null == value ) {
          dropdown.value = dropdown.options.Count - 1 ;
          return ;
        }

        if(value is int i)
        {
          dlv = _diameterRange.GetIndex(DiameterFactory.FromNpsMm((double)i));
        }
        else if(value is double d)
        {
          dlv = _diameterRange.GetIndex(DiameterFactory.FromNpsMm(d));
        }
        else
        {
          dropdown.value = dropdown.options.Count - 1;
          return;
        }

        dropdown.value = dlv;
      }
    }

    public override bool IsReadOnly
    {
      get { return ! dropdown.interactable ; }
      set { dropdown.interactable = ! value ; }
    }

    private void Start()
    {
      dropdown.onValueChanged.AddListener( ValueChangedListener ) ;
    }

    private void OnDestroy()
    {
      dropdown.onValueChanged.RemoveListener( ValueChangedListener ) ;
    }

    private void ValueChangedListener( int newValue )
    {
      OnValueChanged( EventArgs.Empty ) ;
      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof(DiameterRangePropertyItemView) )]
    public class DiameterRangePropertyItemViewEditor : PropertyItemViewEditor<DiameterRangePropertyItemView>
    {
      protected override void OnValueGUI(DiameterRangePropertyItemView view )
      {
        if ( null != view.dropdown ) {
          view.dropdown.value = UnityEditor.EditorGUILayout.IntField( "Value", view.dropdown.value ) ;
        }
      }
    }
#endif
  }
}