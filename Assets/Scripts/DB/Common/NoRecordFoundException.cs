namespace Chiyoda.DB
{
  public class NoRecordFoundException : System.Exception
  {
    public NoRecordFoundException()
    {
    }

    public NoRecordFoundException(string message) : base(message)
    {
    }
  }
}