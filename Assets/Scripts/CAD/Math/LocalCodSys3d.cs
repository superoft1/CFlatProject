using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
  /// <summary>
  /// ローカル座標系を示す構造体。
  /// </summary>
  public class LocalCodSys3d
  {
    /// <summary>
    /// 原点基準・回転・反転なしのローカル座標系を取得します。
    /// </summary>
    public static LocalCodSys3d Identity { get; }

    static LocalCodSys3d()
    {
      Identity = new LocalCodSys3d();
    }

    /// <summary>
    /// ローカル座標系の原点を取得します。
    /// </summary>
    public Vector3d Origin { get; }

    /// <summary>
    /// ローカル座標系のクオータニオンを取得します。
    /// </summary>
    public Quaternion Rotation
    {
      get
      {
        if ( IsMirrorType ) {
          return Quaternion.LookRotation( -(Vector3) DirectionZ, (Vector3) DirectionY ) ;
        }
        else {
          return Quaternion.LookRotation( (Vector3) DirectionZ, (Vector3) DirectionY ) ;
        }
      }
    }

    /// <summary>
    /// ローカル座標系のスケーリングを取得します。
    /// </summary>
    public Vector3d Scale
    {
      get
      {
        if ( IsMirrorType ) {
          return new Vector3d( 1, 1, -1 ) ;
        }
        else {
          return new Vector3d( 1, 1, 1 ) ;
        }
      }
    }

    public bool IsMirrorType => ( Vector3d.Dot( Vector3d.Cross( DirectionX, DirectionY ), DirectionZ ) < 0 ) ;

    /// <summary>
    /// ローカル座標系のX軸方向を取得します。
    /// </summary>
    public Vector3d DirectionX { get; }

    /// <summary>
    /// ローカル座標系のY軸方向を取得します。
    /// </summary>
    public Vector3d DirectionY { get; }

    /// <summary>
    /// ローカル座標系のZ軸方向を取得します。
    /// </summary>
    public Vector3d DirectionZ { get; }

    public LocalCodSys3d GetMirroredCodSysByX() => new LocalCodSys3d( Origin, -DirectionX, DirectionY, DirectionZ, false ) ;
    public LocalCodSys3d GetMirroredCodSysByY() => new LocalCodSys3d( Origin, DirectionX, -DirectionY, DirectionZ, false ) ;
    public LocalCodSys3d GetMirroredCodSysByZ() => new LocalCodSys3d( Origin, DirectionX, DirectionY, -DirectionZ, false ) ;

    /// <summary>
    /// 親座標系の原点をローカル座標の原点とし、親座標系のX軸・Y軸・Z軸を各ローカル軸のX軸・Y軸・Z軸とする座標系を構築します。
    /// </summary>
    public LocalCodSys3d()
      : this( Vector3d.zero )
    {
    }

    /// <summary>
    /// 指定した原点を用いて座標系を構築します。
    /// </summary>
    /// <param name="o">原点</param>
    public LocalCodSys3d( in Vector3d o )
    {
      Origin = o;
      DirectionX = Vector3d.right;
      DirectionY = Vector3d.up;
      DirectionZ = Vector3d.forward;
    }


//    /// <summary>
//    /// 指定した原点と回転を用いて座標系を構築します。
//    /// </summary>
//    /// <param name="o">原点</param>
//    /// <param name="q">回転</param>
//    public LocalCodSys3d( in Vector3d o, in Quaternion q )
//    {
//      Origin = o;
//      DirectionX = new Vector3d( q * new Vector3( 1, 0, 0 ) );
//      DirectionY = new Vector3d( q * new Vector3( 0, 1, 0 ) );
//      DirectionZ = new Vector3d( q * new Vector3( 0, 0, 1 ) );
//    }

    /// <summary>
    /// 指定した原点・回転・ミラーモードを用いて座標系を構築します。
    /// </summary>
    /// <param name="o">原点</param>
    /// <param name="q">回転</param>
    /// <param name="mirrorMode">Z軸ミラーモード</param>
    public LocalCodSys3d( in Vector3d o, in Quaternion q, bool mirrorMode )
    {
      Origin = o;
      DirectionX = new Vector3d( q * new Vector3( 1f, 0f, 0f ) );
      DirectionY = new Vector3d( q * new Vector3( 0f, 1f, 0f ) );
      DirectionZ = new Vector3d( q * new Vector3( 0f, 0f, ( mirrorMode ? -1f : +1f ) ) ) ;
    }

    /// <summary>
    /// 指定した原点と軸方向を用いて座標系を構築します (全ての軸が正しく直交している前提)。
    /// </summary>
    /// <param name="o">原点</param>
    /// <param name="x">X軸</param>
    /// <param name="y">Y軸</param>
    /// <param name="z">Z軸</param>
    /// <param name="additionalMirrorMode">追加のZ軸反転</param>
    private LocalCodSys3d( in Vector3d o, in Vector3d x, in Vector3d y, in Vector3d z, bool additionalMirrorMode )
    {
      Origin = o ;
      DirectionX = x ;
      DirectionY = y ;
      if ( additionalMirrorMode ) {
        DirectionZ = -z ;
      }
      else {
        DirectionZ = z ;
      }
    }

    /// <summary>
    /// 指定した原点と軸方向を用いて座標系を構築します。
    /// <para>（軸にゼロベクトルを指定した場合、その軸は他の軸から自動計算される。また各軸が直交していない場合、X軸→Y軸→Z軸を優先順位として直交するように修正される）</para>
    /// </summary>
    /// <param name="o">原点</param>
    /// <param name="x">X軸</param>
    /// <param name="y">Y軸</param>
    /// <param name="z">Z軸</param>
    public LocalCodSys3d( in Vector3d o, Vector3d x, Vector3d y, Vector3d z )
    {
      // まずはnormalize
      x.Normalize();
      y.Normalize();
      z.Normalize();

      if ( Vector3d.zero == x ) {
        // xがゼロ
        if ( Vector3d.zero == y ) {
          // x・yがゼロ
          if ( Vector3d.zero == z ) {
            // x・y・zがゼロなら親座標系と一致でよい
            x = Vector3d.right;
            y = Vector3d.up;
            z = Vector3d.forward;
          }
          else {
            // zのみゼロでない
            CreateOtherDirections( z, out x, out y );
          }
        }
        else {
          // xがゼロ、yがゼロでない
          CreateOrthoDirection( y, ref z, out x );
        }
      }
      else {
        // xがゼロでない
        if ( Vector3d.zero == y ) {
          // xがゼロでない、yがゼロ
          CreateOrthoDirection( x, ref z, out y );
          y = -y; // zをdir1に与えているため、座標系が反転してしまっている
        }
        else {
          // x・yがゼロでない
          CreateOrthoDirection( x, ref y, out var z2 );
          if ( Vector3d.Dot( z, z2 ) < 0 ) {
            // 反転モード
            z = -z2 ;
          }
          else {
            // 非反転モード
            z = z2 ;
          }
        }
      }

      Origin = o;
      DirectionX = x;
      DirectionY = y;
      DirectionZ = z;
    }

    /// <summary>
    /// 指定した原点と軸方向を用いて座標系を構築します。
    /// </summary>
    /// <param name="newOrigin">原点</param>
    /// <param name="directions">新しいXYZ軸を持つ原点</param>
    public LocalCodSys3d( in Vector3d newOrigin, LocalCodSys3d directions )
    {
      Origin = newOrigin;
      DirectionX = directions.DirectionX;
      DirectionY = directions.DirectionY;
      DirectionZ = directions.DirectionZ;
    }


    /// <summary>
    /// ローカル座標系上の座標をグローバル座標系上の座標に変換します。
    /// </summary>
    /// <param name="localPos">ローカル座標系上の座標</param>
    /// <returns>グローバル座標系上の座標</returns>
    public Vector3d GlobalizePoint( in Vector3d localPos )
    {
      return GlobalizeVector( localPos ) + Origin;
    }
    /// <summary>
    /// グローバル座標系上の座標をローカル座標系上の座標に変換します。
    /// </summary>
    /// <param name="globalPos">グローバル座標系上の座標</param>
    /// <returns>ローカル座標系上の座標</returns>
    public Vector3d LocalizePoint( in Vector3d globalPos )
    {
      return LocalizeVector( globalPos - Origin );
    }

    /// <summary>
    /// ローカル座標系上のベクトルをグローバル座標系上のベクトルに変換します。
    /// </summary>
    /// <param name="localVec">ローカル座標系上のベクトル</param>
    /// <returns>グローバル座標系上のベクトル</returns>
    public Vector3d GlobalizeVector( in Vector3d localVec )
    {
      return localVec.x * DirectionX + localVec.y * DirectionY + localVec.z * DirectionZ;
    }
    /// <summary>
    /// グローバル座標系上のベクトルをローカル座標系上のベクトルに変換します。
    /// </summary>
    /// <param name="globalVec">グローバル座標系上のベクトル</param>
    /// <returns>ローカル座標系上のベクトル</returns>
    public Vector3d LocalizeVector( in Vector3d globalVec )
    {
      var x = Vector3d.Dot( DirectionX, globalVec );
      var y = Vector3d.Dot( DirectionY, globalVec );
      var z = Vector3d.Dot( DirectionZ, globalVec );
      return new Vector3d( x, y, z );
    }

    /// <summary>
    /// ローカル座標系上で表現されるローカル座標系をグローバル座標系上で表現されるローカル座標系に変換します。
    /// </summary>
    /// <param name="localCodSys">ローカル座標系上で表現されるローカル座標系</param>
    /// <returns>グローバル座標系上で表現されるローカル座標系</returns>
    public LocalCodSys3d GlobalizeCodSys( LocalCodSys3d localCodSys )
    {
      return new LocalCodSys3d( GlobalizePoint( localCodSys.Origin ),
                                GlobalizeVector( localCodSys.DirectionX ),
                                GlobalizeVector( localCodSys.DirectionY ),
                                GlobalizeVector( localCodSys.DirectionZ ),
                                false );
    }

    /// <summary>
    /// ローカル座標系上で表現されるバウンディングボックスをグローバル座標系上で表現されるバウンディングボックスに変換します。
    /// </summary>
    /// <param name="bounds">ローカル座標系上で表現されるバウンディングボックス</param>
    /// <returns>グローバル座標系上で表現されるバウンディングボックス</returns>
    public Bounds GlobalizeBounds( in Bounds bounds )
    {
      var globalCenter = GlobalizePoint( bounds.center );

      var size = bounds.size;
      var globalSizeX = GlobalizeVector( new Vector3d( size.x, 0, 0 ) );
      var globalSizeY = GlobalizeVector( new Vector3d( 0, size.y, 0 ) );
      var globalSizeZ = GlobalizeVector( new Vector3d( 0, 0, size.z ) );

      var x = (float)(Math.Abs( globalSizeX.x ) + Math.Abs( globalSizeY.x ) + Math.Abs( globalSizeZ.x ));
      var y = (float)(Math.Abs( globalSizeX.y ) + Math.Abs( globalSizeY.y ) + Math.Abs( globalSizeZ.y ));
      var z = (float)(Math.Abs( globalSizeX.z ) + Math.Abs( globalSizeY.z ) + Math.Abs( globalSizeZ.z ));

      return new Bounds( (Vector3)globalCenter, new Vector3( x, y, z ) );
    }

    /// <summary>
    /// グローバル座標系上で表現されるローカル座標系をローカル座標系上で表現されるローカル座標系に変換します。
    /// </summary>
    /// <param name="globalCodSys">グローバル座標系上で表現されるローカル座標系</param>
    /// <returns>ローカル座標系上で表現されるローカル座標系</returns>
    public LocalCodSys3d LocalizeCodSys( LocalCodSys3d globalCodSys )
    {
      return new LocalCodSys3d( LocalizePoint( globalCodSys.Origin ),
                                LocalizeVector( globalCodSys.DirectionX ),
                                LocalizeVector( globalCodSys.DirectionY ),
                                LocalizeVector( globalCodSys.DirectionZ ),
                                false );
    }


    /// <summary>
    /// 指定ベクトルに対する直交ベクトルと、さらにそれとも直交するベクトルを生成する。
    /// </summary>
    /// <param name="baseDir">指定ベクトル</param>
    /// <param name="dir1"><paramref name="baseDir"/>と直交するベクトル</param>
    /// <param name="dir2"><paramref name="baseDir"/>とも<paramref name="dir1"/>とも直交するベクトル</param>
    public static void CreateOtherDirections( in Vector3d baseDir, out Vector3d dir1, out Vector3d dir2 )
    {
      var cosx = Math.Abs( Vector3d.Dot( baseDir, Vector3d.right ) );
      var cosy = Math.Abs( Vector3d.Dot( baseDir, Vector3d.up ) );
      var cosz = Math.Abs( Vector3d.Dot( baseDir, Vector3d.forward ) );
      if ( cosx < cosy ) {
        if ( cosx < cosz ) {
          // xが最も直交
          dir1 = Vector3d.right;
        }
        else {
          // zが最も直交
          dir1 = Vector3d.forward;
        }
      }
      else {
        if ( cosy < cosz ) {
          // yが最も直交
          dir1 = Vector3d.up;
        }
        else {
          // zが最も直交
          dir1 = Vector3d.forward;
        }
      }
      CreateOrthoDirection( baseDir, ref dir1, out dir2 );
    }

    /// <summary>
    /// 指定ベクトルと参照ベクトルを使用して、指定するベクトルに対する直交ベクトルと、さらにそれとも直交するベクトルを生成する。
    /// </summary>
    /// <param name="baseDir">指定ベクトル</param>
    /// <param name="dir1">[in] <paramref name="baseDir"/>と直交させたいベクトル／[out] 直交するよう修正したベクトル</param>
    /// <param name="dir2"><paramref name="baseDir"/>とも<paramref name="dir1"/>とも直交するベクトル</param>
    private static void CreateOrthoDirection( in Vector3d baseDir, ref Vector3d dir1, out Vector3d dir2 )
    {
      if ( baseDir.sqrMagnitude < Vector3d.kEpsilon * Vector3d.kEpsilon ) {
        throw new InvalidOperationException();
      }

      dir2 = Vector3d.Cross( baseDir, dir1 );
      if ( dir2.sqrMagnitude < Vector3d.kEpsilon * Vector3d.kEpsilon ) {
        // 平行に近い場合
        CreateOtherDirections( baseDir, out dir1, out dir2 );
      }
      else {
        dir2.Normalize();
        dir1 = Vector3d.Cross( dir2, baseDir );
      }
    }

    /// <summary>
    /// 原点を指定量だけ移動した座標系を返します。
    /// </summary>
    /// <param name="dir">移動量</param>
    /// <returns>移動後の座標系</returns>
    public LocalCodSys3d Translate( in Vector3d dir )
    {
      return new LocalCodSys3d( this.Origin + dir, this );
    }
  }
}
