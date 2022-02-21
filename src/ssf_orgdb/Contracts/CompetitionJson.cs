using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SSF.Timing.Tools.OrganizationDb.Contracts;

[DataContract]
public class CompetitionJson : DataContractBase<CompetitionJson>, ICompetition, IAutoMapAble
{
	[DataMember]
	public string Id { get; set; }

	[DataMember]
	public DateTime RaceDate { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string Sector { get; set; }

	[DataMember]
	public string Season { get; set; }

	[DataMember]
	public string GeographicalScope { get; set; }

	[DataMember]
	public string EventType { get; set; }

	[DataMember]
	public string Place { get; set; }

	[DataMember]
	public string Website { get; set; }

	[DataMember(Name = "Organizer")]
	public EntityNameIDJson OrganizerRef { get; set; }

	public IEntityNameID Organizer
	{
		get
		{
			return OrganizerRef;
		}
		set
		{
			OrganizerRef = MapHelper.Map<EntityNameIDJson>(value);
		}
	}

	[DataMember(Name = "Groups")]
	public List<CompetitionGroupJson> GroupsJsons { get; set; }

	public List<ICompetitionGroup> Groups
	{
		get
		{
			List<CompetitionGroupJson> competitors = GroupsJsons;
			if (competitors != null)
			{
				return new List<ICompetitionGroup>(competitors);
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				List<CompetitionGroupJson> list = new List<CompetitionGroupJson>(value.Count);
				foreach (ICompetitionGroup p in value)
				{
					CompetitionGroupJson pj = ((!(p is CompetitionGroupJson)) ? MapHelper.Map<CompetitionGroupJson>(p) : ((CompetitionGroupJson)p));
					list.Add(pj);
				}
				GroupsJsons = list;
			}
			else
			{
				GroupsJsons = null;
			}
		}
	}
}
