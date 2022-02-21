using System;
using System.IO;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Contracts;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Writers;

internal class JsonCompetitionWriter : ICompetitionWriter, IReaderWriter, IDisposable
{
	public class Factory : ICompetitionWriterFactory, IReaderWriterFactory<ICompetitionWriter>
	{
		public bool TryGet(FileInfo fi, out ICompetitionWriter writer)
		{
			if (fi.Extension.StartsWith(".json", StringComparison.InvariantCultureIgnoreCase) && TryCreate(fi, out var jsonWriter))
			{
				writer = jsonWriter;
				return true;
			}
			writer = null;
			return false;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(JsonCompetitionWriter).Name);

	private readonly string m_path;

	public FlagsCollection Flags { get; set; }

	private JsonCompetitionWriter(FileInfo fi)
	{
		m_path = fi.FullName;
	}

	public void Dispose()
	{
	}

	public void Write(ICompetition competition)
	{
		try
		{
			s_log.Debug("Writing: " + m_path);
			DoWrite(competition);
		}
		catch (Exception ex)
		{
			s_log.Error("Write Failed: " + m_path, ex);
			throw;
		}
	}

	private void DoWrite(ICompetition competition)
	{
		FileInfo fi = new FileInfo(m_path);
		fi.Directory.Create();
		CompetitionJson json = competition as CompetitionJson;
		if (json == null)
		{
			json = MapHelper.Map<CompetitionJson>(competition);
		}
		json.Save(fi);
	}

	private static bool TryCreate(FileInfo fi, out JsonCompetitionWriter jsonWriter)
	{
		try
		{
			jsonWriter = new JsonCompetitionWriter(fi);
			return true;
		}
		catch (Exception ex)
		{
			s_log.Error(fi.FullName, ex);
			jsonWriter = null;
			return false;
		}
	}
}
