using System;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb;

public interface IReaderWriter : IDisposable
{
	FlagsCollection Flags { get; set; }
}
