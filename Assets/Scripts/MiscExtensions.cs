using System;
using System.Collections.Generic;
using System.Text ;

namespace Chiyoda
{
  public static class MiscExtensions
  {
    public static void ForEach<T>( this IEnumerable<T> enu, Action<T> action )
    {
      foreach ( var t in enu ) action( t );
    }

    public static void ForEach<T>( this IEnumerable<T> enu, Action<T, int> action )
    {
      int i = 0 ;
      foreach ( var t in enu ) {
        action( t, i ) ;
        ++i ;
      }
    }

    public static void Deconstruct<TKey, TValue>( this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value )
    {
      key = pair.Key;
      value = pair.Value;
    }

    public static SortedList<TKey, TValue> ToSortedList<TKey, TValue>( this IEnumerable<TValue> enu, Converter<TValue, TKey> keySelector )
    {
      var list = new SortedList<TKey, TValue>() ;
      foreach ( var value in enu ) {
        list.Add( keySelector( value ), value ) ;
      }

      return list ;
    }

    internal static object GetTreeViewItemSource( this UI.TreeViewItem treeViewItem )
    {
      if ( null == treeViewItem ) return null;
      if ( treeViewItem.Tag is UI.ITreeViewSource source ) return source.Value;
      return treeViewItem.Tag;
    }

    internal static object GetListBoxItemSource( this UI.ListBoxItem listBoxItem )
    {
      if ( null == listBoxItem ) return null;
      return listBoxItem.Tag;
    }

    public static int ToInteger( this double value )
    {
      if ( int.MaxValue <= value ) return int.MaxValue ;
      if ( value <= int.MinValue ) return int.MinValue ;
      return (int)Math.Floor( value + 0.5 );
    }
    public static int ToInteger( this float value )
    {
      if ( int.MaxValue <= value ) return int.MaxValue ;
      if ( value <= int.MinValue ) return int.MinValue ;
      return (int)Math.Floor( value + 0.5 );
    }
    public static bool ToBoolean( this double value )
    {
      return (0 != value);
    }
    public static bool ToBoolean( this float value )
    {
      return (0 != value);
    }



    private static readonly char[] CSV_SEPARETOR_CHARS = { ',', '\r', '\n' } ;
    
    public static string QuoteCSV( this string str )
    {
      if ( 0 <= str.IndexOfAny( CSV_SEPARETOR_CHARS ) ) return str ;  // クオーテーション不要
      return $"\"{str.Replace( "\"", "\"\"" )}\"" ;  // "→""
    }

    public static string UnquoteCsv( this string str, out int nextStartIndex, out bool endOfLine )
    {
      return str.UnquoteCsv( 0, out nextStartIndex, out endOfLine ) ;
    }

    public static string UnquoteCsv( this string str, int startIndex, out int nextStartIndex, out bool endOfLine )
    {
      if ( str.Length <= startIndex ) {
        nextStartIndex = startIndex ;
        endOfLine = true ;
        return null ;
      }

      if ( '"' != str[ startIndex ] ) {
        // "なし
        var separatorIndex = str.IndexOfAny( CSV_SEPARETOR_CHARS, startIndex ) ;
        if ( separatorIndex < 0 ) {
          // 最後まで
          endOfLine = true ;
          nextStartIndex = str.Length ;
          return str.Substring( startIndex ) ;
        }
        else if ( ',' == str[ separatorIndex ] ) {
          // ,で区切り
          nextStartIndex = separatorIndex + 1 ;
          endOfLine = false ;
        }
        else if ( '\n' == str[ separatorIndex ] ) {
          // \nで区切り
          nextStartIndex = separatorIndex + 1 ;
          endOfLine = true ;
        }
        else {
          // \rの場合、\rか\r\nかで分岐
          if ( separatorIndex == str.Length - 1 || '\n' != str[ separatorIndex + 1 ] ) {
            // \r
            nextStartIndex = separatorIndex + 1 ;
          }
          else {
            // \r\n
            nextStartIndex = separatorIndex + 2 ;
          }
          endOfLine = true ;
        }

        return str.Substring( startIndex, separatorIndex - startIndex ) ;
      }
      else {
       // "あり
       var builder = new StringBuilder() ;
       int strPos = startIndex + 1 ;
       while ( true ) {
         int nextQuoteIndex = str.IndexOf( '"', strPos ) ;
         if ( nextQuoteIndex < 0 ) {
           // 終了まで("がない)
           nextStartIndex = str.Length ;
           endOfLine = true ;
           builder.Append( str.Substring( strPos ) ) ;
           return builder.ToString() ;
         }

         if ( nextQuoteIndex == str.Length - 1 ) {
           // "で終了
           nextStartIndex = str.Length ;
           endOfLine = true ;
           builder.Append( str.Substring( strPos, nextQuoteIndex - strPos ) ) ;
           return builder.ToString() ;
         }
         if ( ',' == str[ nextQuoteIndex + 1 ] ) {
           // ",で区切り
           nextStartIndex = nextQuoteIndex + 2 ;
           endOfLine = false ;
           builder.Append( str.Substring( strPos, nextQuoteIndex - strPos ) ) ;
           return builder.ToString() ;
         }
         if ( '\n' == str[ nextQuoteIndex + 1 ] ) {
           // "\nで区切り
           nextStartIndex = nextQuoteIndex + 2 ;
           endOfLine = true ;
           builder.Append( str.Substring( strPos, nextQuoteIndex - strPos ) ) ;
           return builder.ToString() ;
         }
         if ( '\r' == str[ nextQuoteIndex + 1 ] ) {
           // \rの場合、\rか\r\nかで分岐
           if ( nextQuoteIndex == str.Length - 2 || '\n' != str[ nextQuoteIndex + 2 ] ) {
             // "\r
             nextStartIndex = nextQuoteIndex + 2 ;
           }
           else {
             // "\r\n
             nextStartIndex = nextQuoteIndex + 3 ;
           }
           endOfLine = true ;
           builder.Append( str.Substring( strPos, nextQuoteIndex - strPos ) ) ;
           return builder.ToString() ;
         }

         // ""を"に変換して継続
         builder.Append( str.Substring( strPos, nextQuoteIndex - strPos + 1 ) ) ;
         strPos = nextQuoteIndex + 2 ;
       }
      }
    }
  }
}
