namespace SSF.Timing.Tools.OrganizationDb;

public class CompetitionClass
{
	private readonly ICompetitionClassFormatter m_formatter;

	public Gender Sex { get; }

	public int AgeFrom { get; }

	public int AgeTo { get; }

	public string Name => m_formatter.GetName(this);

	public CompetitionClass(Gender sex, int ageFrom, int ageTo, ICompetitionClassFormatter formatter)
	{
		Sex = sex;
		AgeFrom = ageFrom;
		AgeTo = ageTo;
		m_formatter = formatter ?? CompetitionClassFormatter.Swedish;
	}

	public override string ToString()
	{
		return Name;
	}
}
