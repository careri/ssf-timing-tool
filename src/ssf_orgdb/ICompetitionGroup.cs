using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb;

public interface ICompetitionGroup : IAutoMapAble
{
	IEntityNameID Class { get; set; }

	IEntityNameID Discipline { get; set; }

	IEntityNameID Category { get; set; }

	CompetitionStart StartType { get; set; }

	List<IPerson> Competitors { get; set; }
}
