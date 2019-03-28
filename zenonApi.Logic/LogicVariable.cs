﻿using zenonApi.Collections;
using zenonApi.Logic.SerializationConverters;
using zenonApi.Serialization;

namespace zenonApi.Logic
{
  public class LogicVariable : zenonSerializable<LogicVariable>
  {
    #region zenonSerializable Implementation
    public override string NodeName => "var";
    #endregion  

    /// <summary>
    /// Symbol of the variable.
    /// This attribute is mandatory.
    /// </summary>
    [zenonSerializableAttribute("name", AttributeOrder = 0)]
    public string Name { get; set; } // TODO: Check for correct naming? (StringExtensions.IsValidZenonLogicName)

    /// <summary>
    /// Name of the data type of the variable.
    /// This attribute is mandatory.
    /// </summary>
    [zenonSerializableAttribute("type", AttributeOrder = 1)]
    public string Type { get; set; }

    /// <summary>
    /// Maximum length if the data type is STRING.
    /// This attribute is mandatory for STRING variables, and should not appear for other data types.
    /// </summary>
    //[zenonSerializableAttribute("len", AttributeOrder = 2)]
    //public int MaxStringLength { get; set; } = 255; // TODO: Should only be used when datatype is string//TODO: Include again

    /// <summary>
    /// Dimension(s) if the variable is an array.
    /// There are at most 3 dimensions, separated by commas.
    /// This attribute is optional.
    /// </summary>
    //[zenonSerializableAttribute("dim", AttributeOrder = 3, Converter = typeof(CoordinateConverter))] //TODO: Include again
    //public (int X, int Y, int Z) ArrayDimension { get; set; } = (0,0,0);

    /// <summary>
    /// Attributes of the variable, separated by comas.
    /// This attribute is optional.
    /// </summary>
    [zenonSerializableAttribute("attr", AttributeOrder = 4, Converter = typeof(LogicVariableAttributeConverter))]
    public LogicVariableAttributes Attributes { get; protected set; } = new LogicVariableAttributes();

    /// <summary>
    /// Initial value of the variable.
    /// Must be a valid constant expression that fits the data type.
    /// This attribute is optional.
    /// </summary>
    [zenonSerializableAttribute("init", AttributeOrder = 5)]
    public string InitialValue { get; set; }

    /// <summary>
    /// Indicates additional information for the variable it belongs to.
    /// </summary>
    [zenonSerializableNode("varinfo", NodeOrder = 6)]
    public ExtendedObservableCollection<LogicVariableInfo> VariableInfos { get; protected set; }
      = new ExtendedObservableCollection<LogicVariableInfo>();
  }
}
