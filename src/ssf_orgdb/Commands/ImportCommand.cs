using System;
using System.IO;
using log4net;

namespace SSF.Timing.Tools.OrganizationDb.Commands;

internal class ImportCommand : IDisposable
{
	private static readonly ILog s_log = LogManager.GetLogger(typeof(ImportCommand).Name);

	private readonly string m_path;

	private readonly IPersonReader m_reader;

	private readonly IPersonWriter m_writer;

	private ImportCommand(FileInfo fi, IPersonReader reader, IPersonWriter writer)
	{
		m_path = fi.FullName;
		m_reader = reader;
		m_writer = writer;
	}

	internal static void Execute(ICommandArgs args)
	{
		try
		{
			FileInfo fi = args.FirstAsFile();
			if (fi != null)
			{
				if (PersonReaderRepository.TryGetReader(fi, out var reader))
				{
					FileInfo importFI = WellKnown.SqliteFI;
					if (PersonWriterRepository.TryGetWriter(importFI, out var writer))
					{
						using (ImportCommand importCmd = new ImportCommand(fi, reader, writer))
						{
							reader.Flags = args.Flags;
							writer.Flags = args.Flags;
							importCmd.Import();
							return;
						}
					}
					reader.Dispose();
					throw new NotSupportedException(importFI.FullName);
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
		m_writer.Dispose();
	}

	private void Import()
	{
		m_writer.Write(m_reader.Get());
	}
}
