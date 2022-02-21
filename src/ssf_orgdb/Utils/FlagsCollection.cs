using System.Collections;
using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb.Utils;

public class FlagsCollection : IEnumerable<string>, IEnumerable
{
	private readonly HashSet<string> m_flags = new HashSet<string>();

	public bool this[string index] => m_flags.Contains(index?.ToLowerInvariant()?.Trim());

	public IEnumerator<string> GetEnumerator()
	{
		return m_flags.GetEnumerator();
	}

	internal void Update(IEnumerable<string> value)
	{
		m_flags.Clear();
		if (value == null)
		{
			return;
		}
		foreach (string item in value)
		{
			Add(item);
		}
	}

	public void Add(string item)
	{
		string key = item?.ToLowerInvariant()?.Trim();
		if (key != null && key.Length > 0)
		{
			m_flags.Add(key);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_flags.GetEnumerator();
	}
}
