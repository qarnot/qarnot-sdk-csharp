using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QarnotSDK
{
    internal class HardwareConstraintsJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HardwareConstraint) || objectType.IsSubclassOf(typeof(HardwareConstraint));
        }
        public override bool CanWrite => false;

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override Object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                var jo = JObject.Load(reader);
                var discriminatorField = jo.Value<string>(nameof(HardwareConstraint.Discriminator).ToLower()) ??
                    jo.Value<string>(nameof(HardwareConstraint.Discriminator));
                switch (discriminatorField)
                {
                    case HardwareConstraintDiscriminators.MinimumCoreHardwareConstraint:
                        return jo.ToObject<MinimumCoreHardware>();
                    case HardwareConstraintDiscriminators.MaximumCoreHardwareConstraint:
                        return jo.ToObject<MaximumCoreHardware>();
                    case HardwareConstraintDiscriminators.MinimumRamCoreRatioHardwareConstraint:
                     return jo.ToObject<MinimumRamCoreRatioHardware>();
                    case HardwareConstraintDiscriminators.MaximumRamCoreRatioHardwareConstraint:
                        return jo.ToObject<MaximumRamCoreRatioHardware>();
                    case HardwareConstraintDiscriminators.SpecificHardwareConstraint:
                        return jo.ToObject<SpecificHardware>();
                    case HardwareConstraintDiscriminators.MinimumRamHardwareConstraint:
                        return jo.ToObject<MinimumRamHardware>();
                    case HardwareConstraintDiscriminators.MaximumRamHardwareConstraint:
                        return jo.ToObject<MaximumRamHardware>();
                    case HardwareConstraintDiscriminators.GpuHardwareConstraint:
                        return jo.ToObject<GpuHardware>();
                    default: return default;
                };
            } catch (Exception ex) {
                Console.WriteLine($"Failed to deserialize Hardware constraints: {ex.Message}", ex);
            }

            return default;
        }
    }
}