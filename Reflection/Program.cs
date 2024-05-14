using System;
using System.Reflection;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field)]
public class CustomNameAttribute : Attribute
{
    public string Name { get; private set; }

    public CustomNameAttribute(string name)
    {
        Name = name;
    }
}

public class MyClass
{
    [CustomName("CustomFieldName")]
    public int I = 0;

    public string ObjectToString()
    {
        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        var pairs = new List<string>();

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<CustomNameAttribute>();
            var fieldName = attribute != null ? attribute.Name : field.Name;
            var fieldValue = field.GetValue(this);
            pairs.Add($"{fieldName}:{fieldValue}");
        }

        return string.Join(", ", pairs);
    }

    public static MyClass StringToObject(string data)
    {
        var instance = new MyClass();
        var pairs = data.Split(new[] { ", " }, StringSplitOptions.None);

        foreach (var pair in pairs)
        {
            var parts = pair.Split(':');
            var propertyName = parts[0];
            var propertyValue = parts[1];

            var field = instance.GetType().GetField(propertyName) ??
                        GetFieldByCustomName(instance, propertyName);

            if (field != null)
            {
                field.SetValue(instance, Convert.ChangeType(propertyValue, field.FieldType));
            }
        }

        return instance;
    }

    private static FieldInfo GetFieldByCustomName(MyClass instance, string customName)
    {
        var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<CustomNameAttribute>();
            if (attribute != null && attribute.Name == customName)
            {
                return field;
            }
        }

        return null;
    }
}

class Program
{
    static void Main()
    {
        var myObject = new MyClass { I = 42 };
        var serialized = myObject.ObjectToString();
        Console.WriteLine(serialized); 

        var deserialized = MyClass.StringToObject("CustomFieldName:42");
        Console.WriteLine(deserialized.I); 
    }
}
