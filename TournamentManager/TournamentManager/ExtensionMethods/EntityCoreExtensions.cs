using System.Runtime.Serialization;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// Extension methods for types implementing <see cref="IEntityCore"/>.
/// </summary>
public static class EntityCoreExtensions
{
    public static void CloneEntity<T>(this IEntityCore sourceObject, out T targetObject) where T : class, IEntityCore
    {
        var ms = new MemoryStream();
        var bf = new DataContractSerializer(typeof(T));
        bf.WriteObject(ms, sourceObject);
        ms.Seek(0, SeekOrigin.Begin);
        targetObject = (T) bf.ReadObject(ms)!;
        ms.Close();
        ms.Dispose();
    }

    public static void ResetEntityAsNew(this IEntityCore entity) 
    {
        entity.IsNew = true;
        entity.IsDirty = true;
        entity.Fields.IsDirty = true;
        for (var f = 0; f < entity.Fields.Count; f++)
        {
            entity.Fields[f].IsChanged = true;
        }
    }
}
