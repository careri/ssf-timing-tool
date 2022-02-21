using System;

namespace SSF.Timing.Tools.OrganizationDb;

public interface ICompetitionWriter : IReaderWriter, IDisposable
{
	void Write(ICompetition c);
}
