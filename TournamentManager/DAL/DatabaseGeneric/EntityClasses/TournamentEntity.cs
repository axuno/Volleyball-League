﻿//////////////////////////////////////////////////////////////
// <auto-generated>This code was generated by LLBLGen Pro 5.11.</auto-generated>
//////////////////////////////////////////////////////////////
// Code is generated on: 
// Code is generated using templates: SD.TemplateBindings.SharedTemplates
// Templates vendor: Solutions Design.
//////////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.RelationClasses;

using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.DAL.EntityClasses
{
	// __LLBLGENPRO_USER_CODE_REGION_START AdditionalNamespaces
	// __LLBLGENPRO_USER_CODE_REGION_END
	/// <summary>Entity class which represents the entity 'Tournament'.<br/><br/></summary>
	[Serializable]
	public partial class TournamentEntity : CommonEntityBase
		// __LLBLGENPRO_USER_CODE_REGION_START AdditionalInterfaces
		// __LLBLGENPRO_USER_CODE_REGION_END	
	{
		private EntityCollection<ExcludeMatchDateEntity> _excludeMatchDates;
		private EntityCollection<RankingEntity> _rankings;
		private EntityCollection<RegistrationEntity> _registrations;
		private EntityCollection<RoundEntity> _rounds;
		private EntityCollection<TournamentEntity> _tournaments;
		private TournamentEntity _tournament;
		private TournamentTypeEntity _tournamentType;

		// __LLBLGENPRO_USER_CODE_REGION_START PrivateMembers
		// __LLBLGENPRO_USER_CODE_REGION_END
		private static TournamentEntityStaticMetaData _staticMetaData = new TournamentEntityStaticMetaData();
		private static TournamentRelations _relationsFactory = new TournamentRelations();

		/// <summary>All names of fields mapped onto a relation. Usable for in-memory filtering</summary>
		public static partial class MemberNames
		{
			/// <summary>Member name Tournament</summary>
			public static readonly string Tournament = "Tournament";
			/// <summary>Member name TournamentType</summary>
			public static readonly string TournamentType = "TournamentType";
			/// <summary>Member name ExcludeMatchDates</summary>
			public static readonly string ExcludeMatchDates = "ExcludeMatchDates";
			/// <summary>Member name Rankings</summary>
			public static readonly string Rankings = "Rankings";
			/// <summary>Member name Registrations</summary>
			public static readonly string Registrations = "Registrations";
			/// <summary>Member name Rounds</summary>
			public static readonly string Rounds = "Rounds";
			/// <summary>Member name Tournaments</summary>
			public static readonly string Tournaments = "Tournaments";
		}

		/// <summary>Static meta-data storage for navigator related information</summary>
		protected class TournamentEntityStaticMetaData : EntityStaticMetaDataBase
		{
			public TournamentEntityStaticMetaData()
			{
				SetEntityCoreInfo("TournamentEntity", InheritanceHierarchyType.None, false, (int)TournamentManager.DAL.EntityType.TournamentEntity, typeof(TournamentEntity), typeof(TournamentEntityFactory), false);
				AddNavigatorMetaData<TournamentEntity, EntityCollection<ExcludeMatchDateEntity>>("ExcludeMatchDates", a => a._excludeMatchDates, (a, b) => a._excludeMatchDates = b, a => a.ExcludeMatchDates, () => new TournamentRelations().ExcludeMatchDateEntityUsingTournamentId, typeof(ExcludeMatchDateEntity), (int)TournamentManager.DAL.EntityType.ExcludeMatchDateEntity);
				AddNavigatorMetaData<TournamentEntity, EntityCollection<RankingEntity>>("Rankings", a => a._rankings, (a, b) => a._rankings = b, a => a.Rankings, () => new TournamentRelations().RankingEntityUsingTournamentId, typeof(RankingEntity), (int)TournamentManager.DAL.EntityType.RankingEntity);
				AddNavigatorMetaData<TournamentEntity, EntityCollection<RegistrationEntity>>("Registrations", a => a._registrations, (a, b) => a._registrations = b, a => a.Registrations, () => new TournamentRelations().RegistrationEntityUsingTournamentId, typeof(RegistrationEntity), (int)TournamentManager.DAL.EntityType.RegistrationEntity);
				AddNavigatorMetaData<TournamentEntity, EntityCollection<RoundEntity>>("Rounds", a => a._rounds, (a, b) => a._rounds = b, a => a.Rounds, () => new TournamentRelations().RoundEntityUsingTournamentId, typeof(RoundEntity), (int)TournamentManager.DAL.EntityType.RoundEntity);
				AddNavigatorMetaData<TournamentEntity, EntityCollection<TournamentEntity>>("Tournaments", a => a._tournaments, (a, b) => a._tournaments = b, a => a.Tournaments, () => new TournamentRelations().TournamentEntityUsingNextTournamentId, typeof(TournamentEntity), (int)TournamentManager.DAL.EntityType.TournamentEntity);
				AddNavigatorMetaData<TournamentEntity, TournamentEntity>("Tournament", "Tournaments", (a, b) => a._tournament = b, a => a._tournament, (a, b) => a.Tournament = b, TournamentManager.DAL.RelationClasses.StaticTournamentRelations.TournamentEntityUsingIdNextTournamentIdStatic, ()=>new TournamentRelations().TournamentEntityUsingIdNextTournamentId, null, new int[] { (int)TournamentFieldIndex.NextTournamentId }, null, true, (int)TournamentManager.DAL.EntityType.TournamentEntity);
				AddNavigatorMetaData<TournamentEntity, TournamentTypeEntity>("TournamentType", "Tournaments", (a, b) => a._tournamentType = b, a => a._tournamentType, (a, b) => a.TournamentType = b, TournamentManager.DAL.RelationClasses.StaticTournamentRelations.TournamentTypeEntityUsingTypeIdStatic, ()=>new TournamentRelations().TournamentTypeEntityUsingTypeId, null, new int[] { (int)TournamentFieldIndex.TypeId }, null, true, (int)TournamentManager.DAL.EntityType.TournamentTypeEntity);
			}
		}

		/// <summary>Static ctor</summary>
		static TournamentEntity()
		{
		}

		/// <summary> CTor</summary>
		public TournamentEntity()
		{
			InitClassEmpty(null, null);
		}

		/// <summary> CTor</summary>
		/// <param name="fields">Fields object to set as the fields for this entity.</param>
		public TournamentEntity(IEntityFields2 fields)
		{
			InitClassEmpty(null, fields);
		}

		/// <summary> CTor</summary>
		/// <param name="validator">The custom validator object for this TournamentEntity</param>
		public TournamentEntity(IValidator validator)
		{
			InitClassEmpty(validator, null);
		}

		/// <summary> CTor</summary>
		/// <param name="id">PK value for Tournament which data should be fetched into this Tournament object</param>
		public TournamentEntity(System.Int64 id) : this(id, null)
		{
		}

		/// <summary> CTor</summary>
		/// <param name="id">PK value for Tournament which data should be fetched into this Tournament object</param>
		/// <param name="validator">The custom validator object for this TournamentEntity</param>
		public TournamentEntity(System.Int64 id, IValidator validator)
		{
			InitClassEmpty(validator, null);
			this.Id = id;
		}

		/// <summary>Private CTor for deserialization</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected TournamentEntity(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			// __LLBLGENPRO_USER_CODE_REGION_START DeserializationConstructor
			// __LLBLGENPRO_USER_CODE_REGION_END
		}

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entities of type 'ExcludeMatchDate' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoExcludeMatchDates() { return CreateRelationInfoForNavigator("ExcludeMatchDates"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entities of type 'Ranking' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoRankings() { return CreateRelationInfoForNavigator("Rankings"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entities of type 'Registration' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoRegistrations() { return CreateRelationInfoForNavigator("Registrations"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entities of type 'Round' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoRounds() { return CreateRelationInfoForNavigator("Rounds"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entities of type 'Tournament' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoTournaments() { return CreateRelationInfoForNavigator("Tournaments"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'Tournament' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoTournament() { return CreateRelationInfoForNavigator("Tournament"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'TournamentType' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoTournamentType() { return CreateRelationInfoForNavigator("TournamentType"); }
		
		/// <inheritdoc/>
		protected override EntityStaticMetaDataBase GetEntityStaticMetaData() {	return _staticMetaData; }

		/// <summary>Initializes the class members</summary>
		private void InitClassMembers()
		{
			PerformDependencyInjection();
			// __LLBLGENPRO_USER_CODE_REGION_START InitClassMembers
			// __LLBLGENPRO_USER_CODE_REGION_END
			OnInitClassMembersComplete();
		}

		/// <summary>Initializes the class with empty data, as if it is a new Entity.</summary>
		/// <param name="validator">The validator object for this TournamentEntity</param>
		/// <param name="fields">Fields of this entity</param>
		private void InitClassEmpty(IValidator validator, IEntityFields2 fields)
		{
			OnInitializing();
			this.Fields = fields ?? CreateFields();
			this.Validator = validator;
			InitClassMembers();
			// __LLBLGENPRO_USER_CODE_REGION_START InitClassEmpty
			// __LLBLGENPRO_USER_CODE_REGION_END

			OnInitialized();
		}

		/// <summary>The relations object holding all relations of this entity with other entity classes.</summary>
		public static TournamentRelations Relations { get { return _relationsFactory; } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'ExcludeMatchDate' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathExcludeMatchDates { get { return _staticMetaData.GetPrefetchPathElement("ExcludeMatchDates", CommonEntityBase.CreateEntityCollection<ExcludeMatchDateEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Ranking' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathRankings { get { return _staticMetaData.GetPrefetchPathElement("Rankings", CommonEntityBase.CreateEntityCollection<RankingEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Registration' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathRegistrations { get { return _staticMetaData.GetPrefetchPathElement("Registrations", CommonEntityBase.CreateEntityCollection<RegistrationEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Round' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathRounds { get { return _staticMetaData.GetPrefetchPathElement("Rounds", CommonEntityBase.CreateEntityCollection<RoundEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Tournament' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathTournaments { get { return _staticMetaData.GetPrefetchPathElement("Tournaments", CommonEntityBase.CreateEntityCollection<TournamentEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Tournament' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathTournament { get { return _staticMetaData.GetPrefetchPathElement("Tournament", CommonEntityBase.CreateEntityCollection<TournamentEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'TournamentType' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathTournamentType { get { return _staticMetaData.GetPrefetchPathElement("TournamentType", CommonEntityBase.CreateEntityCollection<TournamentTypeEntity>()); } }

		/// <summary>The Id property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."Id".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, true, true</remarks>
		public virtual System.Int64 Id
		{
			get { return (System.Int64)GetValue((int)TournamentFieldIndex.Id, true); }
			set { SetValue((int)TournamentFieldIndex.Id, value); }
		}

		/// <summary>The Name property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."Name".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 255.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.String Name
		{
			get { return (System.String)GetValue((int)TournamentFieldIndex.Name, true); }
			set { SetValue((int)TournamentFieldIndex.Name, value); }
		}

		/// <summary>The Description property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."Description".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 255.<br/>Table field behavior characteristics (is nullable, is PK, is identity): true, false, false</remarks>
		public virtual System.String Description
		{
			get { return (System.String)GetValue((int)TournamentFieldIndex.Description, true); }
			set { SetValue((int)TournamentFieldIndex.Description, value); }
		}

		/// <summary>The TypeId property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."TypeId".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): true, false, false</remarks>
		public virtual Nullable<System.Int64> TypeId
		{
			get { return (Nullable<System.Int64>)GetValue((int)TournamentFieldIndex.TypeId, false); }
			set { SetValue((int)TournamentFieldIndex.TypeId, value); }
		}

		/// <summary>The IsComplete property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."IsComplete".<br/>Table field type characteristics (type, precision, scale, length): Bit, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Boolean IsComplete
		{
			get { return (System.Boolean)GetValue((int)TournamentFieldIndex.IsComplete, true); }
			set { SetValue((int)TournamentFieldIndex.IsComplete, value); }
		}

		/// <summary>The IsPlanningMode property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."IsPlanningMode".<br/>Table field type characteristics (type, precision, scale, length): Bit, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Boolean IsPlanningMode
		{
			get { return (System.Boolean)GetValue((int)TournamentFieldIndex.IsPlanningMode, true); }
			set { SetValue((int)TournamentFieldIndex.IsPlanningMode, value); }
		}

		/// <summary>The NextTournamentId property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."NextTournamentId".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): true, false, false</remarks>
		public virtual Nullable<System.Int64> NextTournamentId
		{
			get { return (Nullable<System.Int64>)GetValue((int)TournamentFieldIndex.NextTournamentId, false); }
			set { SetValue((int)TournamentFieldIndex.NextTournamentId, value); }
		}

		/// <summary>The CreatedOn property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."CreatedOn".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime CreatedOn
		{
			get { return (System.DateTime)GetValue((int)TournamentFieldIndex.CreatedOn, true); }
			set { SetValue((int)TournamentFieldIndex.CreatedOn, value); }
		}

		/// <summary>The ModifiedOn property of the Entity Tournament<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Tournament"."ModifiedOn".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime ModifiedOn
		{
			get { return (System.DateTime)GetValue((int)TournamentFieldIndex.ModifiedOn, true); }
			set { SetValue((int)TournamentFieldIndex.ModifiedOn, value); }
		}

		/// <summary>Gets the EntityCollection with the related entities of type 'ExcludeMatchDateEntity' which are related to this entity via a relation of type '1:n'. If the EntityCollection hasn't been fetched yet, the collection returned will be empty.<br/><br/></summary>
		[TypeContainedAttribute(typeof(ExcludeMatchDateEntity))]
		public virtual EntityCollection<ExcludeMatchDateEntity> ExcludeMatchDates { get { return GetOrCreateEntityCollection<ExcludeMatchDateEntity, ExcludeMatchDateEntityFactory>("Tournament", true, false, ref _excludeMatchDates); } }

		/// <summary>Gets the EntityCollection with the related entities of type 'RankingEntity' which are related to this entity via a relation of type '1:n'. If the EntityCollection hasn't been fetched yet, the collection returned will be empty.<br/><br/></summary>
		[TypeContainedAttribute(typeof(RankingEntity))]
		public virtual EntityCollection<RankingEntity> Rankings { get { return GetOrCreateEntityCollection<RankingEntity, RankingEntityFactory>("Tournament", true, false, ref _rankings); } }

		/// <summary>Gets the EntityCollection with the related entities of type 'RegistrationEntity' which are related to this entity via a relation of type '1:n'. If the EntityCollection hasn't been fetched yet, the collection returned will be empty.<br/><br/></summary>
		[TypeContainedAttribute(typeof(RegistrationEntity))]
		public virtual EntityCollection<RegistrationEntity> Registrations { get { return GetOrCreateEntityCollection<RegistrationEntity, RegistrationEntityFactory>("Tournament", true, false, ref _registrations); } }

		/// <summary>Gets the EntityCollection with the related entities of type 'RoundEntity' which are related to this entity via a relation of type '1:n'. If the EntityCollection hasn't been fetched yet, the collection returned will be empty.<br/><br/></summary>
		[TypeContainedAttribute(typeof(RoundEntity))]
		public virtual EntityCollection<RoundEntity> Rounds { get { return GetOrCreateEntityCollection<RoundEntity, RoundEntityFactory>("Tournament", true, false, ref _rounds); } }

		/// <summary>Gets the EntityCollection with the related entities of type 'TournamentEntity' which are related to this entity via a relation of type '1:n'. If the EntityCollection hasn't been fetched yet, the collection returned will be empty.<br/><br/></summary>
		[TypeContainedAttribute(typeof(TournamentEntity))]
		public virtual EntityCollection<TournamentEntity> Tournaments { get { return GetOrCreateEntityCollection<TournamentEntity, TournamentEntityFactory>("Tournament", true, false, ref _tournaments); } }

		/// <summary>Gets / sets related entity of type 'TournamentEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(false)]
		public virtual TournamentEntity Tournament
		{
			get { return _tournament; }
			set { SetSingleRelatedEntityNavigator(value, "Tournament"); }
		}

		/// <summary>Gets / sets related entity of type 'TournamentTypeEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(false)]
		public virtual TournamentTypeEntity TournamentType
		{
			get { return _tournamentType; }
			set { SetSingleRelatedEntityNavigator(value, "TournamentType"); }
		}

		// __LLBLGENPRO_USER_CODE_REGION_START CustomEntityCode
		// __LLBLGENPRO_USER_CODE_REGION_END

	}
}

namespace TournamentManager.DAL
{
	public enum TournamentFieldIndex
	{
		///<summary>Id. </summary>
		Id,
		///<summary>Name. </summary>
		Name,
		///<summary>Description. </summary>
		Description,
		///<summary>TypeId. </summary>
		TypeId,
		///<summary>IsComplete. </summary>
		IsComplete,
		///<summary>IsPlanningMode. </summary>
		IsPlanningMode,
		///<summary>NextTournamentId. </summary>
		NextTournamentId,
		///<summary>CreatedOn. </summary>
		CreatedOn,
		///<summary>ModifiedOn. </summary>
		ModifiedOn,
		/// <summary></summary>
		AmountOfFields
	}
}

namespace TournamentManager.DAL.RelationClasses
{
	/// <summary>Implements the relations factory for the entity: Tournament. </summary>
	public partial class TournamentRelations: RelationFactory
	{
		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and ExcludeMatchDateEntity over the 1:n relation they have, using the relation between the fields: Tournament.Id - ExcludeMatchDate.TournamentId</summary>
		public virtual IEntityRelation ExcludeMatchDateEntityUsingTournamentId
		{
			get { return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.OneToMany, "ExcludeMatchDates", true, new[] { TournamentFields.Id, ExcludeMatchDateFields.TournamentId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and RankingEntity over the 1:n relation they have, using the relation between the fields: Tournament.Id - Ranking.TournamentId</summary>
		public virtual IEntityRelation RankingEntityUsingTournamentId
		{
			get { return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.OneToMany, "Rankings", true, new[] { TournamentFields.Id, RankingFields.TournamentId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and RegistrationEntity over the 1:n relation they have, using the relation between the fields: Tournament.Id - Registration.TournamentId</summary>
		public virtual IEntityRelation RegistrationEntityUsingTournamentId
		{
			get { return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.OneToMany, "Registrations", true, new[] { TournamentFields.Id, RegistrationFields.TournamentId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and RoundEntity over the 1:n relation they have, using the relation between the fields: Tournament.Id - Round.TournamentId</summary>
		public virtual IEntityRelation RoundEntityUsingTournamentId
		{
			get { return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.OneToMany, "Rounds", true, new[] { TournamentFields.Id, RoundFields.TournamentId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and TournamentEntity over the 1:n relation they have, using the relation between the fields: Tournament.Id - Tournament.NextTournamentId</summary>
		public virtual IEntityRelation TournamentEntityUsingNextTournamentId
		{
			get { return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.OneToMany, "Tournaments", true, new[] { TournamentFields.Id, TournamentFields.NextTournamentId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and TournamentEntity over the m:1 relation they have, using the relation between the fields: Tournament.NextTournamentId - Tournament.Id</summary>
		public virtual IEntityRelation TournamentEntityUsingIdNextTournamentId
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "Tournament", false, new[] { TournamentFields.Id, TournamentFields.NextTournamentId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between TournamentEntity and TournamentTypeEntity over the m:1 relation they have, using the relation between the fields: Tournament.TypeId - TournamentType.Id</summary>
		public virtual IEntityRelation TournamentTypeEntityUsingTypeId
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "TournamentType", false, new[] { TournamentTypeFields.Id, TournamentFields.TypeId }); }
		}

	}
	
	/// <summary>Static class which is used for providing relationship instances which are re-used internally for syncing</summary>
	internal static class StaticTournamentRelations
	{
		internal static readonly IEntityRelation ExcludeMatchDateEntityUsingTournamentIdStatic = new TournamentRelations().ExcludeMatchDateEntityUsingTournamentId;
		internal static readonly IEntityRelation RankingEntityUsingTournamentIdStatic = new TournamentRelations().RankingEntityUsingTournamentId;
		internal static readonly IEntityRelation RegistrationEntityUsingTournamentIdStatic = new TournamentRelations().RegistrationEntityUsingTournamentId;
		internal static readonly IEntityRelation RoundEntityUsingTournamentIdStatic = new TournamentRelations().RoundEntityUsingTournamentId;
		internal static readonly IEntityRelation TournamentEntityUsingNextTournamentIdStatic = new TournamentRelations().TournamentEntityUsingNextTournamentId;
		internal static readonly IEntityRelation TournamentEntityUsingIdNextTournamentIdStatic = new TournamentRelations().TournamentEntityUsingIdNextTournamentId;
		internal static readonly IEntityRelation TournamentTypeEntityUsingTypeIdStatic = new TournamentRelations().TournamentTypeEntityUsingTypeId;

		/// <summary>CTor</summary>
		static StaticTournamentRelations() { }
	}
}
