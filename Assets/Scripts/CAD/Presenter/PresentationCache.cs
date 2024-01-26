using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Presenter
{
  enum PresentationType
  {
    Raise,
    Update,
    TransformUpdate,
    Destroy,
  }

  readonly struct PresentationCacheItem
  {
    public IElement Element { get; }
    public PresentationType PresentationType { get; }

    public PresentationCacheItem( IElement elm, PresentationType type )
    {
      Element = elm;
      PresentationType = type;
    }
  }

  /// <summary>
  /// Presenterの各種操作をキャッシュして、Update時に一度だけ呼ぶような仕組み
  /// </summary>
  class PresentationCache
  {
    private List<PresentationCacheItem> _list = new List<PresentationCacheItem>();
    private readonly Dictionary<IElement, PresentationType> _dicTypes = new Dictionary<IElement, PresentationType>();

    /// <summary>
    /// Raise処理をキャッシュ
    /// </summary>
    /// <param name="element"></param>
    public void AddRaise( IElement element )
    {
      PresentationType type;
      if ( _dicTypes.TryGetValue( element, out type ) ) {
        switch ( type ) {
          case PresentationType.Raise:
            // Raiseの重複があってはならない
            throw new InvalidOperationException();

          case PresentationType.Update:
            // UpdateからのRaiseは無視
            break;

          case PresentationType.TransformUpdate:
            // TransformUpdateからのRaiseは無視
            break;

          case PresentationType.Destroy:
            // DestroyからのRaiseはUpdateに変換
            UpdateCache( element, PresentationType.Update );
            break;
        }
      }
      else {
        AddCache( element, PresentationType.Raise );
      }
    }

    /// <summary>
    /// Update処理をキャッシュ
    /// </summary>
    /// <param name="element"></param>
    public void AddUpdate( IElement element )
    {
      PresentationType type;
      if ( _dicTypes.TryGetValue( element, out type ) ) {
        if ( PresentationType.TransformUpdate == type ) {
          // TransformUpdateからのUpdateはUpdateのみ使用
          UpdateCache( element, PresentationType.Update );
        }
        else {
          // Raise→Update、Update→Update、Destroy→Updateはいずれも何もしなくてよい
          return ;
        }
      }
      else {
        AddCache( element, PresentationType.Update );
      }
    }

    /// <summary>
    /// 座標系のみのUpdate処理をキャッシュ
    /// </summary>
    /// <param name="element"></param>
    public void AddTransformUpdate( IElement element )
    {
      PresentationType type;
      if ( _dicTypes.TryGetValue( element, out type ) ) {
        // Raise→TransformUpdate、Update→TransformUpdate、TransformUpdate→TransformUpdate、Destroy→TransformUpdateはいずれも何もしなくてよい
        return;
      }
      else {
        AddCache( element, PresentationType.TransformUpdate );
      }
    }

    /// <summary>
    /// Destroy処理をキャッシュ
    /// </summary>
    /// <param name="element"></param>
    public void AddDestroy( IElement element )
    {
      PresentationType type;
      if ( _dicTypes.TryGetValue( element, out type ) ) {
        switch ( type ) {
          case PresentationType.Raise:
            // RaiseからのDestroyは何もしない＝削除
            RemoveCache( element );
            break;

          case PresentationType.Update:
          case PresentationType.TransformUpdate:
            // Update・TransformUpdateからのDestroyはDestroyに変換
            UpdateCache( element, PresentationType.Destroy );
            break;

          case PresentationType.Destroy:
            // Destroyの重複があってはならない
            throw new InvalidOperationException();
        }
      }
      else {
        AddCache( element, PresentationType.Destroy );
      }
    }

    /// <summary>
    /// キャッシュした処理を全て実行
    /// </summary>
    /// <returns></returns>
    public IEnumerable<PresentationCacheItem> Flush()
    {
      while ( 0 < _list.Count ) {
        var orgList = _list;
        _list = new List<PresentationCacheItem>();
        _dicTypes.Clear();
        // 不要なものを全てdestroy
        foreach ( var item in orgList.Where( x => PresentationType.Destroy == x.PresentationType ) ) {
          yield return item;
        }
        // 次に必要なものをraise & update
        foreach ( var item in orgList.Where( x => PresentationType.Raise == x.PresentationType ) ) {
          yield return item;
          yield return new PresentationCacheItem( item.Element, PresentationType.Update );
        }
        // 次に通常のupdate
        foreach ( var item in orgList.Where( x => PresentationType.Update == x.PresentationType ) ) {
          yield return item ;
        }
        // 最後に座標だけのupdateを実行
        foreach ( var item in orgList.Where( x => PresentationType.TransformUpdate == x.PresentationType ) ) {
          yield return item ;
        }
      }
    }

    public bool HasCache()
    {
      return _list.Any();
    }
    
    private void AddCache( IElement element, PresentationType type )
    {
      _list.Add( new PresentationCacheItem( element, type ) );
      _dicTypes.Add( element, type );
    }

    private void UpdateCache( IElement element, PresentationType type )
    {
      var index = _list.FindIndex( item => item.Element == element );
      if ( index < 0 ) throw new InvalidOperationException();

      _list[index] = new PresentationCacheItem( element, type );
      _dicTypes[element] = type;
    }

    private void RemoveCache( IElement element )
    {
      var index = _list.FindIndex( item => item.Element == element );
      if ( index < 0 ) throw new InvalidOperationException();

      _list.RemoveAt( index );
      _dicTypes.Remove( element );
    }
  }
}
