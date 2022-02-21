using System;
using System.Collections.Generic;
using System.IO;
using SSF.Timing.Tools.OrganizationDb.Readers;

namespace SSF.Timing.Tools.OrganizationDb;

public class PersonReaderRepository
{
	private static readonly Lazy<PersonReaderRepository> s_singleton = new Lazy<PersonReaderRepository>(() => new PersonReaderRepository(), isThreadSafe: true);

	private readonly List<IPersonReaderFactory> m_readerFactories = new List<IPersonReaderFactory>();

	private PersonReaderRepository()
	{
		m_readerFactories.Add(new ExcelPersonReader.Factory());
		m_readerFactories.Add(new SSFPersonXmlReader.Factory());
		m_readerFactories.Add(new SQLitePersonReader.Factory());
	}

	internal static bool TryGetReader(FileInfo fi, out IPersonReader reader)
	{
		foreach (IPersonReaderFactory f in s_singleton.Value.m_readerFactories)
		{
			if (f.TryGet(fi, out reader))
			{
				return true;
			}
		}
		reader = null;
		return false;
	}
}
