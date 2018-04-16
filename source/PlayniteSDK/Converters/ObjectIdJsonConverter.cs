using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public class ObjectIdJsonConverter : JsonConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }
            else
            {
                return new ObjectId((string)reader.Value);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(((ObjectId)value).ToString());
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ObjectIdContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == typeof(ObjectId))
            {
                JsonContract contract = base.CreateObjectContract(objectType);
                contract.Converter = new ObjectIdJsonConverter();
                return contract;
            }

            return base.CreateContract(objectType);
        }
    }
}
