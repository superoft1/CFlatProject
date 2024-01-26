using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.App
{
  /// <summary>
  /// 実行オプション クラス。
  /// </summary>
  class Options
  {
    private static Options _instance;

    /// <summary>
    /// 実行オプション クラスの現在のインスタンスを取得します。
    /// </summary>
    public static Options Instance
    {
      get
      {
        if(null== _instance ) {
          _instance = new Options();
        }

        return _instance;
      }
    }

    /// <summary>
    /// 全オプションをデフォルト設定とするコンストラクタ。
    /// </summary>
    private Options()
    {
      Reset();
    }

    /// <summary>
    /// 全オプションをデフォルト設定に設定します。
    /// </summary>
    public void Reset()
    {
      // TODO
    }
  }
}
