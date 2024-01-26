using System ;
using System.Collections.Generic ;
using System.Xml ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.IO
{
  public class SerializationContext
  {
    public int Version { get ; }
    private readonly XmlWriter _writer ;
    public Document Document { get ; }
    private int _storageCounter = 0 ;
    private readonly Dictionary<object, int> _objectStorage = new Dictionary<object, int>() ;
    private readonly Dictionary<object, int> _lateSerializeStorage = new Dictionary<object, int>() ;

    public SerializationContext( XmlWriter writer, Document doc )
    {
      _writer = writer ;
      Document = doc ;
      Version = 0 ;
    }

    public bool WriteDocument()
    {
      _storageCounter = 0 ;
      return WriteValue( "document", Document ) ;
    }

    private bool Add( object obj, out int id )
    {
      if ( _objectStorage.TryGetValue( obj, out id ) ) return false ;

      if ( false == _lateSerializeStorage.TryGetValue( obj, out id ) ) {
        id = ++_storageCounter ;
      }

      _objectStorage.Add( obj, id );
      _lateSerializeStorage.Remove( obj ) ;
      return true ;
    }
    private bool AddLate( object obj, out int id )
    {
      if ( _objectStorage.TryGetValue( obj, out id ) ) return false ;

      if ( false == _lateSerializeStorage.TryGetValue( obj, out id ) ) {
        id = ++_storageCounter ;
        _lateSerializeStorage.Add( obj, id ) ;
      }

      return true ;
    }

    public void WriteString( string value )
    {
      _writer.WriteString( value ) ;
    }
    
    public bool WriteField( TypeField field, object value )
    {
      return WriteElement( field.Name, value, field.IsLateSerialize ) ;
    }
    public bool WriteValue( string name, object value )
    {
      return WriteElement( name, value, false ) ;
    }
    public bool WriteReferenceOnly( string name, object value )
    {
      return WriteElement( name, value, true ) ;
    }

    private bool WriteElement( string name, object value, bool isLateSerialize )
    {
      if ( null == value ) {
        _writer.WriteStartElement( name ) ;
        _writer.WriteEndElement() ;
        return true ;
      }

      if ( false == value.GetType().IsValueType ) {
        if ( isLateSerialize ) {
          if ( LateSerialize( name, value ) ) return true ;
          // 遅延シリアライズ不要
        }
      }

      var serializer = Serializers.GetSerializer( value.GetType(), Version ) ;
      if ( null == serializer ) return false ;

      bool result = true ;
      _writer.WriteStartElement( name ) ;
      if ( false == serializer.CanRefer || false == WriteReference( value ) ) {
        result = serializer.Write( this, value ) ;
      }
      _writer.WriteEndElement() ;
      
      return result ;
    }

    private bool WriteReference( object value )
    {
      if ( Add( value, out var id ) ) {
        _writer.WriteAttributeString( "id", id.ToString() );
        _writer.WriteAttributeString( "type", TypeManager.GetTypeName( value.GetType() ) ) ;
        return false ;
      }
      else {
        _writer.WriteAttributeString( "ref", id.ToString() );
        return true ;
      }
    }

    private bool LateSerialize( string name, object value )
    {
      if ( false == AddLate( value, out var id ) ) return false ;

      _writer.WriteStartElement( name ) ;
      _writer.WriteAttributeString( "ref", id.ToString() ) ;
      _writer.WriteEndElement() ;

      return true ;
    }
  }
  public class DeserializationContext : IDisposable
  {
    public int Version { get ; }
    private readonly DeserializationContext _parentContext ;
    private readonly XmlReader _reader ;
    public Document Document { get ; }
    private readonly Dictionary<int, object> _objectStorage ;
    private readonly Dictionary<int, Action<object>> _lateSerializeStorage ;
    private readonly Dictionary<object, Action<object>> _postProcesses ;
    
    public object Parent { get ; private set ; }

    public IDisposable PushParent( object parent )
    {
      if ( null == parent ) return null ;
      return new ElementPusher( this, parent ) ;
    }

    public DeserializationContext( XmlReader reader, Document doc )
      : this( reader, doc, null )
    {
    }

    private DeserializationContext( XmlReader reader, Document doc, DeserializationContext parentContext )
    {
      _reader = reader ;
      Document = doc ;
      _parentContext = parentContext ;
      Parent = parentContext?.Parent ;
      _objectStorage = parentContext?._objectStorage ?? new Dictionary<int, object>() ;
      _lateSerializeStorage = parentContext?._lateSerializeStorage ?? new Dictionary<int, Action<object>>() ;
      _postProcesses = parentContext?._postProcesses ?? new Dictionary<object, Action<object>>() ;
    }

    public void Dispose()
    {
      _reader?.Dispose() ;
      _parentContext?._reader?.Read() ;
    }

    private DeserializationContext PushContext()
    {
      var subtreeReader = _reader.ReadSubtree() ;
      subtreeReader.Read() ;

      return new DeserializationContext( subtreeReader, Document, this ) ;
    }

    private bool Get( int id, out object obj )
    {
      return _objectStorage.TryGetValue( id, out obj ) ;
    }

    public bool Add( int id, object obj )
    {
      if ( _objectStorage.ContainsKey( id ) ) return false ;

      _objectStorage.Add( id, obj ) ;

      if ( _lateSerializeStorage.TryGetValue( id, out var actions ) ) {
        _lateSerializeStorage.Remove( id ) ;
        actions.Invoke( obj );
      }
      
      return true ;
    }

    public bool ReadDocument()
    {
      using ( Document.History.SuppressRegister() ) {
        return ReadIntoValue( "document", typeof( Document ), Document ) ;
      }
    }

    public string ReadString()
    {
      return _reader.ReadString() ;
    }

    public bool ReadString( string name, out string value )
    {
      value = null ;
      if ( ! _reader.IsStartElement( name ) ) return false ;
      if ( _reader.IsEmptyElement ) return true ;

      _reader.ReadStartElement() ;
      value = _reader.ReadString() ;
      _reader.ReadEndElement() ;
      return true ;
    }

    public bool ReadField( object obj, TypeField field )
    {
      if ( field.DeserializeWithSetValue ) {
        if ( ! ReadValue<object>( field.Name, field.SerializeType, value => field.SetValue( obj, value ) ) ) return false ;
      }
      else {
        var value = field.GetValue( obj ) ;
        if ( ! ReadIntoValue( field.Name, field.SerializeType, value ) ) return false ;
      }

      return true ;
    }

    public bool ReadValue<T>( string name, Type type, Action<T> onRead )
    {
      if ( ! _reader.IsStartElement( name ) ) {
        onRead( default ) ;
        return false ;
      }

      // 参照型の場合は参照用処理
      if ( false == type.IsValueType ) {
        return ReadObject( type, onRead ) ;
      }
      
      var serializer = Serializers.GetSerializer( type, Version ) ;
      if ( null == serializer ) {
        onRead( default ) ;
        return false ;
      }

      using ( var con = PushContext() ) {
        con._reader.ReadStartElement() ;
        return serializer.Read( con, obj => onRead( (T) obj ) ) ;
      }
    }

    private bool ReadObject<T>( Type type, Action<T> onRead )
    {
      if ( GetReference( obj => onRead( (T) obj ) ) ) {
        // 参照が既にある
        _reader.Read() ;
        return true ;
      }

      if ( _reader.IsEmptyElement ) {
        // 参照型かつ空要素なら、default==null
        onRead( default ) ;
        _reader.Read() ;
        return true ;
      }

      using ( var con = PushContext() ) {
        ISerializer serializer ;
        if ( ! con.GetObjectID( out var id ) ) {
          serializer = Serializers.GetSerializer( type, Version ) ;
        }
        else {
          if ( ! con.GetTargetType( out var storedType ) ) return false ;
          if ( ! type.IsAssignableFrom( storedType ) ) return false ;

          serializer = Serializers.GetSerializer( storedType, Version ) ;
        }

        if ( null == serializer ) return false ;

        con._reader.ReadStartElement() ;
        return serializer.Read( con, obj =>
        {
          Add( id, obj ) ;
          ExecutePostProcess( obj ) ;
          onRead( (T)obj ) ;
        } ) ;
      }
    }
    private bool GetReference( Action<object> onRead )
    {
      // 既存参照チェック
      var refstr = _reader.GetAttribute( "ref" ) ;
      if ( null == refstr || false == int.TryParse( refstr, out var id ) ) return false ;

      if ( Get( id, out var obj ) ) {
        onRead( obj ) ;
        return true ;
      }
      
      // 将来参照チェック
      if ( _lateSerializeStorage.TryGetValue( id, out var actions ) ) {
        actions += onRead ;
        _lateSerializeStorage[ id ] = actions ;
      }
      else {
        _lateSerializeStorage.Add( id, onRead ) ;
      }

      return true ;
    }

    private bool GetObjectID( out int id )
    {
      id = default ;

      var idstr = _reader.GetAttribute( "id" ) ;
      if ( null == idstr || false == int.TryParse( idstr, out id ) ) return false ;

      if ( _objectStorage.ContainsKey( id ) ) return false ;

      return true ;
    }

    private bool GetTargetType( out Type type )
    {
      var typestr = _reader.GetAttribute( "type" ) ;
      if ( null == typestr ) {
        type = null ;
        return false ;
      }
      type = TypeManager.FromTypeName( typestr ) ;
      return ( null != type ) ;
    }

    private bool ReadIntoValue( string name, ISerializer serializer, in object value )
    {
      if ( ! _reader.IsStartElement( name ) ) return false ;

      using ( var con = PushContext() ) {
        con._reader.ReadStartElement() ;

        if ( serializer.ReadInto( con, value ) ) {
          ExecutePostProcess( value ) ;
          return true ;
        }

        return false ;
      }
    }

    private bool ReadIntoValue( string name, Type type, in object value )
    {
      var serializer = Serializers.GetSerializer( type, Version ) ;
      if ( null == serializer ) return false ;

      return ReadIntoValue( name, serializer, value ) ;
    }

    public bool IsEmpty()
    {
      return _reader.IsEmptyElement ;
    }

    public bool GetNextElementName( out string name )
    {
      name = null ;
      if ( ! _reader.IsStartElement() ) return false ;

      name = _reader.Name ;
      return true ;
    }
    
    private void ExecutePostProcess( object value )
    {
      if ( _postProcesses.TryGetValue( value, out var actions ) ) {
        actions?.Invoke( value ) ;
      }
      else {
        _postProcesses.Add( value, null ) ;
      }
    }

    public void RegisterPostProcess( object value, Action<object> action )
    {
      if ( _postProcesses.TryGetValue( value, out var actions ) ) {
        if ( null == actions ) {
          // ExecutePostProcess済みなので、後から実行
          action( value ) ;
        }
        else {
          actions += action ;
          _postProcesses[ value ] = actions ;
        }
      }
      else {
        _postProcesses.Add( value, action ) ;
      }
    }

    
    
    private class ElementPusher : IDisposable
    {
      private readonly DeserializationContext _con ;
      private readonly object _orgParent ;

      public ElementPusher( DeserializationContext con, object parent )
      {
        _con = con ;
        _orgParent = con.Parent ;
        con.Parent = parent ;
      }

      void IDisposable.Dispose()
      {
        _con.Parent = _orgParent ;
      }
    }
  }
}