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
	/// <summary>Entity class which represents the entity 'IdentityUserLogin'.<br/><br/></summary>
	[Serializable]
	public partial class IdentityUserLoginEntity : CommonEntityBase
		// __LLBLGENPRO_USER_CODE_REGION_START AdditionalInterfaces
		// __LLBLGENPRO_USER_CODE_REGION_END	
	{
		private UserEntity _user;

		// __LLBLGENPRO_USER_CODE_REGION_START PrivateMembers
		// __LLBLGENPRO_USER_CODE_REGION_END
		private static IdentityUserLoginEntityStaticMetaData _staticMetaData = new IdentityUserLoginEntityStaticMetaData();
		private static IdentityUserLoginRelations _relationsFactory = new IdentityUserLoginRelations();

		/// <summary>All names of fields mapped onto a relation. Usable for in-memory filtering</summary>
		public static partial class MemberNames
		{
			/// <summary>Member name User</summary>
			public static readonly string User = "User";
		}

		/// <summary>Static meta-data storage for navigator related information</summary>
		protected class IdentityUserLoginEntityStaticMetaData : EntityStaticMetaDataBase
		{
			public IdentityUserLoginEntityStaticMetaData()
			{
				SetEntityCoreInfo("IdentityUserLoginEntity", InheritanceHierarchyType.None, false, (int)TournamentManager.DAL.EntityType.IdentityUserLoginEntity, typeof(IdentityUserLoginEntity), typeof(IdentityUserLoginEntityFactory), false);
				AddNavigatorMetaData<IdentityUserLoginEntity, UserEntity>("User", "IdentityUserLogins", (a, b) => a._user = b, a => a._user, (a, b) => a.User = b, TournamentManager.DAL.RelationClasses.StaticIdentityUserLoginRelations.UserEntityUsingUserIdStatic, ()=>new IdentityUserLoginRelations().UserEntityUsingUserId, null, new int[] { (int)IdentityUserLoginFieldIndex.UserId }, null, true, (int)TournamentManager.DAL.EntityType.UserEntity);
			}
		}

		/// <summary>Static ctor</summary>
		static IdentityUserLoginEntity()
		{
		}

		/// <summary> CTor</summary>
		public IdentityUserLoginEntity()
		{
			InitClassEmpty(null, null);
		}

		/// <summary> CTor</summary>
		/// <param name="fields">Fields object to set as the fields for this entity.</param>
		public IdentityUserLoginEntity(IEntityFields2 fields)
		{
			InitClassEmpty(null, fields);
		}

		/// <summary> CTor</summary>
		/// <param name="validator">The custom validator object for this IdentityUserLoginEntity</param>
		public IdentityUserLoginEntity(IValidator validator)
		{
			InitClassEmpty(validator, null);
		}

		/// <summary> CTor</summary>
		/// <param name="loginProvider">PK value for IdentityUserLogin which data should be fetched into this IdentityUserLogin object</param>
		/// <param name="providerKey">PK value for IdentityUserLogin which data should be fetched into this IdentityUserLogin object</param>
		/// <param name="userId">PK value for IdentityUserLogin which data should be fetched into this IdentityUserLogin object</param>
		public IdentityUserLoginEntity(System.String loginProvider, System.String providerKey, System.Int64 userId) : this(loginProvider, providerKey, userId, null)
		{
		}

		/// <summary> CTor</summary>
		/// <param name="loginProvider">PK value for IdentityUserLogin which data should be fetched into this IdentityUserLogin object</param>
		/// <param name="providerKey">PK value for IdentityUserLogin which data should be fetched into this IdentityUserLogin object</param>
		/// <param name="userId">PK value for IdentityUserLogin which data should be fetched into this IdentityUserLogin object</param>
		/// <param name="validator">The custom validator object for this IdentityUserLoginEntity</param>
		public IdentityUserLoginEntity(System.String loginProvider, System.String providerKey, System.Int64 userId, IValidator validator)
		{
			InitClassEmpty(validator, null);
			this.LoginProvider = loginProvider;
			this.ProviderKey = providerKey;
			this.UserId = userId;
		}

		/// <summary>Private CTor for deserialization</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected IdentityUserLoginEntity(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			// __LLBLGENPRO_USER_CODE_REGION_START DeserializationConstructor
			// __LLBLGENPRO_USER_CODE_REGION_END
		}

		/// <summary>Method which will construct a filter (predicate expression) for the unique constraint defined on the fields: Id .</summary>
		/// <returns>true if succeeded and the contents is read, false otherwise</returns>
		public IPredicateExpression ConstructFilterForUCId()
		{
			var filter = new PredicateExpression();
			filter.Add(TournamentManager.DAL.HelperClasses.IdentityUserLoginFields.Id == this.Fields.GetCurrentValue((int)IdentityUserLoginFieldIndex.Id));
 			return filter;
		}

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'User' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoUser() { return CreateRelationInfoForNavigator("User"); }
		
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
		/// <param name="validator">The validator object for this IdentityUserLoginEntity</param>
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
		public static IdentityUserLoginRelations Relations { get { return _relationsFactory; } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'User' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathUser { get { return _staticMetaData.GetPrefetchPathElement("User", CommonEntityBase.CreateEntityCollection<UserEntity>()); } }

		/// <summary>The Id property of the Entity IdentityUserLogin<br/><br/></summary>
		/// <remarks>Mapped on  table field: "IdentityUserLogin"."Id".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, true</remarks>
		public virtual System.Int64 Id
		{
			get { return (System.Int64)GetValue((int)IdentityUserLoginFieldIndex.Id, true); }
		}

		/// <summary>The LoginProvider property of the Entity IdentityUserLogin<br/><br/></summary>
		/// <remarks>Mapped on  table field: "IdentityUserLogin"."LoginProvider".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 128.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, true, false</remarks>
		public virtual System.String LoginProvider
		{
			get { return (System.String)GetValue((int)IdentityUserLoginFieldIndex.LoginProvider, true); }
			set	{ SetValue((int)IdentityUserLoginFieldIndex.LoginProvider, value); }
		}

		/// <summary>The ProviderDisplayName property of the Entity IdentityUserLogin<br/><br/></summary>
		/// <remarks>Mapped on  table field: "IdentityUserLogin"."ProviderDisplayName".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 128.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.String ProviderDisplayName
		{
			get { return (System.String)GetValue((int)IdentityUserLoginFieldIndex.ProviderDisplayName, true); }
			set	{ SetValue((int)IdentityUserLoginFieldIndex.ProviderDisplayName, value); }
		}

		/// <summary>The ProviderKey property of the Entity IdentityUserLogin<br/><br/></summary>
		/// <remarks>Mapped on  table field: "IdentityUserLogin"."ProviderKey".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 128.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, true, false</remarks>
		public virtual System.String ProviderKey
		{
			get { return (System.String)GetValue((int)IdentityUserLoginFieldIndex.ProviderKey, true); }
			set	{ SetValue((int)IdentityUserLoginFieldIndex.ProviderKey, value); }
		}

		/// <summary>The UserId property of the Entity IdentityUserLogin<br/><br/></summary>
		/// <remarks>Mapped on  table field: "IdentityUserLogin"."UserId".<br/>Table field type characteristics (type, precision, scale, length): BigInt, 19, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, true, false</remarks>
		public virtual System.Int64 UserId
		{
			get { return (System.Int64)GetValue((int)IdentityUserLoginFieldIndex.UserId, true); }
			set	{ SetValue((int)IdentityUserLoginFieldIndex.UserId, value); }
		}

		/// <summary>Gets / sets related entity of type 'UserEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(false)]
		public virtual UserEntity User
		{
			get { return _user; }
			set { SetSingleRelatedEntityNavigator(value, "User"); }
		}

		// __LLBLGENPRO_USER_CODE_REGION_START CustomEntityCode
		// __LLBLGENPRO_USER_CODE_REGION_END

	}
}

namespace TournamentManager.DAL
{
	public enum IdentityUserLoginFieldIndex
	{
		///<summary>Id. </summary>
		Id,
		///<summary>LoginProvider. </summary>
		LoginProvider,
		///<summary>ProviderDisplayName. </summary>
		ProviderDisplayName,
		///<summary>ProviderKey. </summary>
		ProviderKey,
		///<summary>UserId. </summary>
		UserId,
		/// <summary></summary>
		AmountOfFields
	}
}

namespace TournamentManager.DAL.RelationClasses
{
	/// <summary>Implements the relations factory for the entity: IdentityUserLogin. </summary>
	public partial class IdentityUserLoginRelations: RelationFactory
	{

		/// <summary>Returns a new IEntityRelation object, between IdentityUserLoginEntity and UserEntity over the m:1 relation they have, using the relation between the fields: IdentityUserLogin.UserId - User.Id</summary>
		public virtual IEntityRelation UserEntityUsingUserId
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "User", false, new[] { UserFields.Id, IdentityUserLoginFields.UserId }); }
		}

	}
	
	/// <summary>Static class which is used for providing relationship instances which are re-used internally for syncing</summary>
	internal static class StaticIdentityUserLoginRelations
	{
		internal static readonly IEntityRelation UserEntityUsingUserIdStatic = new IdentityUserLoginRelations().UserEntityUsingUserId;

		/// <summary>CTor</summary>
		static StaticIdentityUserLoginRelations() { }
	}
}
