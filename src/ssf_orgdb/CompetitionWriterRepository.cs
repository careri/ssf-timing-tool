using System;
using System.Collections.Generic;
using System.IO;
using SSF.Timing.Tools.OrganizationDb.Writers;

namespace SSF.Timing.Tools.OrganizationDb;

public class CompetitionWriterRepository
{
	private static readonly Lazy<CompetitionWriterRepository> s_singleton = new Lazy<CompetitionWriterRepository>(() => new CompetitionWriterRepository(), isThreadSafe: true);

	private readonly List<ICompetitionWriterFactory> m_writerFactories = new List<ICompetitionWriterFactory>();

	private CompetitionWriterRepository()
	{
		m_writerFactories.Add(new JsonCompetitionWriter.Factory());
		m_writerFactories.Add(new SSFCompetitionWriter.Factory());
	}

	internal static bool TryGetWriter(FileInfo fi, out ICompetitionWriter writer)
	{
		foreach (ICompetitionWriterFactory f in s_singleton.Value.m_writerFactories)
		{
			if (f.TryGet(fi, out writer))
			{
				return true;
			}
		}
		writer = null;
		return false;
	}
}
