using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;
using log4net.Config;
using SSF.Timing.Tools.OrganizationDb.Commands;

namespace SSF.Timing.Tools.OrganizationDb;

internal class Program
{
	private static readonly string s_workingDir;

	private static readonly ILog log;

	private static int ExitCode;

	public static DirectoryInfo WorkingDI => new DirectoryInfo(s_workingDir);

	static Program()
	{
		log = LogManager.GetLogger(typeof(Program));
		s_workingDir = Directory.GetCurrentDirectory();
	}

	private static void Main(string[] args)
	{
		XmlConfigurator.Configure();
		ICommandArgs cmdArgs = CommandArgs.Parse(args);
		if (cmdArgs.IsValid)
		{
			try
			{
				MapDefinitions.Init();
				ExecuteCommand(cmdArgs);
			}
			catch (Exception ex)
			{
				log.Error("Command failed", ex);
				ExitCode = -1;
			}
		}
		else
		{
			log.Warn("No command");
		}
		LogManager.Flush(1000);
		if (Debugger.IsAttached)
		{
			Thread.Sleep(1000);
			Console.WriteLine("Hit any key to quit");
			Console.Read();
		}
		Environment.Exit(ExitCode);
	}

	private static void ExecuteCommand(ICommandArgs args)
	{
		switch (args.Name)
		{
		case "import":
			ImportCommand.Execute(args);
			break;
		case "export":
			ExportCommand.Execute(args);
			break;
		case "create_competition":
			CreateCompetitionCommand.Execute(args);
			break;
		default:
			throw new ArgumentException(args.ToString());
		}
	}
}
