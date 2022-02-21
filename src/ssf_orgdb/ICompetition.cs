using System;
using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb;

public interface ICompetition : IAutoMapAble
{
	string Id { get; set; }

	DateTime RaceDate { get; set; }

	string Name { get; set; }

	string Sector { get; set; }

	string Season { get; set; }

	string GeographicalScope { get; set; }

	string EventType { get; set; }

	string Place { get; set; }

	string Website { get; set; }

	IEntityNameID Organizer { get; set; }

	List<ICompetitionGroup> Groups { get; set; }
}
