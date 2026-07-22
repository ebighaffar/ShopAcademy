using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Mongo;

/// <summary>
/// Registers BSON class maps for domain entities stored in MongoDB. Called once at
/// startup — <see cref="BsonClassMap"/> registration is process-wide and must not
/// run twice, hence the idempotency guard.
/// </summary>
public static class MongoMappingConfig
{
    private static bool _registered;
    private static readonly Lock Lock = new();

    public static void Register()
    {
        lock (Lock)
        {
            if (_registered)
                return;

            RegisterExceptionLog();
            _registered = true;
        }
    }

    private static void RegisterExceptionLog()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(ExceptionLog)))
            return;

        BsonClassMap.RegisterClassMap<ExceptionLog>(map =>
        {
            // AutoMap picks up the private parameterless constructor and the
            // properties' private setters, so no explicit creator is needed.
            map.AutoMap();

            // Id is a string in the domain but stored as a native ObjectId. The
            // StringObjectIdGenerator lets the driver generate the value on insert,
            // so nothing in the app has to assign ids.
            map.MapIdMember(x => x.Id)
                .SetIdGenerator(StringObjectIdGenerator.Instance)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));

            map.SetIgnoreExtraElements(true);
        });
    }
}