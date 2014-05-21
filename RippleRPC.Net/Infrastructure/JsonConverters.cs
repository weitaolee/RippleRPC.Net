using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using RippleRPC.Net.Model;

namespace Newtonsoft.Json.Converters
{
    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        private DateTime initialTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                DateTime currentValue = ((DateTime) value).ToUniversalTime();
                TimeSpan span = currentValue - initialTime;
                writer.WriteValue(span.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            }
            else
                throw new ArgumentException("Must provide a valid datetime struct", "value");
            
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long ticks = Convert.ToInt64(reader.Value);
            return initialTime.AddSeconds(ticks);
        }
        
    }

    public class RippleValueConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            double val = ((double) value)*1000000;
            writer.WriteValue(val.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            double val = Convert.ToDouble(reader.Value);
            return val/1000000;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof (double))
                return true;
            return false;
        }
    }

    public class OfferItemConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return serializer.Deserialize<OfferItem>(reader);
            }
            catch (JsonSerializationException){}

            try
            {
                double val = serializer.Deserialize<double>(reader);
                return val / 1000000;
            }
            catch (JsonSerializationException) { }
            
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(object))
                return true;
            return false;
        }
    }

    public class JsonEnumTypeConverter<T> : JsonConverter
    {

        //adapted from: http://stackoverflow.com/questions/794838/datacontractjsonserializer-and-enums/794962#794962
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((T) value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Enum.Parse(typeof(T), reader.Value.ToString(), true);            
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (T);
        }
    }
}
