using System;
using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb;

public interface IPersonReader : IReaderWriter, IDisposable
{
	IEnumerable<IPerson> Get();
}
