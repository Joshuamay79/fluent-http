﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using AonWeb.FluentHttp.HAL.Representations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AonWeb.FluentHttp.HAL.Serialization
{
    public class HalResourceConverter : JsonConverter
    {
        public HalResourceConverter() : this(null) { }

        public HalResourceConverter(Type type)
        {
            ObjectType = type;
        }

        protected Type ObjectType { get; set; }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = value as IHalResource;

            if (resource == null)
                return;

            writer.WriteStartObject();

            var embeds = new List<Tuple<HalEmbeddedAttribute, MemberInfo>>();

            foreach (var memberInfo in GetMembers(value.GetType()))
            {
                var embeddedAttribute = memberInfo.GetCustomAttributes(true).OfType<HalEmbeddedAttribute>().FirstOrDefault();

                if (embeddedAttribute != null)
                {
                    embeds.Add(new Tuple<HalEmbeddedAttribute, MemberInfo>(embeddedAttribute, memberInfo));
                }
                else if (memberInfo.Name == "Links")
                {
                    WriteLinks(writer, resource.Links);

                }
                else
                {
                    if (memberInfo is PropertyInfo && !((PropertyInfo)memberInfo).CanRead)
                        continue;

                    var propertyName = GetPropertyName(value.GetType(), memberInfo, serializer.ContractResolver as CamelCasePropertyNamesContractResolver);
                    var propertyValue = GetPropertyValue(value, memberInfo);

                    writer.WritePropertyName(propertyName);

                    if (propertyValue == null)
                        writer.WriteNull();
                    else
                        serializer.Serialize(writer, propertyValue);
                }
            }

            WriteEmbedded(writer, embeds, value, serializer);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            objectType = ObjectType ?? objectType;
            var json = JObject.Load(reader);

            var hasJsonPropEmbedded = GetMembers(objectType)
                .Select(p => p.GetCustomAttribute<JsonPropertyAttribute>())
                    .Any(a => a != null && a.PropertyName == "_embedded");

            JToken embedded = null;

            if (!hasJsonPropEmbedded)
            {
                if (json.TryGetValue("_embedded", out embedded))
                    json.Remove("_embedded");
            }

            JToken links;
            if (json.TryGetValue("_links", out links))
                json.Remove("_links");

            object resource;

            try
            {
                resource = Activator.CreateInstance(objectType);
            }
            catch (Exception ex)
            {
                throw SerializationErrorHelper.CreateError(reader, string.Format("Could not create HalResource object. Type: {0}", objectType.Name), ex);
            }

            serializer.Populate(json.CreateReader(), resource);

            TryPopulateLinks(reader, objectType, serializer, links, resource);

            TryPopulateEmbedded(embedded, objectType, serializer, resource);


            return resource;
        }


        private static void WriteEmbedded(JsonWriter writer, IList<Tuple<HalEmbeddedAttribute, MemberInfo>> attributesAndProperties, object parentValue, JsonSerializer serializer)
        {
            if (attributesAndProperties.Count == 0)
                return;

            writer.WritePropertyName("_embedded");

            writer.WriteStartObject();

            foreach (var attributesAndProperty in attributesAndProperties)
            {
                var memberInfo = attributesAndProperty.Item2;

                if (memberInfo is PropertyInfo && !((PropertyInfo)memberInfo).CanRead)
                    continue;

                var embeddedAttribute = attributesAndProperty.Item1;
                var embedValue = GetPropertyValue(parentValue, memberInfo);

                writer.WritePropertyName(embeddedAttribute.Rel);

                if (embedValue == null)
                    writer.WriteNull();
                else
                    serializer.Serialize(writer, embedValue, embeddedAttribute.Type ?? embedValue.GetType());

            }

            writer.WriteEndObject();
        }

        private static void WriteLinks(JsonWriter writer, IEnumerable<HyperMediaLink> links)
        {
            writer.WritePropertyName("_links");

            if (links != null)
            {
                writer.WriteStartObject();

                foreach (var link in links)
                {
                    writer.WritePropertyName(link.Rel);
                    writer.WriteStartObject();
                    writer.WritePropertyName("href");
                    writer.WriteValue(link.Href);
                    writer.WritePropertyName("templated");
                    writer.WriteValue(link.IsTemplated);
                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
        }

        private static IEnumerable<MemberInfo> GetMembers(Type type)
        {
            return type.GetProperties().Cast<MemberInfo>().Concat(type.GetFields());
        }

        private static string GetPropertyName(Type declaringType, MemberInfo memberInfo, DefaultContractResolver resolver)
        {
            var dataContractAttribute = declaringType.GetCustomAttribute<DataContractAttribute>(true);

            var dataMemberAttribute = dataContractAttribute != null ? memberInfo.GetCustomAttribute<DataMemberAttribute>(true) : null;

            var propertyAttribute = memberInfo.GetCustomAttribute<JsonPropertyAttribute>(true);

            string name;
            if (propertyAttribute != null && propertyAttribute.PropertyName != null)
                name = propertyAttribute.PropertyName;
            else if (dataMemberAttribute != null && dataMemberAttribute.Name != null)
                name = dataMemberAttribute.Name;
            else
                name = memberInfo.Name;

            return resolver != null ? resolver.GetResolvedPropertyName(name) : name;
        }

        private static object GetPropertyValue(object value, MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(value);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(value);
            }

            return null;
        }

        private static void SetValue(object parent, object value, MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)memberInfo).SetValue(parent, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)memberInfo).SetValue(parent, value);
                    break;
            }
        }

        public static Type GetUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
            }

            return null;
        }
        
        private static void TryPopulateLinks(
            JsonReader reader,
            Type objectType,
            JsonSerializer serializer,
            JToken links,
            object resource)
        {
            if (links == null)
                return;

            var linkProperty = objectType.GetProperty("Links");

            if (linkProperty == null)
                throw SerializationErrorHelper.CreateError(reader, string.Format("Could not create HyperMediaLinks object. Could not find property 'Links' on object of type {0}", objectType.Name));

            var linkListType = linkProperty.PropertyType;

            if (!typeof(IList<HyperMediaLink>).IsAssignableFrom(linkListType))
                throw SerializationErrorHelper.CreateError(reader, string.Format("Could not create HyperMediaLinks object. Links property type '{0}' on type '{1}' is not assignable to IList<HyperMediaLink>", linkListType.Name, objectType.Name));

            IList<HyperMediaLink> list;

            try
            {
                list = (IList<HyperMediaLink>)Activator.CreateInstance(linkListType);
            }
            catch (Exception ex)
            {
                throw SerializationErrorHelper.CreateError(reader, string.Format("Could not create HyperMediaLinks object. Type: {0}", linkListType.Name), ex);
            }

            var enumerator = ((JObject)links).GetEnumerator();

            while (enumerator.MoveNext())
            {
                var link = new HyperMediaLink { Rel = enumerator.Current.Key };
                serializer.Populate(enumerator.Current.Value.CreateReader(), link);
                list.Add(link);
            }

            linkProperty.SetValue(resource, list);
        }

        private static void TryPopulateEmbedded(JToken embedded, Type objectType, JsonSerializer serializer, object resource)
        {
            if (embedded == null)
                return;

            if (embedded.Type == JTokenType.Array)
            {
                foreach (var item in ((JArray)embedded))
                    TryPopulateEmbedded(item, objectType, serializer, resource);
            }
            else
            {
                var enumerator = ((JObject)embedded).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var rel = enumerator.Current.Key;

                    foreach (var memberInfo in GetMembers(objectType))
                    {
                        var attribute = memberInfo.GetCustomAttributes(true).OfType<HalEmbeddedAttribute>().FirstOrDefault(attr => attr.Rel == rel);

                        if (attribute == null)
                            continue;

                        var type = attribute.Type ?? GetUnderlyingType(memberInfo);

                        var propValue = enumerator.Current.Value.ToObject(type, serializer);

                        SetValue(resource, propValue, memberInfo);
                    }
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IHalResource).IsAssignableFrom(objectType);
        }
    }
}