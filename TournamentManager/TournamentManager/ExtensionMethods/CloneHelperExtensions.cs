using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.ExtensionMethods
{
	/// <summary>
	/// For cloning an Entity and all related entities, i.e. the whole graph
	/// </summary>
	public static class CloneHelperExtensions
	{
		static CloneHelperExtensions()
		{
		}

		public static void CloneEntity<T>(T sourceObject, out T targetObject) where T : class, IEntityCore
		{
			var ms = new MemoryStream();
			var bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
			bf.Serialize(ms, sourceObject);
			ms.Seek(0, SeekOrigin.Begin);
			targetObject = (T) bf.Deserialize(ms);
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
        /*
		public static T CloneEntity<T>(this T entity, bool asNew = false) where T: class, IEntityCore
		{
			T newEntity;
			CloneEntity(entity, out newEntity);

			if (asNew)
			{
				var ogu = new ObjectGraphUtils();
				var flatList = ogu.ProduceTopologyOrderedList(newEntity);
				foreach (var t in flatList)
					ResetEntityAsNew(t);
			}

			return newEntity;
		}*/
	}
}
