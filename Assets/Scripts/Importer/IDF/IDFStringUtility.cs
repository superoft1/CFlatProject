using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

namespace IDF
{
  public static class IDFStringUtility
  {
    public static string[] Split( string text )
    {
      var list = new List<string>();
      bool inSpace = true, inComma = false;
      int lastPos = 0, pos = 0;
      foreach ( var c in text ) {
        switch ( c ) {
          case ' ':
          case '\t':
            if ( !inSpace ) {
              list.Add( text.Substring( lastPos, pos - lastPos ) );
              inSpace = true;
              inComma = false;
            }
            break;

          case ',':
            if ( !inSpace ) {
              list.Add( text.Substring( lastPos, pos - lastPos ) );
              inSpace = true;
              inComma = true;
            }
            else if ( inComma ) {
              list.Add( string.Empty );
            }
            break;

          default:
            if ( inSpace ) {
              lastPos = pos;
              inSpace = false;
              inComma = false;
            }
            break;
        }
        ++pos;
      }
      if ( !inSpace ) list.Add( text.Substring( lastPos ) );
      return list.ToArray();
    }
  }
}