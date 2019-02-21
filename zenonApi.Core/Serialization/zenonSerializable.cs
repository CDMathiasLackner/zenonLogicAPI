﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;

// TODO: Add converters for zenonSerializableNode
// TODO: Add order considerations for the items
// Nullable

namespace zenonApi.Serialization
{
  public abstract class zenonSerializable<TSelf, TParent, TRoot> : IZenonSerializable<TParent, TRoot>
    where TSelf : zenonSerializable<TSelf, TParent, TRoot>
    where TParent : class, IZenonSerializable
    where TRoot : class, IZenonSerializable
  {
    protected abstract string NodeName { get; }
    public virtual TParent Parent { get; protected set; }
    public virtual TRoot Root { get; protected set; }

    static Dictionary<Type, IZenonSerializationConverter> converterCache = new Dictionary<Type, IZenonSerializationConverter>();


    public virtual XElement Export()
    {
      // Create a node for the current element, check for all properties with a zenonSerializableAttribute-
      // or zenonSerializableNode-Attribute and append them
      XElement current = new XElement(this.NodeName);
      TSelf self = (TSelf)this;

      // Get all the properties together with their attributes in tuples
      var properties = this.GetType().GetRuntimeProperties().Select(x => (property: x, attributes: x.GetCustomAttributes()));

      // Group the tuples by the required attribute types and order them if required by their specified serialization order
      var attributesDict = properties
        .Select(x => (property: x.property, attribute: x.attributes.OfType<zenonSerializableAttributeAttribute>().FirstOrDefault()))
        .Where(x => x.attribute != null)
        .OrderBy(x => x.attribute.AttributeOrder);

      var nodesDict = properties
        .Select(x => (property: x.property, attribute: x.attributes.OfType<zenonSerializableNodeAttribute>().FirstOrDefault()))
        .Where(x => x.attribute != null)
        .OrderBy(x => x.attribute.NodeOrder);

      var contentsDict = properties
        .Select(x => (property: x.property, attribute: x.attributes.OfType<zenonSerializableNodeContentAttribute>().FirstOrDefault()))
        .Where(x => x.attribute != null);

      // Now simply create everything according to the preselection
      foreach (var attributeDict in attributesDict)
      {
        exportAttribute(current, self, attributeDict.property, attributeDict.attribute);
      }

      foreach (var nodeDict in nodesDict)
      {
        exportNode(current, self, nodeDict.property, nodeDict.attribute);
      }

      foreach (var contentDict in contentsDict)
      {
        exportNodeContent(current, self, contentDict.property, contentDict.attribute);
      }

      return current;
    }


    private static void exportAttribute(XElement target, TSelf source, PropertyInfo property, zenonSerializableAttributeAttribute attributeAttribute)
    {
      if (property.GetGetMethod(true) == null)
      {
        // No getter, nothing to do
        return;
      }

      if (attributeAttribute != null)
      {
        object sourceValue = property.GetValue(source);
        if (attributeAttribute.OmitIfNull && sourceValue == null)
        {
          // Omit the whole node
          return;
        }

        // Check if there is an converter for this property
        if (attributeAttribute.Converter != null)
        {
          IZenonSerializationConverter converterInstance = getConverter(attributeAttribute.Converter);
          // Ensure to call the correct method overload of the converter by using the (object)-cast
          target.SetAttributeValue(attributeAttribute.AttributeName, converterInstance.Convert((object)property.GetValue(source)));
        }
        else
        {
          var value = property.GetValue(source);
          if (value == null)
          {
            // If the value is null, we omit the attribute
            return;
          }

          var valueType = value.GetType();
          if (valueType.IsEnum)
          {
            // Try to find a zenonSerializable attribute to write the correct value
            var attribute = valueType.GetField(value.ToString()).GetCustomAttribute<zenonSerializableEnumAttribute>();
            if (attribute != null)
            {
              // Set the value from the attribute, otherwise use the default string value (after the outer if-clause)
              target.SetAttributeValue(attributeAttribute.AttributeName, attribute.Name);
              return;
            }
          }

          string stringValue = property.GetValue(source)?.ToString();
          target.SetAttributeValue(attributeAttribute.AttributeName, stringValue);
        }
      }
    }


    private static void exportNode(XElement target, TSelf source, PropertyInfo property, zenonSerializableNodeAttribute nodeAttribute)
    {
      if (property.GetGetMethod(true) == null)
      {
        // No getter, nothing to do
        return;
      }

      if (nodeAttribute != null)
      {
        object sourceValue = property.GetValue(source);
        if (nodeAttribute.OmitIfNull && sourceValue == null)
        {
          // Omit the whole node
          return;
        }

        if (typeof(IZenonSerializable).IsAssignableFrom(property.PropertyType))
        {
          MethodInfo exportMethod = property.PropertyType.GetMethod(nameof(Export), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
          XElement child = (XElement)exportMethod.Invoke(property.GetValue(source), null);

          target.Add(child);
        }
        else if (property.PropertyType.IsEnum)
        {
          // TODO: zenonSerializableEnum? Required at all?
        }
        else if (typeof(IList).IsAssignableFrom(property.PropertyType))
        {
          IList list = (IList)property.GetValue(source);
          if (list == null)
          {
            return;
          }

          // Currently we only support a list of IZenonSerializable
          var genericParameterType = property.PropertyType.GenericTypeArguments.First();
          if (!typeof(IZenonSerializable).IsAssignableFrom(genericParameterType))
          {
            throw new NotImplementedException($"Currently, only lists of types derived by {nameof(IZenonSerializable)} are supported.");
          }

          foreach (IZenonSerializable listItem in list)
          {
            MethodInfo exportMethod = genericParameterType.GetMethod(nameof(Export), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            XElement child = (XElement)exportMethod.Invoke(listItem, null);

            target.Add(child);
          }
        }
        else
        {
          // Just write the string representation of the property as the value
          XElement child = new XElement(nodeAttribute.NodeName);
          var value = property.GetValue(source)?.ToString();
          if (value != null)
          {
            child.Value = value;
          }

          target.Add(child);
        }
      }
    }


    private static void exportNodeContent(XElement target, TSelf source, PropertyInfo property, zenonSerializableNodeContentAttribute contentAttribute)
    {
      if (property.GetGetMethod(true) == null)
      {
        // No getter, nothing to do
        return;
      }

      if (contentAttribute != null)
      {
        object sourceValue = property.GetValue(source);
        if (sourceValue == null)
        {
          return;
        }

        // Check if there is an converter for this property
        if (contentAttribute.Converter != null)
        {
          IZenonSerializationConverter converterInstance = getConverter(contentAttribute.Converter);
          // Ensure to call the correct method overload of the converter by using the (object)-cast
          target.Value = converterInstance.Convert((object)property.GetValue(source));
        }
        else
        {
          var value = property.GetValue(source);
          if (value == null)
          {
            // If the value is null, we set the content to null too.
            target.Value = null;
            return;
          }

          var valueType = value.GetType();
          if (valueType.IsEnum)
          {
            // Try to find a zenonSerializable attribute to write the correct value
            var attribute = valueType.GetField(value.ToString()).GetCustomAttribute<zenonSerializableEnumAttribute>();
            if (attribute != null)
            {
              // Set the value from the attribute, otherwise use the default string value (after the outer if-clause)
              target.SetValue(attribute.Name);
              return;
            }
          }

          string stringValue = property.GetValue(source)?.ToString();
          target.SetValue(stringValue);
        }
      }
    }


    public static TSelf Import(XElement source, TParent parent = null, TRoot root = null)
    {
      // Create an instance of the current type
      var result = (TSelf)Activator.CreateInstance(typeof(TSelf), true);
      result.Parent = parent;
      result.Root = root;
      Type t = typeof(TSelf);

      // The source element must match the node name
      if (source.Name != result.NodeName)
      {
        throw new Exception($"Expected {result.NodeName}, but got {source.Name}");
      }

      // Find all the attributes and properties of the current type for deserialization
      foreach (var property in t.GetRuntimeProperties())
      {
        if (property.GetSetMethod(true) == null)
        {
          // No setter, nothing to do at all
          continue;
        }

        importAttribute(result, source, property);
        importNode(result, source, property);
        importNodeContent(result, source, property);
      }

      return result;
    }


    private static IZenonSerializationConverter getConverter(Type converterType)
    {
      if (converterType == null)
      {
        throw new ArgumentNullException(nameof(converterType));
      }

      IZenonSerializationConverter converterInstance = null;
      if (converterCache.ContainsKey(converterType))
      {
        converterInstance = converterCache[converterType];
      }
      else
      {
        converterInstance = (IZenonSerializationConverter)Activator.CreateInstance(converterType, true);
        converterCache[converterType] = converterInstance;
      }

      return converterInstance;
    }


    private static void importAttribute(TSelf target, XElement sourceXml, PropertyInfo property)
    {
      var zenonAttribute = property.GetCustomAttribute<zenonSerializableAttributeAttribute>();
      if (zenonAttribute != null)
      {
        var xmlAttribute = sourceXml.Attributes(zenonAttribute.AttributeName).FirstOrDefault();
        if (xmlAttribute == null)
        {
          return;
        }

        // Check if there is a converter
        if (zenonAttribute.Converter != null)
        {
          IZenonSerializationConverter converterInstance = getConverter(zenonAttribute.Converter);
          property.SetValue(target, converterInstance.Convert(xmlAttribute.Value));
        }
        else if (property.PropertyType.IsEnum)
        {
          // If no converter is registered, try to convert it manually
          // TODO: Special types like guids would be nice to consider here

          foreach (var value in Enum.GetValues(property.PropertyType))
          {
            var field = property.PropertyType.GetField(value.ToString());

            // Try to match the enum value either by the attribute or the string name
            var attribute = field.GetCustomAttribute<zenonSerializableEnumAttribute>();
            if ((attribute != null && xmlAttribute.Value == attribute.Name)
              || xmlAttribute.Value == field.Name)
            {
              property.SetValue(target, value);
              return;
            }
          }

          // If neither the attribute nor the string name matches, something went wrong
          throw new Exception($"Cannot set value \"{xmlAttribute.Value}\" for {property.PropertyType.Name}, either a "
            + $"{nameof(zenonSerializableEnumAttribute)} must be set for the enum fields or "
            + $"the name must exactly match the XML value.");
        }
        else if (typeof(IConvertible).IsAssignableFrom(property.PropertyType))
        {
          var converted = Convert.ChangeType(xmlAttribute.Value, property.PropertyType);
          property.SetValue(target, converted);
        }
        else
        {
          throw new Exception($"Cannot convert types without an {nameof(IZenonSerializationConverter)}, which do not implement {nameof(IConvertible)}.");
        }
      }
    }


    private static void importNode(TSelf target, XElement sourceXml, PropertyInfo property)
    {
      var prop = property.GetCustomAttribute<zenonSerializableNodeAttribute>();
      if (prop != null)
      {
        var xmlNodes = sourceXml.Elements(prop.NodeName).OfType<XNode>().Cast<XElement>().ToList();

        // Currently we only support deserializing to a concrete List types, not IEnumerable or similar, maybe in the future
        if (typeof(IList).IsAssignableFrom(property.PropertyType))
        {
          // Create the list which will hold the instances
          IList list = (IList)Activator.CreateInstance(property.PropertyType, true);

          // Get the generic type, so that we can instantiate entries for the list (this should be a zenonSerializable)
          Type genericParameter = property.PropertyType.GetGenericArguments().First();
          if (typeof(IZenonSerializable).IsAssignableFrom(genericParameter))
          {
            foreach (var xmlNode in xmlNodes)
            {
              MethodInfo importMethod = genericParameter.GetMethod(nameof(Import), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
              object root = target.Root;
              if (root == null && target.Parent == null)
              {
                // the current object is most likely the desired root
                root = target;
              }
              var child = importMethod.Invoke(null, new object[] { xmlNode, target, root });

              list.Add(child);
            }
          }

          property.SetValue(target, list);
        }
        else if (typeof(IZenonSerializable).IsAssignableFrom(property.PropertyType))
        {
          if (xmlNodes.Count > 1)
          {
            throw new Exception($"Expected a IList<{nameof(IZenonSerializable)}>, but got {nameof(IZenonSerializable)}.");
          }

          if (xmlNodes.Count == 1)
          {
            MethodInfo importMethod = property.PropertyType.GetMethod(nameof(Import), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            object root = target.Root;
            if (root == null)
            {
              root = target;
            }

            var child = importMethod.Invoke(null, new object[] { xmlNodes.First(), target, root });

            property.SetValue(target, child);
          }
        }
        else if (xmlNodes.Count == 1)
        {
          // TODO: There are no converters yet for this case
          // Just try to deserialize the value directly
          if (typeof(IConvertible).IsAssignableFrom(property.PropertyType))
          {
            var value = Convert.ChangeType(xmlNodes.First().Value, property.PropertyType);
            property.SetValue(target, value);
          }
          else
          {
            throw new Exception($"Cannot convert types without an {nameof(IZenonSerializationConverter)}, which do not implement {nameof(IConvertible)}.");
          }
        }
      }
    }


    private static void importNodeContent(TSelf target, XElement sourceXml, PropertyInfo property)
    {
      var zenonAttribute = property.GetCustomAttribute<zenonSerializableNodeContentAttribute>();
      if (zenonAttribute != null)
      {
        if (sourceXml.Value == null)
        {
          return;
        }

        // Check if there is a converter
        if (zenonAttribute.Converter != null)
        {
          IZenonSerializationConverter converterInstance = getConverter(zenonAttribute.Converter);
          property.SetValue(target, converterInstance.Convert(sourceXml.Value));
        }
        else if (property.PropertyType.IsEnum)
        {
          // If no converter is registered, try to convert it manually
          // TODO: Special types like guids would be nice to consider here

          foreach (var value in Enum.GetValues(property.PropertyType))
          {
            var field = property.PropertyType.GetField(value.ToString());

            // Try to match the enum value either by the attribute or the string name
            var attribute = field.GetCustomAttribute<zenonSerializableEnumAttribute>();
            if ((attribute != null && sourceXml.Value == attribute.Name)
              || sourceXml.Value == field.Name)
            {
              property.SetValue(target, value);
              return;
            }
          }

          // If neither the attribute nor the string name matches, something went wrong
          throw new Exception($"Cannot set value \"{sourceXml.Value}\" for {property.PropertyType.Name}, either a "
            + $"{nameof(zenonSerializableEnumAttribute)} must be set for the enum fields or "
            + $"the name must exactly match the XML value.");
        }
        else if (typeof(IConvertible).IsAssignableFrom(property.PropertyType))
        {
          var converted = Convert.ChangeType(sourceXml.Value, property.PropertyType);
          property.SetValue(target, converted);
        }
        else
        {
          throw new Exception($"Cannot convert types without an {nameof(IZenonSerializationConverter)}, which do not implement {nameof(IConvertible)}.");
        }
      }
    }
  }
}
