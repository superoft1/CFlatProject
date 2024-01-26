using System;
using System.Collections ;
using System.Collections.Generic;
using System.Configuration ;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Body ;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;
using Chiyoda.UI.PropertyViewPresenters;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chiyoda.UI.PropertyUI
{
  abstract class PropertyItemView : MonoBehaviour
  {
    [SerializeField]
    private Text label;

    private bool _updateObjectValue = true;

    public PropertyCategoryView CategoryView { get; set; }

    public IPropertyViewPresenter PropertyViewPresenter { get; private set; }
    public IObjectValueMapper ObjectValueMapper { get; private set; }

    public void SetupPresenter( IPropertyViewPresenter presenter, IObjectValueMapper mapper )
    {
      if ( null == presenter ) {
        throw new NullReferenceException( "presenter" );
      }

      if ( null == mapper ) {
        throw new NullReferenceException( "mapper" );
      }

      Label = mapper.Label;
      PropertyViewPresenter = presenter;
      ObjectValueMapper = mapper;

      OnPresenterSet();

      SetProperty( presenter.GetProperty( mapper ), presenter.GetValue( mapper ), presenter.GetValueList( mapper ) ) ;

      switch ( presenter.GetVisibility( mapper ) ) {
        case PropertyVisibility.Hidden:
          gameObject.SetActive( false );
          break;

        case PropertyVisibility.ReadOnly:
          IsReadOnly = true;
          break;

        default:
          ValueChanged += ( sender, e ) =>
          {
            if ( _updateObjectValue ) {
              presenter.SetValue( mapper, Value );
            }
          };
          IsReadOnly = false;
          break;
      }

      SetSpecificProperty();
    }

    protected virtual void OnPresenterSet() { }

    protected virtual void SetSpecificProperty() { }

    public virtual string Label
    {
      get { return label.text; }
      protected set { label.text = value; }
    }

    public void SetProperty( INamedProperty prop, object value )
    {
      SetProperty( prop, value, null ) ;
    }

    public void SetProperty( INamedProperty prop, object value, IEnumerable listData )
    {
      if ( Property != prop ) {
        Property = prop ;
        OnPropertyChanged() ;
      }
      
      SetList( listData ) ;

      Value = value ;
    }

    protected virtual void OnPropertyChanged()
    {
    }

    protected virtual void SetList( IEnumerable listData )
    {
    }

    protected INamedProperty Property { get ; private set ; }

    public abstract object Value { get; protected set; }

    public abstract bool IsReadOnly { get; set; }

    public event EventHandler ValueChanged;

    protected virtual void OnValueChanged( EventArgs e )
    {
      ValueChanged?.Invoke( this, e );
    }

    protected void ReapplyCurrentValue()
    {
      if ( null != PropertyViewPresenter ) {
        SetList( PropertyViewPresenter.GetValueList( ObjectValueMapper ) ) ;
        Value = PropertyViewPresenter.GetValue( ObjectValueMapper );
      }
    }

    public void UpdateValues()
    {
      _updateObjectValue = false; // PropertyViewへ反映する際にはObjectへの値の再設定を控える

      ReapplyCurrentValue();

      _updateObjectValue = true;
    }



#if UNITY_EDITOR
    public abstract class PropertyItemViewEditor<TPropertyItemView> : Editor
      where TPropertyItemView : PropertyItemView
    {
      public override void OnInspectorGUI()
      {
        var view = target as TPropertyItemView;

        if ( null != view.label ) {
          view.Label = EditorGUILayout.TextField( "Property Name", view.Label );
          view.IsReadOnly = EditorGUILayout.Toggle( "Read Only", view.IsReadOnly );
        }

        OnValueGUI( view );
      }

      protected abstract void OnValueGUI( TPropertyItemView view );
    }
#endif
  }
}
