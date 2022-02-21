using System;
using System.IO;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Contracts;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Commands;

internal class CreateCompetitionCommand : IDisposable
{
	private static readonly ILog s_log = LogManager.GetLogger(typeof(CreateCompetitionCommand).Name);

	private readonly string m_path;

	private readonly ICompetitionReader m_reader;

	private CreateCompetitionCommand(FileInfo fi, ICompetitionReader reader)
	{
		m_path = fi.FullName;
		m_reader = reader;
	}

	internal static void Execute(ICommandArgs args)
	{
		try
		{
			FileInfo fi = args.FirstAsFile();
			if (fi != null)
			{
				if (CompetitionReaderRepository.TryGetReader(fi, out var reader))
				{
					reader.Flags = args.Flags;
					using CreateCompetitionCommand createCompetitionCmd = new CreateCompetitionCommand(fi, reader);
					createCompetitionCmd.CreateCompetition();
					return;
				}
				throw new NotSupportedException(fi.FullName);
			}
			s_log.Error("Missing import file as arg1");
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
		}
	}

	public void Dispose()
	{
		m_reader.Dispose();
	}

	private void CreateCompetition()
	{
		ICompetition c = m_reader.Get();
		FileInfo fi = new FileInfo(m_path);
		string name = Path.GetFileNameWithoutExtension(fi.Name);
		FileInfo outFI = null;
		FlagsCollection flags = m_reader.Flags;
		if (flags != null && flags["ssf"])
		{
			outFI = new FileInfo(Path.Combine(fi.DirectoryName, "SSFEntry_" + name + ".xml"));
		}
		else
		{
			FlagsCollection flags2 = m_reader.Flags;
			if (flags2 != null && flags2["json"])
			{
				outFI = new FileInfo(Path.Combine(fi.DirectoryName, name + ".json"));
			}
			else
			{
				FlagsCollection flags3 = m_reader.Flags;
				if (flags3 != null && flags3["excel"])
				{
					outFI = new FileInfo(Path.Combine(fi.DirectoryName, name + ".xlsx"));
				}
			}
		}
		if (outFI == null)
		{
			CompetitionJson json = c as CompetitionJson;
			if (json == null)
			{
				json = MapHelper.Map<CompetitionJson>(c);
			}
			s_log.Info(json.ToJson());
		}
		else
		{
			WriteFile(c, outFI);
		}
	}

	private void WriteFile(ICompetition c, FileInfo outFI)
	{
		try
		{
			if (CompetitionWriterRepository.TryGetWriter(outFI, out var writer))
			{
				writer.Write(c);
				return;
			}
			throw new NotSupportedException(outFI.FullName);
		}
		catch (Exception ex)
		{
			s_log.Error("Write failed: " + outFI.FullName, ex);
			throw;
		}
	}
}
