using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Vernuntii.VersionPersistence.Presentation.Serializers
{
    internal class VersionPresentationYamlSerializer : IVersionPresentationSerializer
    {
        public static readonly VersionPresentationYamlSerializer Default = new();

        public string SerializeSemanticVersion(
            IVersionCache versionCache,
            VersionPresentationKind presentationKind,
            VersionPresentationParts presentationParts)
        {
            var versionCacheConverter = new VersionCacheYamlConverter(presentationKind, presentationParts);

            var builder = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithTypeConverter(versionCacheConverter);

            versionCacheConverter.Serializer = builder.BuildValueSerializer();
            return builder.Build().Serialize(versionCache).TrimEnd();
        }

        private class VersionCacheYamlConverter : IYamlTypeConverter
        {
            private static readonly Type s_acceptableVersionCacheType = typeof(IVersionCache);

            internal IValueSerializer? Serializer { get; set; }

            private readonly VersionPresentationKind _presentationKind;
            private readonly VersionPresentationParts _presentationParts;

            public VersionCacheYamlConverter(VersionPresentationKind presentationKind, VersionPresentationParts presentationParts)
            {
                _presentationKind = presentationKind;
                _presentationParts = presentationParts;
            }

            public bool Accepts(Type type) =>
                type.IsAssignableTo(s_acceptableVersionCacheType);

            public object? ReadYaml(IParser parser, Type type) =>
                throw new NotImplementedException();

            public void WriteYaml(IEmitter emitter, object? untypedVersionCache, Type type)
            {
                if (Serializer is null) {
                    throw new InvalidOperationException("The serializer has not been property-injected");
                }

                var versionCache = untypedVersionCache as IVersionCache ?? throw new InvalidOperationException($"Value of type {s_acceptableVersionCacheType} was expected");

                if (_presentationKind == VersionPresentationKind.Value) {
                    var singlePart = _presentationParts.AssertSingleDueToValueKind();
                    _ = versionCache.TryGetData(singlePart, out var value, out var valueType);
                    Serializer.SerializeValue(emitter, value!, valueType);
                } else {
                    emitter.Emit(new MappingStart());

                    foreach (var presentationPart in _presentationParts) {
                        VersionPresentationSerializerHelpers.GetData(versionCache, presentationPart, out var data, out var dataType);
                        emitter.Emit(new Scalar(presentationPart.Name));
                        Serializer.SerializeValue(emitter, data, dataType);
                    }

                    emitter.Emit(new MappingEnd());
                }
            }
        }
    }
}
