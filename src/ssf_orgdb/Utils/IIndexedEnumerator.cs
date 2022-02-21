using System;
using System.Collections;
using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb.Utils;

public interface IIndexedEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	int Index { get; }
}
