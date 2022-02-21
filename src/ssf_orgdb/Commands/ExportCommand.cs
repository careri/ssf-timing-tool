using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Commands;

internal class ExportCommand : IDisposable
{
	private static readonly ILog s_log = LogManager.GetLogger(typeof(ExportCommand).Name);

	private readonly string m_path;

	private readonly IPersonWriter m_writer;

	private readonly IPersonReader m_reader;

	private ExportCommand(FileInfo fi, IPersonWriter writer, IPersonReader reader)
	{
		m_path = fi.FullName;
		m_writer = writer;
		m_reader = reader;
	}

	internal static void Execute(ICommandArgs args)
	{
		try
		{
			FileInfo fi = args.FirstAsFile();
			if (fi != null)
			{
				if (!PersonWriterRepository.TryGetWriter(fi, out var writer))
				{
					throw new NotSupportedException(fi.FullName);
				}
				FileInfo dbFI = WellKnown.SqliteFI;
				s_log.Debug(dbFI.FullName + " -> " + fi.FullName);
				if (PersonReaderRepository.TryGetReader(dbFI, out var reader))
				{
					using (ExportCommand exportCmd = new ExportCommand(fi, writer, reader))
					{
						exportCmd.Export();
						return;
					}
				}
				writer.Dispose();
			}
			else
			{
				s_log.Error("Missing import file as arg1");
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
		}
	}

	public void Dispose()
	{
		m_writer.Dispose();
		m_reader.Dispose();
	}

	private void Export()
	{
		m_writer.Write(AddClass(m_reader.Get()));
	}

	private IEnumerable<IPerson> AddClass(IEnumerable<IPerson> persons)
	{
		if (persons == null)
		{
			yield break;
		}
		_ = DateTime.Now;
		CompetitionSeason compSeason = new CompetitionSeason
		{
			MalePrefix = "H ",
			FemalePrefix = "D "
		};
		foreach (IPerson p in persons)
		{
			DateOfBirth dob = p.DateOfBirth;
			if (dob.Type == DateOfBirth.DateType.Date)
			{
				p.Class = compSeason.GetClassName(p.Sex, dob.Date);
			}
			yield return p;
		}
	}
}
