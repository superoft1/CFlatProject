using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.App
{
  /// <summary>
  /// アプリケーション引数を<see cref="Options" />クラスに登録するためのクラス。
  /// </summary>
  static class ArgumentReader
  {
    /// <summary>
    /// アプリケーション引数を読み取って、<see cref="Chiyoda.App.Options" />クラスに設定します。
    /// </summary>
    /// <param name="args">Main関数の引数args。</param>
    public static void Read( string[] args )
    {
      Options.Instance.Reset();

      // TODO: オプション読み取り
    }
  }
}
