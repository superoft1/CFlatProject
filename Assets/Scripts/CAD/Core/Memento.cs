using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Chiyoda.CAD.IO ;
using UnityEngine;

namespace Chiyoda.CAD.Core
{
  public static class MementoEqualityComparer<T>
  {
    private static Func<T, T, bool> _comparer;

    static MementoEqualityComparer()
    {
      if ( typeof( T ) == typeof( float ) ) {
        _comparer = Compile<float>( FloatEqualityComparer );
      }
      else if ( typeof( T ) == typeof( double ) ) {
        _comparer = Compile<double>( DoubleEqualityComparer );
      }
      else if ( typeof( T ) == typeof( Vector3 ) ) {
        _comparer = Compile<Vector3>( Vector3fEqualityComparer );
      }
      else if ( typeof( T ) == typeof( Vector3d ) ) {
        _comparer = Compile<Vector3d>( Vector3dEqualityComparer );
      }
      else if ( typeof( T ) == typeof( LocalCodSys3d ) ) {
        _comparer = Compile<LocalCodSys3d>( LocalCodSys3dEqualityComparer );
      }
      else {
        _comparer = EqualityComparer<T>.Default.Equals;
      }
    }

    public static bool Equals( T x, T y )
    {
      return _comparer( x, y );
    }

    // 特定の型の比較演算をTにするためにラムダを経由
    private static Func<T, T, bool> Compile<U>( Func<U, U, bool> equalityComparer )
    {
      var x = Expression.Parameter( typeof( T ), "x" );
      var y = Expression.Parameter( typeof( T ), "y" );

      return Expression.Lambda<Func<T, T, bool>>(
        Expression.Invoke(
          Expression.Constant( equalityComparer ),
          Expression.Convert( x, typeof( U ) ),
          Expression.Convert( y, typeof( U ) )
        ), x, y ).Compile();
    }

    private static bool FloatEqualityComparer( float x, float y )
    {
      var diff = (double)(x - y);
      return (-Tolerance.DistanceTolerance < diff && diff < Tolerance.DistanceTolerance);
    }
    private static bool DoubleEqualityComparer( double x, double y )
    {
      var diff = x - y;
      return (-Tolerance.DistanceTolerance < diff && diff < Tolerance.DistanceTolerance);
    }
    private static bool Vector3fEqualityComparer( Vector3 x, Vector3 y )
    {
      return ((double)Vector3.Distance( x, y ) < Tolerance.DistanceTolerance);
    }
    private static bool Vector3dEqualityComparer( Vector3d x, Vector3d y )
    {
      return (Vector3d.Distance( x, y ) < Tolerance.DistanceTolerance);
    }
    private static bool LocalCodSys3dEqualityComparer( LocalCodSys3d x, LocalCodSys3d y )
    {
      if ( !Vector3dEqualityComparer( x.Origin, y.Origin ) ) return false;
      if ( !Vector3dEqualityComparer( x.DirectionY, y.DirectionY ) ) return false;
      if ( !Vector3dEqualityComparer( x.DirectionZ, y.DirectionZ ) ) return false;
      if ( !Vector3dEqualityComparer( x.DirectionX, y.DirectionX ) ) return false;

      return true;
    }
  }

  public interface IMementoValue : IMemento
  {
    object Value { get ; set ; }
  }

  /// <summary>
  /// 値型に対する履歴化クラス。
  /// </summary>
  /// <typeparam name="T">値型</typeparam>
  public class Memento<T> : AbstractMemento, IMementoValue
  {
    private T _t;

    public T Value
    {
      get { return _t; }
      set
      {
        if ( !MementoEqualityComparer<T>.Equals( _t, value ) ) {
          this.History.RegisterHistoryAction( new HistoryAction( this, value ) );
        }
      }
    }

    object IMementoValue.Value
    {
      get => Value ;
      set => Value = (T) value ;
    }

    internal void CopyFrom( T value )
    {
      this.History.RegisterHistoryAction( new HistoryAction( this, value, true ) );
    }

    /// <summary>値が新規に設定される直前に呼ばれるイベントです。</summary>
    public event EventHandler<ValueChangedEventArgs<T>> BeforeNewlyValueChanged;
    /// <summary>値が新規に設定された直前に呼ばれるイベントです。</summary>
    public event EventHandler<ValueChangedEventArgs<T>> AfterNewlyValueChanged;
    /// <summary>値が履歴操作により変更される直前に呼ばれるイベントです。</summary>
    public event EventHandler<ValueChangedEventArgs<T>> BeforeHistoricallyValueChanged;
    /// <summary>値が履歴操作により変更された直後に呼ばれるイベントです。</summary>
    public event EventHandler<ValueChangedEventArgs<T>> AfterHistoricallyValueChanged;

    /// <summary>値が新規に設定される直前にも履歴操作により変更される直前にも呼ばれるイベントです。</summary>
    public event EventHandler<ValueChangedEventArgs<T>> BeforeValueChanged
    {
      add
      {
        BeforeNewlyValueChanged += value ;
        BeforeHistoricallyValueChanged += value ;
      }
      remove
      {
        BeforeNewlyValueChanged -= value ;
        BeforeHistoricallyValueChanged -= value ;
      }
    }
    /// <summary>値が新規に設定された直後にも履歴操作により変更された直後にも呼ばれるイベントです。</summary>
    public event EventHandler<ValueChangedEventArgs<T>> AfterValueChanged
    {
      add
      {
        AfterNewlyValueChanged += value ;
        AfterHistoricallyValueChanged += value ;
      }
      remove
      {
        AfterNewlyValueChanged -= value ;
        AfterHistoricallyValueChanged -= value ;
      }
    }

    public Memento( IMemorableObject owner ) : base( owner )
    {
    }
    public Memento( IMemorableObject owner, T value ) : this( owner )
    {
      _t = value;
    }

    private void OnBeforeNewlyValueChanged( T oldValue, T newValue )
    {
      BeforeNewlyValueChanged?.Invoke( this, new ValueChangedEventArgs<T>( oldValue, newValue ) );
    }

    private void OnNewlyValueChanged( T oldValue, T newValue )
    {
      AfterNewlyValueChanged?.Invoke( this, new ValueChangedEventArgs<T>( oldValue, newValue ) );
    }

    private void OnBeforeHistoricallyValueChanged( T oldValue, T newValue )
    {
      BeforeHistoricallyValueChanged?.Invoke( this, new ValueChangedEventArgs<T>( oldValue, newValue ) );
    }

    private void OnAfterHistoricallyValueChanged( T oldValue, T newValue )
    {
      AfterHistoricallyValueChanged?.Invoke( this, new ValueChangedEventArgs<T>( oldValue, newValue ) );
    }

    class HistoryAction : HistoryActionBase
    {
      private readonly Memento<T> _memento;
      private readonly T _oldValue;
      private T _newValue;

      public HistoryAction( Memento<T> memento, T newValue, bool withEvent = true )
        : base( memento )
      {
        _memento = memento ;
        _oldValue = memento._t ;
        _newValue = newValue ;

        Redo( true, withEvent ) ;
      }

      public override void Undo()
      {
        _memento.OnBeforeHistoricallyValueChanged( _newValue, _oldValue );

        _memento._t = _oldValue;
        
        _memento.OnAfterHistoricallyValueChanged( _newValue, _oldValue );
      }

      public override void Redo()
      {
        Redo( false, true ) ;
      }

      private void Redo( bool firstTime, bool withEvent )
      {
        if ( withEvent ) {
          if ( firstTime ) _memento.OnBeforeNewlyValueChanged( _oldValue, _newValue ) ;
          else _memento.OnBeforeHistoricallyValueChanged( _oldValue, _newValue ) ;
        }

        _memento._t = _newValue ;

        if ( withEvent ) {
          if ( firstTime ) _memento.OnNewlyValueChanged( _oldValue, _newValue ) ;
          else _memento.OnAfterHistoricallyValueChanged( _oldValue, _newValue ) ;
        }
      }

      public override bool Merge( IHistoryAction action )
      {
        var act = action as HistoryAction;
        if ( null == act ) return false;

        _newValue = act._newValue;
        return true;
      }
    }
  }



  /// <summary>
  /// List&lt;T&gt;型に対する履歴化クラス。
  /// </summary>
  /// <typeparam name="T">値型</typeparam>
  public class MementoList<T> : AbstractMemento, IList<T>, ISerializableList
  {
    private readonly List<T> _list;

    public event EventHandler<ItemChangedEventArgs<T>> AfterNewlyItemChanged;
    public event EventHandler<ItemChangedEventArgs<T>> AfterHistoricallyItemChanged;
    public event EventHandler<ItemChangedEventArgs<T>> AfterItemChanged
    {
      add
      {
        AfterNewlyItemChanged += value ;
        AfterHistoricallyItemChanged += value ;
      }
      remove
      {
        AfterNewlyItemChanged -= value ;
        AfterHistoricallyItemChanged -= value ;
      }
    }

    public MementoList( IMemorableObject owner ) : base( owner )
    {
      _list = new List<T>();
    }
    public MementoList( IMemorableObject owner, int capacity ) : base( owner )
    {
      _list = new List<T>( capacity );
    }
    public MementoList( IMemorableObject owner, IEnumerable<T> collection ) : base( owner )
    {
      _list = new List<T>( collection );
    }

    Type ISerializableList.ItemType => typeof( T ) ;

    void ISerializableList.Add( object item )
    {
      Add( (T) item ) ;
    }

    public int Count => _list.Count ;

    public bool IsReadOnly => false ;

    public T this[int index]
    {
      get => _list[index] ;
      set
      {
        if ( !MementoEqualityComparer<T>.Equals( _list[index], value ) ) {
          this.History.RegisterHistoryAction( new SpliceHistoryAction( this, index, 1, new[] { value } ) ) ;
        }
      }
    }

    public int IndexOf( T item )
    {
      return _list.IndexOf( item );
    }

    public void Insert( int index, T item )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, index, 0, new[] { item } ) );
    }

    public void RemoveAt( int index )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, index, 1, Array.Empty<T>() ) );
    }

    public void Add( T item )
    {
      this.History.RegisterHistoryAction( new MergeableHistoryAction( this, _list.Count, 0, new[] { item } ) );
    }

    public void AddRange( IEnumerable<T> collection )
    {
      var items = collection.ToArray();
      this.History.RegisterHistoryAction( new MergeableHistoryAction( this, _list.Count, 0, items ) );
    }

    public void SetRange( IEnumerable<T> collection )
    {
      var items = collection.ToArray();
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, 0, _list.Count, items ) );
    }

    internal void CopyFrom( IEnumerable<T> collection )
    {
      var items = collection.ToArray();
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, 0, _list.Count, items, false ) );
    }

    public void InsertRange( int index, IEnumerable<T> collection )
    {
      var items = collection.ToArray();
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, index, 0, items ) );
    }

    public void RemoveRange( int index, int count )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, index, count, Array.Empty<T>() ) ) ;
    }

    public void Clear()
    {
      if ( 0 < _list.Count ) {
        this.History.RegisterHistoryAction( new SpliceHistoryAction( this, 0, _list.Count, Array.Empty<T>() ) ) ;
      }
    }

    public bool Contains( T item )
    {
      return _list.Contains( item );
    }

    public void CopyTo( T[] array, int arrayIndex )
    {
      _list.CopyTo( array, arrayIndex );
    }

    public bool Remove( T item )
    {
      var index = _list.IndexOf( item );
      if ( index < 0 ) return false;

      RemoveAt( index );
      return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _list.GetEnumerator();
    }

    private void OnAfterNewlyItemChanged( T[] oldValues, T[] newValues )
    {
      AfterNewlyItemChanged?.Invoke( this, new ItemChangedEventArgs<T>( oldValues, newValues ) );
    }
    private void OnAfterHistoricallyItemChanged( T[] oldValues, T[] newValues )
    {
      AfterHistoricallyItemChanged?.Invoke( this, new ItemChangedEventArgs<T>( oldValues, newValues ) );
    }

    class SpliceHistoryAction : HistoryActionBase
    {
      private readonly MementoList<T> _memento;
      protected int _startIndex;
      protected T[] _oldValues;
      protected T[] _newValues;

      public SpliceHistoryAction( MementoList<T> memento, int startIndex, int removeCount, T[] newValues, bool withEvent = true )
        : base( memento )
      {
        if ( removeCount < 0 ) throw new ArgumentOutOfRangeException( nameof( removeCount ) ) ;

        _memento = memento ;
        _startIndex = startIndex ;
        _oldValues = memento._list.GetRange( startIndex, removeCount ).ToArray() ;
        _newValues = newValues ;

        Redo( true, withEvent ) ;
      }

      public override void Undo()
      {
        Splice( _memento._list, _startIndex, _newValues.Length, _oldValues );

        _memento.OnAfterHistoricallyItemChanged( _newValues, _oldValues );
      }

      public override void Redo()
      {
        Redo( false, true ) ;
      }

      private void Redo( bool firstTime, bool withEvent )
      {
        Splice( _memento._list, _startIndex, _oldValues.Length, _newValues ) ;

        if ( withEvent ) {
          if ( firstTime ) _memento.OnAfterNewlyItemChanged( _oldValues, _newValues ) ;
          else _memento.OnAfterHistoricallyItemChanged( _oldValues, _newValues ) ;
        }
      }

      private static void Splice( List<T> list, int startIndex, int length, T[] newValues )
      {
        if ( 0 != length ) {
          list.RemoveRange( startIndex, length );
        }

        if ( list.Count == startIndex ) {
          list.AddRange( newValues );
        }
        else {
          list.InsertRange( startIndex, newValues );
        }
      }

      public override bool Merge( IHistoryAction action )
      {
        return false;
      }
    }

    class MergeableHistoryAction : SpliceHistoryAction
    {
      public MergeableHistoryAction( MementoList<T> memento, int startIndex, int removeCount, T[] newValues, bool withEvent = true )
        : base( memento, startIndex, removeCount, newValues, withEvent )
      {
      }

      public override bool Merge( IHistoryAction action )
      {
        var mergeableAction = action as MergeableHistoryAction;
        if ( null == mergeableAction ) return false;

        if ( mergeableAction._oldValues.Length > 0 &&
             mergeableAction._newValues.Length > 0 ) throw new InvalidOperationException();

        if ( mergeableAction._oldValues.Length > 0 ) {
          var removeCount = mergeableAction._oldValues.Length - _newValues.Length;
          if ( removeCount > 0 ) {
            _startIndex = mergeableAction._startIndex;
            _oldValues = JoinArrays( mergeableAction._oldValues.Take( removeCount ).ToArray(), _oldValues );
            _newValues = Array.Empty<T>();
          }
          else {
            _newValues = _newValues.Take( -removeCount ).ToArray();
          }
        }
        if ( mergeableAction._newValues.Length > 0 ) {
          _newValues = JoinArrays( _newValues, mergeableAction._newValues );
        }
        return true;
      }

      private static T[] JoinArrays( T[] array1, T[] array2 )
      {
        var result = new T[array1.Length + array2.Length];
        Array.Copy( array1, 0, result, 0, array1.Length );
        Array.Copy( array2, 0, result, array1.Length, array2.Length );
        return result;
      }
    }
  }



  /// <summary>
  /// HashSet&lt;T&gt;型に対する履歴化クラス。
  /// </summary>
  /// <typeparam name="T">値型</typeparam>
  public class MementoSet<T> : AbstractMemento, ICollection<T>, ISerializableList
  {
    private readonly HashSet<T> _set;

    public event EventHandler<ItemChangedEventArgs<T>> AfterNewlyItemChanged;
    public event EventHandler<ItemChangedEventArgs<T>> AfterHistoricallyItemChanged;
    public event EventHandler<ItemChangedEventArgs<T>> AfterItemChanged
    {
      add
      {
        AfterNewlyItemChanged += value ;
        AfterHistoricallyItemChanged += value ;
      }
      remove
      {
        AfterNewlyItemChanged -= value ;
        AfterHistoricallyItemChanged -= value ;
      }
    }

    public MementoSet( IMemorableObject owner ) : base( owner )
    {
      _set = new HashSet<T>();
    }
    public MementoSet( IMemorableObject owner, IEnumerable<T> collection ) : base( owner )
    {
      _set = new HashSet<T>( collection );
    }
    public MementoSet( IMemorableObject owner, IEqualityComparer<T> comparer ) : base( owner )
    {
      _set = new HashSet<T>( comparer );
    }
    public MementoSet( IMemorableObject owner, IEnumerable<T> collection, IEqualityComparer<T> comparer ) : base( owner )
    {
      _set = new HashSet<T>( collection, comparer );
    }
    
    Type ISerializableList.ItemType => typeof( T ) ;

    void ISerializableList.Add( object item )
    {
      Add( (T) item ) ;
    }

    public IEqualityComparer<T> Comparer { get { return _set.Comparer; } }

    public int Count => _set.Count ;

    public bool IsReadOnly => false ;

    void ICollection<T>.Add( T item ) => Add( item ) ;

    public bool Add( T item )
    {
      if ( true == _set.Contains( item ) ) return false;

      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, Array.Empty<T>(), new T[] { item } ) );
      return true;
    }

    public int AddRange( IEnumerable<T> collection )
    {
      var added = collection.Where( item => !_set.Contains( item ) ).ToArray();
      if ( 0 != added.Length ) {
        this.History.RegisterHistoryAction( new SpliceHistoryAction( this, Array.Empty<T>(), added ) );
      }
      return added.Length;
    }

    public void SetRange( IEnumerable<T> collection )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, _set.ToArray(), collection.ToArray() ) );
    }

    internal void CopyFrom( IEnumerable<T> collection )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, _set.ToArray(), collection.ToArray(), false ) );
    }

    public void Clear()
    {
      if ( 0 < _set.Count ) {
        this.History.RegisterHistoryAction( new SpliceHistoryAction( this, _set.ToArray(), Array.Empty<T>() ) );
      }
    }

    public bool Contains( T item )
    {
      return _set.Contains( item );
    }

    public void CopyTo( T[] array, int arrayIndex )
    {
      _set.CopyTo( array, arrayIndex );
    }

    public bool Remove( T item )
    {
      if ( false == _set.Contains( item ) ) return false;

      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, new T[] { item }, Array.Empty<T>() ) );
      return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _set.GetEnumerator();
    }

    private void OnAfterNewlyItemChanged( T[] oldValues, T[] newValues )
    {
      AfterNewlyItemChanged?.Invoke( this, new ItemChangedEventArgs<T>( oldValues, newValues ) );
    }
    private void OnAfterHistoricallyItemChanged( T[] oldValues, T[] newValues )
    {
      AfterHistoricallyItemChanged?.Invoke( this, new ItemChangedEventArgs<T>( oldValues, newValues ) );
    }

    class SpliceHistoryAction : HistoryActionBase
    {
      private readonly MementoSet<T> _memento;
      private T[] _oldValues;
      private T[] _newValues;

      public SpliceHistoryAction( MementoSet<T> memento, T[] oldValues, T[] newValues, bool withEvent = true )
        : base( memento )
      {
        _memento = memento;
        _oldValues = oldValues;
        _newValues = newValues;

        Redo( true, withEvent ) ;
      }

      public override void Undo()
      {
        Splice( _memento._set, _newValues, _oldValues );

        _memento.OnAfterHistoricallyItemChanged( _newValues, _oldValues );
      }

      public override void Redo()
      {
        Redo( false, true ) ;
      }

      private void Redo( bool firstTime, bool withEvent )
      {
        Splice( _memento._set, _oldValues, _newValues ) ;

        if ( withEvent ) {
          if ( firstTime ) _memento.OnAfterNewlyItemChanged( _oldValues, _newValues ) ;
          else _memento.OnAfterHistoricallyItemChanged( _oldValues, _newValues ) ;
        }
      }

      private static void Splice( HashSet<T> set, T[] oldValues, T[] newValues )
      {
        foreach ( var item in oldValues ) set.Remove( item );
        foreach ( var item in newValues ) set.Add( item );
      }

      public override bool Merge( IHistoryAction action )
      {
        var historyAction = action as SpliceHistoryAction;
        if ( null == historyAction ) return false;

        EraseCommon( ref _newValues, ref historyAction._oldValues );
        _oldValues = JoinArrays( _oldValues, historyAction._oldValues );
        _newValues = JoinArrays( _newValues, historyAction._newValues );
        return true;
      }

      private static void EraseCommon( ref T[] array1, ref T[] array2 )
      {
        if ( 0 == array1.Length || 0 == array2.Length ) return;

        var set1 = new HashSet<T>( array1 );
        var set2 = new HashSet<T>( array2 );
        set1.ExceptWith( array2 );
        set2.ExceptWith( array1 );
        array1 = set1.ToArray();
        array2 = set2.ToArray();
      }

      private static T[] JoinArrays( T[] array1, T[] array2 )
      {
        var result = new T[array1.Length + array2.Length];
        Array.Copy( array1, 0, result, 0, array1.Length );
        Array.Copy( array2, 0, result, array1.Length, array2.Length );
        return result;
      }
    }
  }



  /// <summary>
  /// HashSet&lt;T&gt;型に対する履歴化クラス。
  /// </summary>
  /// <typeparam name="TKey">キー型</typeparam>
  /// <typeparam name="TValue">値型</typeparam>
  public class MementoDictionary<TKey, TValue> : AbstractMemento, IDictionary<TKey, TValue>, ISerializableList
  {
    private readonly Dictionary<TKey, TValue> _dic;

    public event EventHandler<ItemChangedEventArgs<KeyValuePair<TKey, TValue>>> AfterNewlyItemChanged;
    public event EventHandler<ItemChangedEventArgs<KeyValuePair<TKey, TValue>>> AfterHistoricallyItemChanged;
    public event EventHandler<ItemChangedEventArgs<KeyValuePair<TKey, TValue>>> AfterItemChanged
    {
      add
      {
        AfterNewlyItemChanged += value ;
        AfterHistoricallyItemChanged += value ;
      }
      remove
      {
        AfterNewlyItemChanged -= value ;
        AfterHistoricallyItemChanged -= value ;
      }
    }

    public MementoDictionary( IMemorableObject owner ) : base( owner )
    {
      _dic = new Dictionary<TKey, TValue>();
    }
    public MementoDictionary( IMemorableObject owner, int capacity ) : base( owner )
    {
      _dic = new Dictionary<TKey, TValue>( capacity );
    }
    public MementoDictionary( IMemorableObject owner, IEqualityComparer<TKey> comparer ) : base( owner )
    {
      _dic = new Dictionary<TKey, TValue>( comparer );
    }
    public MementoDictionary( IMemorableObject owner, IDictionary<TKey, TValue> dictionary ) : base( owner )
    {
      _dic = new Dictionary<TKey, TValue>( dictionary );
    }
    public MementoDictionary( IMemorableObject owner, int capacity, IEqualityComparer<TKey> comparer ) : base( owner )
    {
      _dic = new Dictionary<TKey, TValue>( capacity, comparer );
    }
    public MementoDictionary( IMemorableObject owner, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer ) : base( owner )
    {
      _dic = new Dictionary<TKey, TValue>( dictionary, comparer );
    }

    Type ISerializableList.ItemType => typeof( KeyValuePair<TKey, TValue> ) ;

    void ISerializableList.Add( object item )
    {
      Add( (KeyValuePair<TKey, TValue>) item ) ;
    }

    public TValue this[TKey key]
    {
      get => _dic[key] ;
      set
      {
        TValue oldValue;
        if ( _dic.TryGetValue( key, out oldValue ) ) {
          this.History.RegisterHistoryAction( new SpliceHistoryAction( this,
                                                                       new[] { new KeyValuePair<TKey, TValue>( key, oldValue ) },
                                                                       new[] { new KeyValuePair<TKey, TValue>( key, value ) }
                                                                     ) );
        }
        else {
          this.History.RegisterHistoryAction( new SpliceHistoryAction( this,
                                                                       new KeyValuePair<TKey, TValue>[0],
                                                                       new[] { new KeyValuePair<TKey, TValue>( key, value ) }
                                                                     ) );
        }
      }
    }

    public IEqualityComparer<TKey> Comparer => _dic.Comparer ;

    public int Count => _dic.Count ;

    public bool IsReadOnly => false ;

    public ICollection<TKey> Keys => _dic.Keys ;

    public ICollection<TValue> Values => _dic.Values ;

    public void Add( TKey key, TValue value )
    {
      TValue oldValue;
      if ( _dic.TryGetValue( key, out oldValue ) ) {
        throw new InvalidOperationException();
      }
      else {
        this.History.RegisterHistoryAction( new SpliceHistoryAction( this,
                                                                     new KeyValuePair<TKey, TValue>[0],
                                                                     new[] { new KeyValuePair<TKey, TValue>( key, value ) }
                                                                   ) );
      }
    }

    public void Add( KeyValuePair<TKey, TValue> item ) => Add( item.Key, item.Value ) ;

    public void Clear()
    {
      if ( 0 < _dic.Count ) {
        this.History.RegisterHistoryAction( new SpliceHistoryAction( this, _dic.ToArray(), new KeyValuePair<TKey, TValue>[0] ) );
      }
    }

    public void SetRange( IEnumerable<KeyValuePair<TKey, TValue> > collection )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, _dic.ToArray(), collection.ToArray() ) );
    }

    internal void CopyFrom( IEnumerable<KeyValuePair<TKey, TValue> > collection )
    {
      this.History.RegisterHistoryAction( new SpliceHistoryAction( this, _dic.ToArray(), collection.ToArray(), false ) );
    }

    public bool Contains( KeyValuePair<TKey, TValue> item )
    {
      if ( !_dic.TryGetValue( item.Key, out var value ) ) return false;
      return MementoEqualityComparer<TValue>.Equals( item.Value, value );
    }

    public bool ContainsKey( TKey key ) => _dic.ContainsKey( key ) ;

    public bool ContainsValue( TValue value ) => _dic.ContainsValue( value ) ;

    public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
    {
      ((IDictionary<TKey, TValue>)_dic).CopyTo( array, arrayIndex );
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dic.GetEnumerator() ;

    public bool Remove( TKey key )
    {
      TValue value;
      if ( !_dic.TryGetValue( key, out value ) ) return false;

      this.History.RegisterHistoryAction( new SpliceHistoryAction( this,
                                                                   new[] { new KeyValuePair<TKey, TValue>( key, value ) },
                                                                   new KeyValuePair<TKey, TValue>[0]
                                                                 ) );
      return true;
    }

    public bool Remove( KeyValuePair<TKey, TValue> item )
    {
      TValue value;
      if ( !_dic.TryGetValue( item.Key, out value ) ) return false;
      if ( !MementoEqualityComparer<TValue>.Equals( item.Value, value ) ) return false;

      this.History.RegisterHistoryAction( new SpliceHistoryAction( this,
                                                                   new[] { item },
                                                                   new KeyValuePair<TKey, TValue>[0]
                                                                 ) );
      return true;
    }

    public bool TryGetValue( TKey key, out TValue value )
    {
      return _dic.TryGetValue( key, out value );
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _dic.GetEnumerator();
    }

    private void OnAfterNewlyItemChanged( KeyValuePair<TKey, TValue>[] oldValues, KeyValuePair<TKey, TValue>[] newValues )
    {
      AfterNewlyItemChanged?.Invoke( this, new ItemChangedEventArgs<KeyValuePair<TKey, TValue>>( oldValues, newValues ) ) ;
    }

    private void OnAfterHistoricallyItemChanged(KeyValuePair<TKey, TValue>[] oldValues, KeyValuePair<TKey, TValue>[] newValues )
    {
      AfterHistoricallyItemChanged?.Invoke( this, new ItemChangedEventArgs<KeyValuePair<TKey, TValue>>( oldValues, newValues ) );
    }

    class SpliceHistoryAction : HistoryActionBase
    {
      private readonly MementoDictionary<TKey, TValue> _memento;
      private KeyValuePair<TKey, TValue>[] _oldValues;
      private KeyValuePair<TKey, TValue>[] _newValues;

      public SpliceHistoryAction( MementoDictionary<TKey, TValue> memento, KeyValuePair<TKey, TValue>[] oldValues, KeyValuePair<TKey, TValue>[] newValues, bool withEvent = true )
        : base( memento )
      {
        _memento = memento ;
        _oldValues = oldValues ;
        _newValues = newValues ;

        Redo( true, withEvent ) ;
      }

      public override void Undo()
      {
        Splice( _newValues, _oldValues, false, true ) ;
      }

      public override void Redo()
      {
        Redo( false, true ) ;
      }

      private void Redo( bool firstTime, bool withEvent )
      {
        Splice( _oldValues, _newValues, firstTime, withEvent ) ;
      }

      private void Splice( KeyValuePair<TKey, TValue>[] oldValues, KeyValuePair<TKey, TValue>[] newValues, bool isFirstTime, bool withEvent )
      {
        var dic = _memento._dic ;
        foreach ( var item in oldValues ) dic.Remove( item.Key ) ;
        foreach ( var item in newValues ) dic.Add( item.Key, item.Value ) ;

        if ( withEvent ) {
          if ( isFirstTime ) _memento.OnAfterNewlyItemChanged( oldValues, newValues ) ;
          else _memento.OnAfterHistoricallyItemChanged( oldValues, newValues ) ;
        }
      }

      public override bool Merge( IHistoryAction action )
      {
        var historyAction = action as SpliceHistoryAction;
        if ( null == historyAction ) return false;

        EraseCommon( ref _newValues, ref historyAction._oldValues );
        _oldValues = JoinArrays( _oldValues, historyAction._oldValues );
        _newValues = JoinArrays( _newValues, historyAction._newValues );
        return true;
      }

      private static void EraseCommon( ref KeyValuePair<TKey, TValue>[] array1, ref KeyValuePair<TKey, TValue>[] array2 )
      {
        if ( 0 == array1.Length || 0 == array2.Length ) return;

        var set1 = new HashSet<KeyValuePair<TKey, TValue>>( array1 );
        var set2 = new HashSet<KeyValuePair<TKey, TValue>>( array2 );
        set1.ExceptWith( array2 );
        set2.ExceptWith( array1 );
        array1 = set1.ToArray();
        array2 = set2.ToArray();
      }

      private static KeyValuePair<TKey, TValue>[] JoinArrays( KeyValuePair<TKey, TValue>[] array1, KeyValuePair<TKey, TValue>[] array2 )
      {
        var result = new KeyValuePair<TKey, TValue>[array1.Length + array2.Length];
        Array.Copy( array1, 0, result, 0, array1.Length );
        Array.Copy( array2, 0, result, array1.Length, array2.Length );
        return result;
      }
    }
  }
}
