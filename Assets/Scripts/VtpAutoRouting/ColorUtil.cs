namespace VtpAutoRouting
{
  public class ColorUtil
  {
    public static UnityEngine.Color GetUnity(string strColor)
    {
      if (strColor == "Blue")
      {
        return UnityEngine.Color.blue;
      }
      else if (strColor == "Magenta")
      {
        return UnityEngine.Color.magenta;
      }
      else if (strColor == "Orange")
      {
        return new UnityEngine.Color(230f / 255f, 81f / 255f, 0f / 255f);
      }
      else if (strColor == "Light Blue")
      {
        return new UnityEngine.Color(1f / 255f, 87f / 255f, 155f / 255f);
      }
      else if (strColor == "Yellow green")
      {
        return new UnityEngine.Color(51f / 255f, 105f / 255f, 30f / 255f);
      }
      else if (strColor == "PINK")
      {
        return new UnityEngine.Color(136f / 255f, 14f / 255f, 79f / 255f);
      }
      else if (strColor == "White")
      {
        return UnityEngine.Color.white;
      }
      else if (strColor == "Red")
      {
        return UnityEngine.Color.red;
      }
      else if (strColor == "Gray")
      {
        return UnityEngine.Color.gray;
      }
      else if (strColor == "Yellow")
      {
        return UnityEngine.Color.yellow;
      }

      return UnityEngine.Color.green;
    }
  }
}
