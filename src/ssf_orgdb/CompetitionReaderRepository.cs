using System;
using System.Collections.Generic;
using System.IO;
using SSF.Timing.Tools.OrganizationDb.Readers;

namespace SSF.Timing.Tools.OrganizationDb;

public class CompetitionReaderRepository
{
	private static readonly Lazy<CompetitionReaderRepository> s_singleton = new Lazy<CompetitionReaderRepository>(() => new CompetitionReaderRepository(), isThreadSafe: true);

	private readonly List<ICompetitionReaderFactory> m_readerFactories = new List<ICompetitionReaderFactory>();

	private CompetitionReaderRepository()
	{
		m_readerFactories.Add(new ExcelPersonCompetitionReader.Factory());
	}

	internal static bool TryGetReader(FileInfo fi, out ICompetitionReader reader)
	{
		foreach (ICompetitionReaderFactory f in s_singleton.Value.m_readerFactories)
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
