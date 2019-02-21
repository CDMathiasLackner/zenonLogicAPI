﻿using System;
using System.Collections.Generic;
using System.Text;

namespace zenonApi.Serialization
{
  public interface IZenonSerializable<TParent, TRoot> : IZenonSerializable
  {
    TParent Parent { get; }
    TRoot Root { get; }
  }

  public interface IZenonSerializable
  {
  }
}