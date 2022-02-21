using System;
using System.Collections.Generic;
using System.IO;
using SSF.Timing.Tools.OrganizationDb.Writers;

namespace SSF.Timing.Tools.OrganizationDb;

public class PersonWriterRepository
{
	private static readonly Lazy<PersonWriterRepository> s_singleton = new Lazy<PersonWriterRepository>(() => new PersonWriterRepository(), isThreadSafe: true);

	private readonly List<IPersonWriterFactory> m_writerFactories = new List<IPersonWriterFactory>();

	private PersonWriterRepository()
	{
		m_writerFactories.Add(new SQLitePersonWriter.Factory());
		m_writerFactories.Add(new ExcelPersonWriter.Factory());
	}

	internal static bool TryGetWriter(FileInfo fi, out IPersonWriter writer)
	{
		foreach (IPersonWriterFactory f in s_singleton.Value.m_writerFactories)
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
