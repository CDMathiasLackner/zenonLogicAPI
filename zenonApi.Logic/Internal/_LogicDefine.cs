﻿using zenonApi.Serialization;

namespace zenonApi.Logic.Internal
{
  /// <summary>
  /// Describes a group of definitions.
  /// </summary>
  internal class _LogicDefine : zenonSerializable<_LogicDefine>
  {
    #region Interface implementation
    public override string NodeName => "defines";
    #endregion

    /// <summary>
    /// The mandatory name of the group. Values can be "(COMMON)", "(GLOBAL)" or
    /// a program name.
    /// "(COMMON)" specifies that the definitions are common to all projects,
    /// "(GLOBAL)" specifies that the definitions are global for the project.
    /// </summary>
    [zenonSerializableAttribute("name")]
    public string Name { get; set; }

    /// <summary>
    /// Formated text (extended ST format using #define pragmas).
    /// </summary>
    [zenonSerializableNodeContent]
    public string SourceCode { get; set; }
  }
}
