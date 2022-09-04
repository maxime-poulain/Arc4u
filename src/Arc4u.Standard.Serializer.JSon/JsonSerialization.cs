using System;
using System.Diagnostics;
using System.Text.Json;

namespace Arc4u.Serializer
{
    public class JsonSerialization : IObjectSerialization
    {
        private const string SerializerTypeTagName = "SerializerType";
        private const string SerializerTypeTagValue = "Json";

        private void SetCurrentActiviySerializerType()
            => Activity.Current?.SetTag(SerializerTypeTagName, SerializerTypeTagValue);

        public byte[] Serialize<T>(T value)
        {
            SetCurrentActiviySerializerType();
            if (value is TimeSpan timeSpan)
            {
                return JsonSerializer.SerializeToUtf8Bytes(timeSpan.Ticks);
            }

            return JsonSerializer.SerializeToUtf8Bytes(value);
        }

        public T Deserialize<T>(byte[] data)
        {
            SetCurrentActiviySerializerType();
            var objectType = typeof(T);
            if (objectType == typeof(TimeSpan?))
            {
                long? ticks = JsonSerializer.Deserialize<long?>(data);
                return ticks.HasValue ? (T) (new TimeSpan(ticks.Value) as object) : default;
            }
            else if (objectType == typeof(TimeSpan))
            {
                return (T) (new TimeSpan(JsonSerializer.Deserialize<long>(data)) as object);
            }

            return JsonSerializer.Deserialize<T>(data);
        }

        public object Deserialize(byte[] data, Type objectType)
        {
            SetCurrentActiviySerializerType();
            if (objectType == typeof(TimeSpan?))
            {
                long? ticks = JsonSerializer.Deserialize<long?>(data);
                return ticks.HasValue ? new TimeSpan(ticks.Value) : null;
            }
            else if (objectType == typeof(TimeSpan))
            {
                return new TimeSpan(JsonSerializer.Deserialize<long>(data));
            }

            return JsonSerializer.Deserialize(data, objectType);
        }
    }
}
