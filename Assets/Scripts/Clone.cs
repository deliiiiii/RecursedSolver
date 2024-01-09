using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class Clone
{
    public static T DeepCopy1<T>(T DeepCopyObject)
    {
        string _ = JsonConvert.SerializeObject(DeepCopyObject);
        return JsonConvert.DeserializeObject<T>(_);
    }
    public static T DeepCopy2<T>(T RealObject)
    {
        using (Stream objectStream = new MemoryStream())
        {
            //?? System.Runtime.Serialization?????????????????  
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, RealObject);
            objectStream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(objectStream);
        }
    }
    public static T DeepCopy3<T>(T source)
    {

        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", "source");
        }
        if (Object.ReferenceEquals(source, null))
        {
            return default(T);
        }
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream();
        using (stream)
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }
    public static T DeepCopy4<T>(T obj)
    {
        if (obj is string || obj.GetType().IsValueType)
            return obj;

        object retval = Activator.CreateInstance(obj.GetType());
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                field.SetValue(retval, DeepCopy4(field.GetValue(obj)));
            }
            catch { }
        }

        return (T)retval;
    }
}