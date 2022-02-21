using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SSF.Timing.Tools.OrganizationDb.Contracts;
using SSF.Timing.Tools.OrganizationDb.Models;

namespace SSF.Timing.Tools.OrganizationDb;

internal class MapDefinitions
{
	internal static void Init()
	{
		Mapper.Initialize(DoInit);
	}

	private static void DoInit(IMapperConfigurationExpression cfg)
	{
		InitPersonDataToJson(cfg);
		InitICompetitionToJson(cfg);
		InitICompetitionGroupsToJson(cfg);
		cfg.CreateMap<EntityNameIDData, EntityNameIDJson>();
		cfg.CreateMap<EntityNameIDJson, EntityNameIDData>();
	}

	private static void InitICompetitionToJson(IMapperConfigurationExpression cfg)
	{
		IMappingExpression<ICompetition, CompetitionJson> mappingExpression = cfg.CreateMap<ICompetition, CompetitionJson>();
		mappingExpression.ForMember((CompetitionJson x) => x.OrganizerRef, delegate(IMemberConfigurationExpression<ICompetition, CompetitionJson, EntityNameIDJson> mcfg)
		{
			mcfg.Ignore();
		});
		mappingExpression.ForMember((CompetitionJson x) => x.GroupsJsons, delegate(IMemberConfigurationExpression<ICompetition, CompetitionJson, List<CompetitionGroupJson>> mcfg)
		{
			mcfg.Ignore();
		});
	}

	private static void InitICompetitionGroupsToJson(IMapperConfigurationExpression cfg)
	{
		IMappingExpression<ICompetitionGroup, CompetitionGroupJson> mappingExpression = cfg.CreateMap<ICompetitionGroup, CompetitionGroupJson>();
		mappingExpression.ForMember((CompetitionGroupJson x) => x.CompetitorJsons, delegate(IMemberConfigurationExpression<ICompetitionGroup, CompetitionGroupJson, List<PersonJson>> mcfg)
		{
			mcfg.Ignore();
		});
	}

	private static void InitPersonDataToJson(IMapperConfigurationExpression cfg)
	{
		IMappingExpression<PersonData, PersonJson> mappingExpression = cfg.CreateMap<PersonData, PersonJson>();
		mappingExpression.ForMember((PersonJson x) => x.NationRef, delegate(IMemberConfigurationExpression<PersonData, PersonJson, EntityNameIDJson> mcfg)
		{
			mcfg.Ignore();
		});
		mappingExpression.ForMember((PersonJson x) => x.OrganizationRef, delegate(IMemberConfigurationExpression<PersonData, PersonJson, EntityNameIDJson> mcfg)
		{
			mcfg.Ignore();
		});
		mappingExpression.ForMember((PersonJson x) => x.DateOfBirthStr, delegate(IMemberConfigurationExpression<PersonData, PersonJson, string> mcfg)
		{
			mcfg.Ignore();
		});
		mappingExpression.ForMember((PersonJson x) => x.StreamEncoding, delegate(IMemberConfigurationExpression<PersonData, PersonJson, Encoding> mcfg)
		{
			mcfg.Ignore();
		});
	}
}
