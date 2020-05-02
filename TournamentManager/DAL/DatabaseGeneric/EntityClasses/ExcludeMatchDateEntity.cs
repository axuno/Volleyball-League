﻿//////////////////////////////////////////////////////////////
// <auto-generated>This code was generated by LLBLGen Pro 5.5.</auto-generated>
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
	/// <summary>Entity class which represents the entity 'ExcludeMatchDate'.<br/><br/></summary>
	[Serializable]
	public partial class ExcludeMatchDateEntity : CommonEntityBase
		// __LLBLGENPRO_USER_CODE_REGION_START AdditionalInterfaces
		// __LLBLGENPRO_USER_CODE_REGION_END	
	{
		private RoundEntity _round;
		private TeamEntity _team;
		private TournamentEntity _tournament;

		// __LLBLGENPRO_USER_CODE_REGION_START PrivateMembers
		// __LLBLGENPRO_USER_CODE_REGION_END
		private static ExcludeMatchDateEntityStaticMetaData _staticMetaData = new ExcludeMatchDateEntityStaticMetaData();
		private static ExcludeMatchDateRelations _relationsFactory = new ExcludeMatchDateRelations();

		/// <summary>All names of fields mapped onto a relation. Usable for in-memory filtering</summary>
		public static partial class MemberNames
		{
			/// <summary>Member name Round</summary>
			public static readonly string Round = "Round";
			/// <summary>Member name Team</summary>
			public static readonly string Team = "Team";
			/// <summary>Member name Tournament</summary>
			public static readonly string Tournament = "Tournament";
		}

		/// <summary>Static meta-data storage for navigator related information</summary>
		protected class ExcludeMatchDateEntityStaticMetaData : EntityStaticMetaDataBase
		{
			public ExcludeMatchDateEntityStaticMetaData()
			{
				SetEntityCoreInfo("ExcludeMatchDateEntity", InheritanceHierarchyType.None, false, (int)TournamentManager.DAL.EntityType.ExcludeMatchDateEntity, typeof(ExcludeMatchDateEntity), typeof(ExcludeMatchDateEntityFactory), false);
				AddNavigatorMetaData<ExcludeMatchDateEntity, RoundEntity>("Round", "ExcludeMatchDates", (a, b) => a._round = b, a => a._round, (a, b) => a.Round = b, TournamentManager.DAL.RelationClasses.StaticExcludeMatchDateRelations.RoundEntityUsingRoundIdStatic, ()=>new ExcludeMatchDateRelations().RoundEntityUsingRoundId, null, new int[] { (int)ExcludeMatchDateFieldIndex.RoundId }, null, true, (int)TournamentManager.DAL.EntityType.RoundEntity);
				AddNavigatorMetaData<ExcludeMatchDateEntity, TeamEntity>("Team", "ExcludeMatchDates", (a, b) => a._team = b, a => a._team, (a, b) => a.Team = b, TournamentManager.DAL.RelationClasses.StaticExcludeMatchDateRelations.TeamEntityUsingTeamIdStatic, ()=>new ExcludeMatchDateRelations().TeamEntityUsingTeamId, null, new int[] { (int)ExcludeMatchDateFieldIndex.TeamId }, null, true, (int)TournamentManager.DAL.EntityType.TeamEntity);
				AddNavigatorMetaData<ExcludeMatchDateEntity, TournamentEntity>("Tournament", "ExcludeMatchDates", (a, b) => a._tournament = b, a => a._tournament, (a, b) => a.Tournament = b, TournamentManager.DAL.RelationClasses.StaticExcludeMatchDateRelations.TournamentEntityUsingTournamentIdStatic, ()=>new ExcludeMatchDateRelations().TournamentEntityUsingTournamentId, null, new int[] { (int)ExcludeMatchDateFieldIndex.TournamentId }, null, true, (int)TournamentManager.DAL.EntityType.TournamentEntity);
			}
		}

		/// <summary>Static ctor</summary>
		static ExcludeMatchDateEntity()
		{
		}

		/// <summary> CTor</summary>
		public ExcludeMatchDateEntity()
		{
			InitClassEmpty(null, null);
		}

		/// <summary> CTor</summary>
		/// <param name="fields">Fields object to set as the fields for this entity.</param>
		public ExcludeMatchDateEntity(IEntityFields2 fields)
		{
			InitClassEmpty(null, fields);
		}

		/// <summary> CTor</summary>
		/// <param name="validator">The custom validator object for this ExcludeMatchDateEntity</param>
		public ExcludeMatchDateEntity(IValidator validator)
		{
			InitClassEmpty(validator, null);
		}

		/// <summary> CTor</summary>
		/// <param name="id">PK value for ExcludeMatchDate which data should be fetched into this ExcludeMatchDate object</param>
		public ExcludeMatchDateEntity(System.Int64 id) : this(id, null)
		{
		}

		/// <summary> CTor</summary>
		/// <param name="id">PK value for ExcludeMatchDate which data should be fetched into this ExcludeMatchDate object</param>
		/// <param name="validator">The custom validator object for this ExcludeMatchDateEntity</param>
		public ExcludeMatchDateEntity(System.Int64 id, IValidator validator)
		{
			InitClassEmpty(validator, null);
			this.Id = id;
		}

		/// <summary>Private CTor for deserialization</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ExcludeMatchDateEntity(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			// __LLBLGENPRO_USER_CODE_REGION_START DeserializationConstructor
			// __LLBLGENPRO_USER_CODE_REGION_END
		}

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'Round' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoRound() { return CreateRelationInfoForNavigator("Round"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'Team' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoTeam() { return CreateRelationInfoForNavigator("Team"); }

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'Tournament' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoTournament() { return CreateRelationInfoForNavigator("Tournament"); }
		
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
		/// <param name="validator">The validator object for this ExcludeMatchDateEntity</param>
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
		public static ExcludeMatchDateRelations Relations { get { return _relationsFactory; } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Round' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathRound { get { return _staticMetaData.GetPrefetchPathElement("Round", CommonEntityBase.CreateEntityCollection<RoundEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Team' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathTeam { get { return _staticMetaData.GetPrefetchPathElement("Team", CommonEntityBase.CreateEntityCollection<TeamEntity>()); } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Tournament' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathTournament { get { return _staticMetaData.GetPrefetchPathElement("Tournament", CommonEntityBase.CreateEntityCollection<TournamentEntity>()); } }

		/// <summary>The Id property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."Id".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, true, true</remarks>
		public virtual System.Int64 Id
		{
			get { return (System.Int64)GetValue((int)ExcludeMatchDateFieldIndex.Id, true); }
			set { SetValue((int)ExcludeMatchDateFieldIndex.Id, value); }		}

		/// <summary>The TournamentId property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."TournamentId".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Int64 TournamentId
		{
			get { return (System.Int64)GetValue((int)ExcludeMatchDateFieldIndex.TournamentId, true); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.TournamentId, value); }
		}

		/// <summary>The RoundId property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."RoundId".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): true, false, false</remarks>
		public virtual Nullable<System.Int64> RoundId
		{
			get { return (Nullable<System.Int64>)GetValue((int)ExcludeMatchDateFieldIndex.RoundId, false); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.RoundId, value); }
		}

		/// <summary>The TeamId property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."TeamId".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): true, false, false</remarks>
		public virtual Nullable<System.Int64> TeamId
		{
			get { return (Nullable<System.Int64>)GetValue((int)ExcludeMatchDateFieldIndex.TeamId, false); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.TeamId, value); }
		}

		/// <summary>The DateFrom property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."DateFrom".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime DateFrom
		{
			get { return (System.DateTime)GetValue((int)ExcludeMatchDateFieldIndex.DateFrom, true); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.DateFrom, value); }
		}

		/// <summary>The DateTo property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."DateTo".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime DateTo
		{
			get { return (System.DateTime)GetValue((int)ExcludeMatchDateFieldIndex.DateTo, true); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.DateTo, value); }
		}

		/// <summary>The Reason property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."Reason".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 60.<br/>Table field behavior characteristics (is nullable, is PK, is identity): true, false, false</remarks>
		public virtual System.String Reason
		{
			get { return (System.String)GetValue((int)ExcludeMatchDateFieldIndex.Reason, true); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.Reason, value); }
		}

		/// <summary>The CreatedOn property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."CreatedOn".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime CreatedOn
		{
			get { return (System.DateTime)GetValue((int)ExcludeMatchDateFieldIndex.CreatedOn, true); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.CreatedOn, value); }
		}

		/// <summary>The ModifiedOn property of the Entity ExcludeMatchDate<br/><br/></summary>
		/// <remarks>Mapped on  table field: "ExcludeMatchDate"."ModifiedOn".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime ModifiedOn
		{
			get { return (System.DateTime)GetValue((int)ExcludeMatchDateFieldIndex.ModifiedOn, true); }
			set	{ SetValue((int)ExcludeMatchDateFieldIndex.ModifiedOn, value); }
		}

		/// <summary>Gets / sets related entity of type 'RoundEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(false)]
		public virtual RoundEntity Round
		{
			get { return _round; }
			set { SetSingleRelatedEntityNavigator(value, "Round"); }
		}

		/// <summary>Gets / sets related entity of type 'TeamEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(false)]
		public virtual TeamEntity Team
		{
			get { return _team; }
			set { SetSingleRelatedEntityNavigator(value, "Team"); }
		}

		/// <summary>Gets / sets related entity of type 'TournamentEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(false)]
		public virtual TournamentEntity Tournament
		{
			get { return _tournament; }
			set { SetSingleRelatedEntityNavigator(value, "Tournament"); }
		}

		// __LLBLGENPRO_USER_CODE_REGION_START CustomEntityCode
		// __LLBLGENPRO_USER_CODE_REGION_END

	}
}

namespace TournamentManager.DAL
{
	public enum ExcludeMatchDateFieldIndex
	{
		///<summary>Id. </summary>
		Id,
		///<summary>TournamentId. </summary>
		TournamentId,
		///<summary>RoundId. </summary>
		RoundId,
		///<summary>TeamId. </summary>
		TeamId,
		///<summary>DateFrom. </summary>
		DateFrom,
		///<summary>DateTo. </summary>
		DateTo,
		///<summary>Reason. </summary>
		Reason,
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
	/// <summary>Implements the relations factory for the entity: ExcludeMatchDate. </summary>
	public partial class ExcludeMatchDateRelations: RelationFactory
	{

		/// <summary>Returns a new IEntityRelation object, between ExcludeMatchDateEntity and RoundEntity over the m:1 relation they have, using the relation between the fields: ExcludeMatchDate.RoundId - Round.Id</summary>
		public virtual IEntityRelation RoundEntityUsingRoundId
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "Round", false, new[] { RoundFields.Id, ExcludeMatchDateFields.RoundId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between ExcludeMatchDateEntity and TeamEntity over the m:1 relation they have, using the relation between the fields: ExcludeMatchDate.TeamId - Team.Id</summary>
		public virtual IEntityRelation TeamEntityUsingTeamId
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "Team", false, new[] { TeamFields.Id, ExcludeMatchDateFields.TeamId }); }
		}

		/// <summary>Returns a new IEntityRelation object, between ExcludeMatchDateEntity and TournamentEntity over the m:1 relation they have, using the relation between the fields: ExcludeMatchDate.TournamentId - Tournament.Id</summary>
		public virtual IEntityRelation TournamentEntityUsingTournamentId
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "Tournament", false, new[] { TournamentFields.Id, ExcludeMatchDateFields.TournamentId }); }
		}

	}
	
	/// <summary>Static class which is used for providing relationship instances which are re-used internally for syncing</summary>
	internal static class StaticExcludeMatchDateRelations
	{
		internal static readonly IEntityRelation RoundEntityUsingRoundIdStatic = new ExcludeMatchDateRelations().RoundEntityUsingRoundId;
		internal static readonly IEntityRelation TeamEntityUsingTeamIdStatic = new ExcludeMatchDateRelations().TeamEntityUsingTeamId;
		internal static readonly IEntityRelation TournamentEntityUsingTournamentIdStatic = new ExcludeMatchDateRelations().TournamentEntityUsingTournamentId;

		/// <summary>CTor</summary>
		static StaticExcludeMatchDateRelations() { }
	}
}