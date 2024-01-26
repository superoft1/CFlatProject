namespace Chiyoda.CAD.Model.Structure
{
  internal class RcMaterial : IStructuralMaterial
  {
    public RcMaterial( DB.StructureElementTable.Record r ) 
      : this( r.Standard,
        0.001 * ( r.H_D ?? 300 ),
        0.001 * ( r.B ?? 300 ),
        MaterialRotation.Rot0 )
    {}

    private RcMaterial( string name, double mainSize, double subSize, MaterialRotation rot )
    {
      Name = name ;
      MainSize = mainSize ;
      SubSize = subSize ;
      Rotation = rot ;
    }

    public IStructuralMaterial CreateCopy()
    {
      return new RcMaterial( Name, MainSize, SubSize, Rotation );
    }

    public string Name { get ; }
    public bool IsSteel => false ;
    public SteelShapeType ShapeType => SteelShapeType.Unknown ;
    public double MainSize { get ; }
    
    public double SubSize { get ; }
    public MaterialRotation Rotation { get ; }
  }
}