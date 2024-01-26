using System.Collections.Generic ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class MaterialSelectionViewModel
  {
    private IList<string> _materialList ;
    private int _materialIndex ;
    private int _shapeIndex ;

    public MaterialSelectionViewModel( IStructuralMaterial material )
    {
      _materialList = MaterialSelectionViewModelHelper.MaterialList( material ) ;
      _materialIndex = _materialList.IndexOf( material.Name ) ;
      _shapeIndex = MaterialSelectionViewModelHelper.TypeToListIndex( material ) ;
    }

    public IList<string> MaterialList => _materialList ;

    public int ShapeType
    {
      get => _shapeIndex ;
      set
      {
        _shapeIndex = value ;
        _materialList = MaterialSelectionViewModelHelper.MaterialList( _shapeIndex ) ;
        _materialIndex = 0 ; 
      }
    }

    public void SetMaterialIndex( IStructuralMaterial mat )
    {
      _materialIndex = _materialList.IndexOf( mat.Name ) ;
    }

    public int CurrentMaterialIndex
    {
      get => _materialIndex ;
      set => _materialIndex = value ;
    }

    public ISpecificationService CurrentSpec =>
      MaterialSelectionViewModelHelper.IsSteel( _shapeIndex )
        ? MaterialDataService.Steel( MaterialSelectionViewModelHelper.ToShapeType( _shapeIndex ) )
        : MaterialDataService.Rc ;

    public IStructuralMaterial Current => CurrentSpec.Get( _materialList[ _materialIndex ] ) ;
  }
}