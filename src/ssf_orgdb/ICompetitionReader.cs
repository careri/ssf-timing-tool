using System;

namespace SSF.Timing.Tools.OrganizationDb;

public interface ICompetitionReader : IReaderWriter, IDisposable
{
	ICompetition Get();
}
