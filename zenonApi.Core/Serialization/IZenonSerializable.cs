﻿using System.Collections.Generic;
using System.Xml.Linq;

namespace zenonApi.Serialization
{
  public interface IZenonSerializable<out TSelf, out TParent, out TRoot>
    : IZenonSerializable<TSelf>
    where TSelf : class, IZenonSerializable<TSelf>
  {
    TParent Parent { get; }
    TRoot Root { get; }
  }

  public interface IZenonSerializable<out TSelf> : IZenonSerializable
    where TSelf : class
  { }

  public interface IZenonSerializable
  {
    /// <summary>
    /// The name of the item in its XML representation.
    /// </summary>
    string NodeName { get; }

    /// <summary>
    /// Contains all unknown nodes, which are not covered by this API and were found for the current item.
    /// The key specifies the original tag name from XML, the value contains the entire XElement representing it.
    /// </summary>
    Dictionary<string, XElement> UnknownNodes { get; }

    /// <summary>
    /// Contains all unknown attributes, which are not covered by this API and were found for the current item.
    /// The key specifies the original tag name from XML, the value contains the attribute's value.
    /// </summary>
    Dictionary<string, string> UnknownAttributes { get; }
  }
}
