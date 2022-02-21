using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Contracts;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Readers;

internal class ExcelPersonCompetitionReader : ICompetitionReader, IReaderWriter, IDisposable
{
	public class Factory : ICompetitionReaderFactory, IReaderWriterFactory<ICompetitionReader>
	{
		public bool TryGet(FileInfo fi, out ICompetitionReader reader)
		{
			if (fi.Extension.StartsWith(".xls", StringComparison.InvariantCultureIgnoreCase) && TryOpen(fi, out var xlsReader))
			{
				reader = xlsReader;
				return true;
			}
			reader = null;
			return false;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(ExcelPersonCompetitionReader).Name);

	private readonly string m_path;

	private readonly IPersonReader m_personReader;

	public FlagsCollection Flags { get; set; }

	private ExcelPersonCompetitionReader(FileInfo fi, IPersonReader personReader)
	{
		m_path = fi.FullName;
		m_personReader = personReader;
	}

	private static bool TryOpen(FileInfo fi, out ExcelPersonCompetitionReader reader)
	{
		try
		{
			if (PersonReaderRepository.TryGetReader(fi, out var personReader))
			{
				reader = new ExcelPersonCompetitionReader(fi, personReader);
				return true;
			}
			reader = null;
			return false;
		}
		catch (Exception ex)
		{
			s_log.Error(fi.FullName, ex);
			reader = null;
			return false;
		}
	}

	public ICompetition Get()
	{
		try
		{
			return DoGet();
		}
		catch (Exception ex)
		{
			s_log.Error(m_path, ex);
			throw;
		}
	}

	private ICompetition DoGet()
	{
		DateTime now = DateTime.Now;
		long nowTicks = DateTime.Now.Ticks;
		IEnumerable<IPerson> persons = m_personReader.Get();
		CompetitionSeason season = new CompetitionSeason();
		return new CompetitionJson
		{
			Name = "Serietävling",
			EventType = "Tävling",
			Sector = "CC",
			Season = season.ToYear.ToString(),
			GeographicalScope = "Klubb (Lokalt)",
			Id = nowTicks.ToString(),
			Organizer = new EntityNameIDJson
			{
				ID = "12563",
				Name = "Korsnäs IF SK"
			},
			Place = "Pumpen, Hosjö, Falun",
			Website = "http://www.korsnasifsk.se",
			RaceDate = now,
			Groups = CreateGroups(persons)
		};
	}

	private List<ICompetitionGroup> CreateGroups(IEnumerable<IPerson> persons)
	{
		IGrouping<string, IPerson>[] grouped = (from p in persons
			group p by p.Class).ToArray();
		List<ICompetitionGroup> list = new List<ICompetitionGroup>(grouped.Length);
		IGrouping<string, IPerson>[] array = grouped;
		foreach (IGrouping<string, IPerson> grp in array)
		{
			CompetitionGroupJson grpJson = new CompetitionGroupJson
			{
				Category = new EntityNameIDJson
				{
					ID = "Ung",
					Name = "Ungdom"
				},
				Class = new EntityNameIDJson
				{
					Name = grp.Key
				},
				Discipline = new EntityNameIDJson
				{
					Name = "Längdåkning"
				},
				Competitors = grp.ToList(),
				StartType = CompetitionStart.Interval
			};
			list.Add(grpJson);
		}
		return list;
	}

	public void Dispose()
	{
		m_personReader.Dispose();
	}
}
