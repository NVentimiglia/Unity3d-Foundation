using System;

namespace FullSerializer
{
    /// <summary>
    ///     Static instance for easy serializing
    /// </summary>
    public static class JsonSerializer
    {
        /// <summary>
        ///     Static Serializer Instance
        /// </summary>
        public static readonly fsSerializer Internal = new fsSerializer();

        /// <summary>
        ///     Serializes the object into json
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Serialize(object value, Type type)
        {
            // serialize the data
            fsData data;
            Internal.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

            // emit the data via JSON
            return fsJsonPrinter.CompressedJson(data);
        }

        /// <summary>
        ///     Serializes the object into json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="prettyJson"></param>
        /// <returns></returns>
        public static string Serialize<T>(T value, bool prettyJson = false)
        {
            fsData data;
            var r = Internal.TrySerialize(value, out data);

            if (r.Failed)
                throw r.AsException;

            if (data.IsDictionary && data.AsDictionary.ContainsKey("$content"))
                data = data.AsDictionary["$content"];

            if (!prettyJson)
                return fsJsonPrinter.CompressedJson(data);

            return fsJsonPrinter.PrettyJson(data);
        }

        /// <summary>
        ///     Deserializes the Json into an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            fsData data;
            var fsFailure1 = fsJsonParser.Parse(json, out data);
            if (fsFailure1.Failed)
                throw fsFailure1.AsException;

            var instance = default(T);

            var fsFailure2 = Internal.TryDeserialize(data, ref instance);

            if (fsFailure2.Failed)
                throw fsFailure2.AsException;

            return instance;
        }

        /// <summary>
        ///     Deserializes the Json into an object
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            fsData data;
            var fsFailure1 = fsJsonParser.Parse(json, out data);
            if (fsFailure1.Failed)
                throw fsFailure1.AsException;

            var instance = Activator.CreateInstance(type);

            var fsFailure2 = Internal.TryDeserialize(data, type, ref instance);

            if (fsFailure2.Failed)
                throw fsFailure2.AsException;

            return instance;
        }
    }
}