using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Chiyoda.CAD.Core;
using Chiyoda.UI.PropertyUI;
using Chiyoda.UI.PropertyViewPresenters;
using UnityEngine;

namespace Chiyoda.UI
{
  enum PropertyVisibility
  {
    Hidden,
    ReadOnly,
    Editable,
  }

  enum PropertyCategory
  {
    BaseData,
    ComponentName,
    EquipmentName,
    EquipNo,
    EquipmentType,
    StructureId,
    StockNumber,
    LineNumber,
    StreamName,
    ServiceClass,
    Dimension,
    Position,
    AutoRouting,
    Insulation,
    OtherValues,
    UserDefinedValues,
    MaxValue = UserDefinedValues,
  }

  enum ValueType
  {
    Auto,
    Label,
    Text,
    Length,
    Position,
    Position2D,
    Rotation,
    GeneralNumeric,
    GeneralInteger,
    Button,
    CheckBox,
    Select,
    Composite,
    DiameterRange,
  }

  static class PropertyCategoryNames
  {
    private static Regex _splitByWord = new Regex( "([a-z])([A-Z])", RegexOptions.Singleline | RegexOptions.CultureInvariant );

    public static string GetName( PropertyCategory category )
    {
      var categoryName = (PropertyCategory.UserDefinedValues == category) ? "UserDefinedValues" : category.ToString();
      return _splitByWord.Replace( categoryName, match => match.Groups[1].Value + " " + match.Groups[2].Value );
    }
  }

  /*
    LineNumberIndex,
    ServiceClass,
    CatalogueName,
    MainDiameter,
    BranchDiameter,
    FaceToFaceDimension,
    Position,
    Rotation,
    UserDefined,*/

  class PropertyView : MonoBehaviour
  {
    private Document _document;
    private IPropertyViewPresenter _presenter;

    [SerializeField]
    private GameObject contents;

    [SerializeField]
    private GameObject categoryPrefab;

    [SerializeField]
    private FocusController focusController;

    public Document Document
    {
      get { return _document; }
      set
      {
        if ( object.ReferenceEquals( _document, value ) ) return;

        if ( null != _document ) {
          _document.SelectionChanged -= Document_SelectionChanged;
        }

        _document = value;

        if ( null != _document ) {
          _document.SelectionChanged += Document_SelectionChanged;
        }

        Document_SelectionChanged( _document, new ItemChangedEventArgs<IElement>( Array.Empty<IElement>(),
                                                                                  Array.Empty<IElement>() ) );
      }
    }

    private IEnumerable<PropertyCategoryView> CategoryViews => contents.transform.Cast<Transform>().Select( t => t.GetComponent<PropertyCategoryView>() ).Where( x => x != null );

    public void UpdatePropertyValues()
    {
      CategoryViews.SelectMany( v => v.PropertyItems ).ForEach( piv => piv.UpdateValues() );
    }

    public void UpdatePropertyView()
    {
      Clear();

      var hasUserDefinedCategory = false;

      foreach ( var pair in _presenter.PropertyMappers ) {
        var view = CreateCategoryView( pair.Key );
        if ( null == view ) continue;

        AddCategoryView( view );

        foreach ( var mapper in pair.Value ) {
          var itemView = view.CreateItemView( _presenter, mapper );
          if ( null == itemView ) continue;

          view.PropertyItems.Add( itemView );
        }

        if ( PropertyCategory.UserDefinedValues == pair.Key ) {
          hasUserDefinedCategory = true;
          CreateUserDefinedItemViews( view, _presenter );
        }
      }
      if ( false == hasUserDefinedCategory ) {
        CreateUserDefinedItemViews( null, _presenter );
      }
      
      focusController.UpdateFocusList();
    }

    private PropertyCategoryView CreateCategoryView( PropertyCategory category )
    {
      var categoryView = Instantiate( categoryPrefab ).GetComponent<PropertyCategoryView>();
      categoryView.Name = PropertyCategoryNames.GetName( category );
      return categoryView;
    }

    private void AddCategoryView( PropertyCategoryView view )
    {
      view.transform.SetParent( contents.transform );
    }

    private void CreateUserDefinedItemViews( PropertyCategoryView view, IPropertyViewPresenter presenter )
    {
      var mappers = presenter.UserDefinedPropertyMappers;
      if ( null == mappers ) return;

      var list = mappers.ToList();
      if ( 0 == list.Count ) return;

      // Categoryの追加
      if ( null == view ) {
        view = CreateCategoryView( PropertyCategory.UserDefinedValues );
        if ( null == view ) return;

        AddCategoryView( view );
      }

      // 実際のビュー作成
      foreach ( var mapper in mappers ) {
        var itemView = view.CreateItemView( presenter, mapper );
        if ( null == itemView ) continue;

        view.PropertyItems.Add( itemView );
      }
    }

    private void Clear()
    {
      var list = new List<GameObject>( contents.transform.childCount );
      foreach ( Transform t in contents.transform ) {
        list.Add( t.gameObject );
      }
      contents.transform.DetachChildren();

      foreach ( var go in list ) {
        var view = go.GetComponent<PropertyCategoryView>();
        if ( null != view ) {
          view.Remove();
        }
        else {
          Destroy( go );
        }
      }
    }

    private void Document_SelectionChanged( object sender, ItemChangedEventArgs<IElement> args )
    {
      if ( null == _document ) {
        _presenter = null;
      }
      else {
        var removedCount = args.RemovedItems.Except( args.AddedItems ).Count();
        var addedCount = args.AddedItems.Except( args.RemovedItems ).Count();
        if ( 0 == removedCount && 0 == addedCount ) {
          return; // 選択済み要素が再選択されたので無視する
        }

        switch ( _document.SelectedElementCount ) {
          case 0:
            _presenter = null;
            break;

          case 1:
            _presenter = PropertyViewPresenter.CreateForSingleObject( _document.SelectedElements.Single() );
            break;

          default:
            _presenter = PropertyViewPresenter.CreateForMultiObject( _document.SelectedElements );
            break;
        }
      }

      if ( null == _presenter ) {
        _presenter = PropertyViewPresenter.Empty;
      }

      UpdatePropertyView();
    }
  }
}
