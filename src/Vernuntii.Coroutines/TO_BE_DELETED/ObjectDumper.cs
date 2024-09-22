using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vernuntii.Coroutines.TO_BE_DELETED;

internal static class ObjectDumper
{
    public static void Dump<T>(this T obj)
    {
        Console.WriteLine(JsonConvert.SerializeObject(obj, new JsonSerializerSettings() {
            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) {
                args.ErrorContext.Handled = true;
            },
            ContractResolver = new MyContractResolver(),
            //MaxDepth = 1,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All
        }));
    }

    public class MyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            //var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //                .Select(p => base.CreateProperty(p, memberSerialization))
            //            .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //                       .Select(f => base.CreateProperty(f, memberSerialization)))
            //            .ToList();
            var props = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Select(f => base.CreateProperty(f, memberSerialization))
                        .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }
    }

    //public static void Dump<T>(this T obj)
    //{
    //    Console.WriteLine(JsonSerializer.Serialize(obj, typeof(T), new JsonSerializerOptions() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() { Modifiers = { AddInternalPropertiesModifier } }, IncludeFields = true }));
    //}

    //static void AddInternalPropertiesModifier(JsonTypeInfo jsonTypeInfo)
    //{
    //    if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
    //        return;

    //    //foreach (PropertyInfo property in jsonTypeInfo.Type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)) {
    //    //    if (property.PropertyType.IsByRef)
    //    //        continue;
    //    //    if (property.PropertyType.IsSubclassOf(typeof(Delegate)))
    //    //        continue;
    //    //    JsonPropertyInfo jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(property.PropertyType, property.Name);
    //    //    jsonPropertyInfo.Get = property.GetValue;
    //    //    jsonPropertyInfo.Set = property.SetValue;
    //    //    jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    //    //}
    //    foreach (FieldInfo property in jsonTypeInfo.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)) {
    //        if (property.FieldType.IsByRef)
    //            continue;
    //        JsonPropertyInfo jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(property.FieldType, property.Name);
    //        jsonPropertyInfo.Get = property.GetValue;
    //        jsonPropertyInfo.Set = property.SetValue;
    //        jsonTypeInfo.Properties.Add(jsonPropertyInfo);
    //    }
    //}
}
