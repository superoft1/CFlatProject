using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// ドキュメント履歴クラス。
  /// </summary>
  public class History
  {
    /// <summary>
    /// 無限に履歴操作を行なえることを示す履歴回数です。
    /// </summary>
    public const int HISTORY_LEVEL_INFINITE = -1;

    public int _historyLevel;

    /// <summary>
    /// 既定の履歴操作回数を取得または設定します。
    /// </summary>
    public static int DefaultHistoryLevel { get ; set ; } = HISTORY_LEVEL_INFINITE ;

    /// <summary>
    /// 履歴操作回数を取得または設定します。履歴操作回数を減らした場合、それを超える既存の履歴は全て解放されます。
    /// </summary>
    public int HistoryLevel
    {
      get { return _historyLevel; }
      set
      {
        _historyLevel = value;
        TrimHistories();
      }
    }

    /// <summary>
    /// 履歴を元に戻したりやり直したりした際のイベントです。
    /// </summary>
    public event EventHandler HistoryMoved;

    /// <summary>
    /// 履歴のUndo可否/Redo可否が変わった可能性がある際のイベントです。
    /// </summary>
    public event EventHandler HistoryStateChanged;

    private readonly Document _doc;

    private int _nextIndex = 0;
    private int _currentIndex = 0;
    private readonly List<int> _historyIndices = new List<int>();
    private readonly Dictionary<int, Dictionary<IMemento, List<IHistoryAction>>> _actions = new Dictionary<int, Dictionary<IMemento, List<IHistoryAction>>>();

    private bool RegisterHistory { get; set; } = true;

    private object LockObject { get { return _actions; } }

    private History( Document doc )
    {
      _doc = doc;
      _historyLevel = DefaultHistoryLevel;
    }

    internal static History Create( Document doc )
    {
      return new History( doc );
    }

    internal IDisposable SuppressRegister()
    {
      return new SuppressRegisterHistory( this );
    }

    private class SuppressRegisterHistory : IDisposable
    {
      private readonly History _history ;

      public SuppressRegisterHistory( History history )
      {
        _history = history;
        _history.RegisterHistory = false;
      }

      ~SuppressRegisterHistory()
      {
        throw new InvalidOperationException( "SuppressRegisterHistory is disposed without Dispose()." );
      }

      public void Dispose()
      {
        _history.RegisterHistory = true;
        GC.SuppressFinalize( this );
      }
    }

    /// <summary>
    /// 変更を通知します。
    /// </summary>
    /// <param name="historyAction">変更処理</param>
    internal void RegisterHistoryAction( IHistoryAction historyAction )
    {
      var memento = historyAction.Memento;
      if ( !ReferenceEquals( memento.History, this ) ) {
        throw new InvalidOperationException();
      }

      if ( !RegisterHistory ) {
        return;
      }

      bool changeStarted = false;
      lock ( LockObject ) {
        changeStarted = RegisterIndex();
        AddHistoryAction( memento, historyAction );
      }

      if ( changeStarted ) {
        OnHistoryStateChanged( EventArgs.Empty );
      }
    }

    private void AddHistoryAction( IMemento memento, IHistoryAction historyAction )
    {
      var mementoList = _actions[_nextIndex];

      if ( false == mementoList.TryGetValue( memento, out List<IHistoryAction> list ) ) {
        list = new List<IHistoryAction>();
        mementoList.Add( memento, list );
      }

      var lastAction = list.LastOrDefault();
      if ( null != lastAction ) {
        if ( lastAction.Merge( historyAction ) ) {
          // 既存の変更処理とマージ成功
          return;
        }
      }

      // マージできなかったため、処理を追加
      list.Add( historyAction );
    }

    /// <summary>
    /// 未確定の変更を確定します。
    /// </summary>
    /// <returns>加わった変更がなければ false 。</returns>
    public bool Commit()
    {
      lock ( LockObject ) {
        if ( !HasChange ) return false;

        EraseRedoBuffers();

        _currentIndex = ++_nextIndex;

        TrimHistories();
      }

#if DEBUG
      Debug.Log( "History: Commit()" );
#endif

      OnHistoryStateChanged( EventArgs.Empty );

      return true;
    }

    /// <summary>
    /// 未確定の変更を全て元に戻します。
    /// </summary>
    /// <returns>加わった変更がなければ false 。</returns>
    public bool Cancel()
    {
      lock ( LockObject ) {
        if ( !HasChange ) return false;

        UndoAt( _nextIndex );
        RemoveAt( _nextIndex );
      }

      OnHistoryStateChanged( EventArgs.Empty );

      return true;
    }

    /// <summary>
    /// 未確定の変更があるかどうかを取得します。
    /// </summary>
    public bool HasChange
    {
      get
      {
        lock ( LockObject ) {
          return _actions.ContainsKey( _nextIndex );
        }
      }
    }

    /// <summary>
    /// 履歴を元に戻せるかどうかを取得します。
    /// </summary>
    public bool CanUndo
    {
      get
      {
        lock ( LockObject ) {
          if ( HasChange ) return false;
          if ( 0 == _historyIndices.Count ) return false;
          if ( _currentIndex == _historyIndices[0] ) return false;

          return true;
        }
      }
    }

    /// <summary>
    /// 履歴をやり直せるかどうかを取得します。
    /// </summary>
    public bool CanRedo
    {
      get
      {
        lock ( LockObject ) {
          if ( HasChange ) return false;
          if ( _currentIndex == _nextIndex ) return false;

          return true;
        }
      }
    }
    
    public bool IsUndoing { get ; private set ; }
    public bool IsRedoing { get ; private set ; }

    /// <summary>
    /// 履歴を元に戻します。
    /// </summary>
    /// <param name="count">元に戻す回数。</param>
    /// <returns>実際にやり直した回数。</returns>
    public int Back( int count )
    {
      if ( count < 0 ) return -Go( -count );
      if ( count == 0 ) return 0;

      int doneCount = 0;

      lock ( LockObject ) {
        if ( ! CanUndo ) return 0;

        IsUndoing = true ;
        try {
          int index ;
          if ( 0 < _historyIndices.Count && _historyIndices[ _historyIndices.Count - 1 ] < _currentIndex ) {
            index = _historyIndices.Count ;
          }
          else {
            index = _historyIndices.IndexOf( _currentIndex ) ;
            if ( index < 0 ) return 0 ;
          }

          for ( ; ; ) {
            --index ;
            if ( index < 0 ) break ;

            _currentIndex = _historyIndices[ index ] ;
            UndoAt( _currentIndex ) ;
            ++doneCount ;
            if ( doneCount == count ) break ;
          }
        }
        finally {
          IsUndoing = false ;
        }
      }

#if DEBUG
      Debug.Log( $"History: Back({doneCount})" );
#endif

      OnHistoryStateChanged( EventArgs.Empty );
      OnHistoryMoved( EventArgs.Empty );

      return doneCount;
    }

    /// <summary>
    /// 履歴をやり直します。
    /// </summary>
    /// <param name="count">やり直す回数。</param>
    /// <returns>実際にやり直した回数。</returns>
    public int Go( int count )
    {
      if ( count < 0 ) return -Back( -count );
      if ( count == 0 ) return 0;

      int doneCount = 0;
      lock ( LockObject ) {
        if ( ! CanRedo ) return 0;

        IsRedoing = true ;
        try {
          int index = _historyIndices.IndexOf( _currentIndex ) ;
          for ( ; ; ) {
            RedoAt( _currentIndex ) ;
            ++index ;
            ++doneCount ;
            if ( index < _historyIndices.Count ) {
              _currentIndex = _historyIndices[ index ] ;
              if ( doneCount == count ) break ;
            }
            else {
              _currentIndex = _nextIndex ;
              break ;
            }
          }
        }
        finally {
          IsRedoing = false ;
        }
      }
      
#if DEBUG
      Debug.Log( $"History: Go({doneCount})" );
#endif

      OnHistoryStateChanged( EventArgs.Empty );
      OnHistoryMoved( EventArgs.Empty );

      return doneCount;
    }

    private bool RegisterIndex()
    {
      if ( false == HasChange ) {
        // 新たな履歴インデックスを追加
        _historyIndices.Add( _nextIndex );
        _actions.Add( _nextIndex, new Dictionary<IMemento, List<IHistoryAction>>() );
        return true;
      }

      return false;
    }

    private void TrimHistories()
    {
      if ( 0 <= _historyLevel ) {
        int eraseCount = _historyIndices.Count - _historyLevel;
        if ( 0 < eraseCount ) {
          for ( int i = 0 ; i < eraseCount ; ++i ) {
            _actions.Remove( _historyIndices[i] );
          }
          _historyIndices.RemoveRange( 0, eraseCount );
        }
      }
    }

    private void UndoAt( int index )
    {
      Dictionary<IMemento, List<IHistoryAction>> dic;
      if ( _actions.TryGetValue( index, out dic ) ) {
        foreach ( var pair in dic ) {
          foreach ( var action in Enumerable.Reverse( pair.Value ) ) {
            action.Undo();
          }
        }
      }
    }

    private void RedoAt( int index )
    {
      Dictionary<IMemento, List<IHistoryAction>> dic;
      if ( _actions.TryGetValue( index, out dic ) ) {
        foreach ( var pair in dic ) {
          foreach ( var action in pair.Value ) {
            action.Redo();
          }
        }
      }
    }

    private void EraseRedoBuffers()
    {
      for ( int index = _currentIndex ; index < _nextIndex ; ++index ) {
        RemoveAt( index );
      }
    }

    private void RemoveAt( int index )
    {
      _historyIndices.Remove( index );
      _actions.Remove( index );
    }

    private void OnHistoryStateChanged( EventArgs e )
    {
      HistoryStateChanged?.Invoke( this, e );
    }

    private void OnHistoryMoved( EventArgs e )
    {
      HistoryMoved?.Invoke( this, e );
    }

  }
}
