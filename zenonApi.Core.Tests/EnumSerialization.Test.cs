﻿using System.Xml.Linq;
using Xunit;
using zenonApi.Serialization;

namespace zenonApi.Core.Tests
{
  public class EnumSerialization
  {
    public enum EnumSerializationEnum
    {
      [zenonSerializableEnum("AbcTest")]
      Abc,
      [zenonSerializableEnum("DefTest")]
      Def,
      [zenonSerializableEnum("GhiTest")]
      Ghi
    }

    #region EnumSerializationAsNode
    public class EnumSerializationAsNodeClass : zenonSerializable<EnumSerializationAsNodeClass>
    {
      [zenonSerializableNode(nameof(SimpleInteger))]
      public int SimpleInteger { get; set; }

      [zenonSerializableNode(nameof(SimpleEnumSerializationEnum))]
      public EnumSerializationEnum SimpleEnumSerializationEnum { get; set; }
    }

    public static EnumSerializationAsNodeClass EnumSerializationAsNodeClassImpl => new EnumSerializationAsNodeClass
    {
      SimpleInteger = 1234,
      SimpleEnumSerializationEnum = EnumSerializationEnum.Abc
    };

    [Fact]
    public void TestEnumSerializationAsNodeToString()
    {
      var enumSerializationAsNodeClass = EnumSerializationAsNodeClassImpl;
      var result = enumSerializationAsNodeClass.ExportAsString();
      Assert.Equal(ComparisonValues.EnumSerializationAsNode, result);

      var deserialized = EnumSerializationAsNodeClass.Import(XElement.Parse(result));
      Assert.True(enumSerializationAsNodeClass.DeepEquals(deserialized, nameof(IZenonSerializable.ObjectStatus)));
    }

    [Fact]
    public void TestEnumSerializationAsNodeToXElement()
    {
      var enumSerializationAsNodeClass = EnumSerializationAsNodeClassImpl;
      var result = enumSerializationAsNodeClass.ExportAsXElement();
      Assert.True(XNode.DeepEquals(XElement.Parse(ComparisonValues.EnumSerializationAsNode), result));

      var deserialized = EnumSerializationAsNodeClass.Import(result);
      Assert.True(enumSerializationAsNodeClass.DeepEquals(deserialized, nameof(IZenonSerializable.ObjectStatus)));
    }
    #endregion


    #region EnumSerializationAsParameter
    public class EnumSerializationAsParameterClass : zenonSerializable<EnumSerializationAsParameterClass>
    {
      [zenonSerializableNode(nameof(SimpleInteger))]
      public int SimpleInteger { get; set; }

      [zenonSerializableAttribute(nameof(SimpleEnumSerializationEnum))]
      public EnumSerializationEnum SimpleEnumSerializationEnum { get; set; }

    }

    public static EnumSerializationAsParameterClass EnumSerializationAsParameterClassImpl => new EnumSerializationAsParameterClass
    {
      SimpleInteger = 1234,
      SimpleEnumSerializationEnum = EnumSerializationEnum.Abc
    };

    [Fact]
    public void TestEnumSerializationAsParameterToString()
    {
      var enumSerializationAsParameterClass = EnumSerializationAsParameterClassImpl;
      var result = enumSerializationAsParameterClass.ExportAsString();
      Assert.Equal(ComparisonValues.EnumSerializationAsParameter, result);

      var deserialized = EnumSerializationAsParameterClass.Import(XElement.Parse(result));
      Assert.True(enumSerializationAsParameterClass.DeepEquals(deserialized, nameof(IZenonSerializable.ObjectStatus)));
    }

    [Fact]
    public void TestEnumSerializationAsParameterToXElement()
    {
      var enumSerializationAsParameterClass = EnumSerializationAsParameterClassImpl;
      var result = enumSerializationAsParameterClass.ExportAsXElement();

      var withoutXmlHeader = XDocument.Parse(ComparisonValues.EnumSerializationAsParameter).Root;
      Assert.True(XNode.DeepEquals(withoutXmlHeader, result));

      var deserialized = EnumSerializationAsParameterClass.Import(result);
      Assert.True(enumSerializationAsParameterClass.DeepEquals(deserialized, nameof(IZenonSerializable.ObjectStatus)));
    }
    #endregion
  }
}
