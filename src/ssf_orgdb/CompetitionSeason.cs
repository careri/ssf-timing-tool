using System;

namespace SSF.Timing.Tools.OrganizationDb;

public class CompetitionSeason
{
	private readonly DateTime m_created;

	private readonly ICompetitionClassFormatter m_formatter;

	public int FromYear { get; }

	public int ToYear { get; }

	public string MalePrefix { get; internal set; }

	public string FemalePrefix { get; internal set; }

	public string Name { get; internal set; }

	public CompetitionSeason()
		: this(DateTime.Now, null)
	{
	}

	public CompetitionSeason(DateTime created)
		: this(created, null)
	{
	}

	public CompetitionSeason(DateTime created, ICompetitionClassFormatter formatter)
	{
		m_formatter = formatter ?? CompetitionClassFormatter.Swedish;
		DateTime now = DateTime.Now;
		m_created = created;
		DateTime seasonBreak = new DateTime(created.Year, 7, 1);
		bool isNextSeason = created >= seasonBreak;
		ToYear = (isNextSeason ? (created.Year + 1) : created.Year);
		FromYear = ToYear - 1;
		Name = $"{FromYear}/{ToYear}";
	}

	public string GetClassName(Gender sex, DateTime dateOfBirth)
	{
		return GetClass(sex, dateOfBirth).Name;
	}

	public CompetitionClass GetClass(Gender sex, DateTime dateOfBirth)
	{
		int ageTo = ToYear - dateOfBirth.Year;
		int ageFrom = ((ageTo > 8) ? ageTo : 0);
		return new CompetitionClass(sex, ageFrom, ageTo, m_formatter);
	}

	internal static CompetitionSeason Parse(string season)
	{
		if (int.TryParse(season, out var year))
		{
			return new CompetitionSeason(new DateTime(year, 1, 1));
		}
		return null;
	}
}
