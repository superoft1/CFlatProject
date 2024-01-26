using System.Collections.Generic ;
using System.Linq ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal partial class RackBase<T>
  {
    public class PipeRackLayerControl 
    {
      private readonly RackBase<T> _rack ;
      private readonly int _layer ;

      private MaterialSelectionViewModel _columnViewModel ;
      private MaterialSelectionViewModel _beamViewModel ;
      private MaterialSelectionViewModel _sideBeamViewModel ;
      
      public PipeRackLayerControl( RackBase<T> rack, int layer )
      {
        _rack = rack ;
        _layer = layer ;
        
        _columnViewModel = new MaterialSelectionViewModel( _rack._frames[0][layer].ColumnMaterial );
        _beamViewModel = new MaterialSelectionViewModel( _rack._frames[0][_layer].BeamMaterial );
        _sideBeamViewModel = new MaterialSelectionViewModel( _rack._connections[0][_layer].BeamMaterial );
      }
    
      [UI.Property( UI.PropertyCategory.OtherValues, "Height", Visibility = UI.PropertyVisibility.ReadOnly, Order = 1 )]
      public double HeightFromGround => _rack.HeightFromGround( _layer ) ;

      [UI.Property( UI.PropertyCategory.OtherValues, "Floor Height",
        ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 2 )]
      public double Height
      {
        get => _rack._frames[0][ _layer ].Height ;
        set
        {
          _rack._frames.SetFloorHeight( _layer , value ) ;
          _rack._connections.ForEach( u => u.SetHeight( _layer, value ) ) ;
        }
      }
      
      [UI.Property( UI.PropertyCategory.OtherValues, "Side Beam Offset", 
        ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 4 )]
      public double SideBeamOffset
      {
        get => _rack._connections.Select( u => u[_layer].HeightOffset ).Max() ;
        set => _rack.SetSideBeamOffset( _layer, value ) ;
      }

      [UI.Property( UI.PropertyCategory.OtherValues, "Beam Shape",
        ValueType = UI.ValueType.Select, ListDataMethodName = "ShapeTypes", Order = 10 )]
      public int BeamShape
      {
        
        get => _beamViewModel.ShapeType ;
        set
        {
          if ( _beamViewModel.ShapeType == value ) {
            return ;
          }

          _beamViewModel.ShapeType = value ;
          _beamViewModel.SetMaterialIndex( _beamViewModel.CurrentSpec.Beam( _rack.Width ) ) ;
          _rack._frames.SetLayerBeamMaterial( _layer, _beamViewModel.Current ) ;
        }
      }

      [UI.Property( UI.PropertyCategory.OtherValues, "Beam",
        ValueType = UI.ValueType.Select, ListDataMethodName = "BeamMaterials", Order = 11 )]
      public int BeamMaterialIndex
      {
        get => _beamViewModel.CurrentMaterialIndex ;
        set
        {
          _beamViewModel.CurrentMaterialIndex = value ;
          _rack._frames.SetLayerBeamMaterial( _layer, _beamViewModel.Current ) ;
        }
      }

      [UI.Property( UI.PropertyCategory.OtherValues, "Column Shape",
        ValueType = UI.ValueType.Select, ListDataMethodName = "ShapeTypes", Order = 12 )]
      public int ColumnShape
      {
        get => _columnViewModel.ShapeType ;
        set
        {
          if ( _columnViewModel.ShapeType == value ) {
            return ;
          }

          _columnViewModel.ShapeType = value ;
          _columnViewModel.SetMaterialIndex( _columnViewModel.CurrentSpec.Column( _beamViewModel.Current ) );
          _rack._frames.SetLayerColumnMaterial( _layer, _columnViewModel.Current ) ; 
        }
      }

      [UI.Property( UI.PropertyCategory.OtherValues, "Column",
        ValueType = UI.ValueType.Select, ListDataMethodName = "ColumnMaterials", Order = 13 )]
      public int ColumnMaterialIndex
      {
        get => _columnViewModel.CurrentMaterialIndex ;
        set
        {
          _columnViewModel.CurrentMaterialIndex = value ;
          _rack._frames.SetLayerColumnMaterial( _layer, _columnViewModel.Current ) ;
        }
      }
      
      [UI.Property( UI.PropertyCategory.OtherValues, "Side Beam Shape",
        ValueType = UI.ValueType.Select, ListDataMethodName = "ShapeTypes", Order = 14 )]
      public int SideBeamShape
      {
        get => _sideBeamViewModel.ShapeType ;
        set
        {
          if ( _sideBeamViewModel.ShapeType == value ) {
            return ;
          }

          _sideBeamViewModel.ShapeType = value ;
          _sideBeamViewModel.SetMaterialIndex( _sideBeamViewModel.CurrentSpec.Beam( _rack.BeamInterval ) );
          _rack.SetSideBeamMaterial( _sideBeamViewModel.Current, _layer ) ;
        }
      }

      [UI.Property( UI.PropertyCategory.OtherValues, "Side Beam",
        ValueType = UI.ValueType.Select, ListDataMethodName = "SideBeamMaterials", Order = 15 )] 
      public int SideBeamMaterialIndex
      {
        get => _sideBeamViewModel.CurrentMaterialIndex ;
        set
        {
          _sideBeamViewModel.CurrentMaterialIndex = value ;
          _rack.SetSideBeamMaterial( _sideBeamViewModel.Current, _layer ) ;
        }
      }
      
      public IList<string> ShapeTypes => MaterialSelectionViewModelHelper.ShapeTypes ;
      public IList<string> SideBeamMaterials => _sideBeamViewModel.MaterialList ;
      public IList<string> ColumnMaterials => _columnViewModel.MaterialList ;
      public IList<string> BeamMaterials => _beamViewModel.MaterialList ;
    }
  }
}