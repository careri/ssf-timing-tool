using System;
using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb;

public interface IPersonWriter : IReaderWriter, IDisposable
{
	void Write(IEnumerable<IPerson> persons);

	void Write(IPerson p);
}
