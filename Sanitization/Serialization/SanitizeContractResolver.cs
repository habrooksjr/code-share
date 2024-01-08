using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System;
using dvcsharp_core_api.Sanitization.Attributes;

namespace dvcsharp_core_api.Sanitization.Serialization
{
    public class SanitizeContractResolver : DefaultContractResolver
    {
        public bool Isgnored(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(SanitizeIgnoreAttribute));
        }

        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = (member as PropertyInfo);
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            jsonProperty.ShouldSerialize = x => !Isgnored(property);
            return jsonProperty;
        }
    }
}
