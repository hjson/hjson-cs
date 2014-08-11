using System;

namespace Hjson
{
  public interface IJsonReader
  {
    void Key(string name);
    void Index(int idx);
    void Value(JsonValue value);
  }
}
