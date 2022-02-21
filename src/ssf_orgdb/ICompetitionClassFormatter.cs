namespace SSF.Timing.Tools.OrganizationDb;

public interface ICompetitionClassFormatter
{
	string GetName(CompetitionClass c);

	CompetitionClass Parse(string str);
}
