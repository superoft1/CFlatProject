using System ;

namespace Chiyoda.CAD.IO
{
  [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = true )]
  public class CustomSerializerAttribute : Attribute
  {
    public int Version { get ; set ; } = int.MinValue ;
    public ISerializer Serializer { get ; }

    public CustomSerializerAttribute( Type serializerType )
    {
      Serializer = Activator.CreateInstance( serializerType ) as ISerializer ;
    }
  }
}