using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SSF.Timing.Tools.OrganizationDb.Contracts;

[DataContract]
public class CompetitionGroupJson : DataContractBase<CompetitionGroupJson>, ICompetitionGroup, IAutoMapAble
{
	[DataMember(Name = "Class")]
	public EntityNameIDJson ClassRef { get; set; }

	public IEntityNameID Class
	{
		get
		{
			return ClassRef;
		}
		set
		{
			ClassRef = MapHelper.Map<EntityNameIDJson>(value);
		}
	}

	[DataMember(Name = "Discipline")]
	public EntityNameIDJson DisciplineRef { get; set; }

	public IEntityNameID Discipline
	{
		get
		{
			return DisciplineRef;
		}
		set
		{
			DisciplineRef = MapHelper.Map<EntityNameIDJson>(value);
		}
	}

	[DataMember(Name = "Category")]
	public EntityNameIDJson CategoryRef { get; set; }

	public IEntityNameID Category
	{
		get
		{
			return CategoryRef;
		}
		set
		{
			CategoryRef = MapHelper.Map<EntityNameIDJson>(value);
		}
	}

	[DataMember]
	public CompetitionStart StartType { get; set; }

	[DataMember(Name = "Competitors")]
	public List<PersonJson> CompetitorJsons { get; set; }

	public List<IPerson> Competitors
	{
		get
		{
			return ConvertList<PersonJson, IPerson>(CompetitorJsons);
		}
		set
		{
			CompetitorJsons = AutoMapList<IPerson, PersonJson>(value);
		}
	}
}
