using System.Collections.Generic;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb;

public interface ICommandArgs
{
	IReadOnlyCollection<string> Args { get; }

	FlagsCollection Flags { get; }

	bool IsValid { get; }

	string Name { get; }
}
