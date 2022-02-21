namespace SSF.Timing.Tools.OrganizationDb;

public interface IGenderFormatter
{
	string GetName(Gender c);

	Gender? Parse(string str);
}
