﻿using zenonApi.Collections;
using zenonApi.Serialization;

namespace zenonApi.Logic
{
  public interface ILogicFileContainer : IZenonSerializable<ILogicFileContainer, ILogicFileContainer, LogicProject>
  {
    ContainerAwareObservableCollection<LogicFolder> Folders { get; }

    ContainerAwareObservableCollection<LogicProgram> Programs { get; }
  }
}
