using System.Collections.Generic;
using System.Linq;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Commands;

public class CommandArgs : ICommandArgs
{
	public string Name { get; }

	public FlagsCollection Flags { get; }

	public IReadOnlyCollection<string> Args { get; }

	public string InputLine { get; }

	public bool IsValid => !string.IsNullOrWhiteSpace(Name);

	private CommandArgs(string name, FlagsCollection flags, List<string> argList, string inputLine)
	{
		Name = name;
		Flags = flags;
		Args = argList;
		InputLine = inputLine;
	}

	public static ICommandArgs Parse(params string[] argParams)
	{
		string[] args = argParams ?? new string[0];
		IEnumerable<string> validArgs = args?.Where((string s) => !string.IsNullOrWhiteSpace(s));
		string orgLine = string.Join(" ", args);
		string cmd = null;
		FlagsCollection flags = new FlagsCollection();
		List<string> argList = new List<string>();
		foreach (string arg in validArgs)
		{
			if (arg.StartsWith("--"))
			{
				flags.Add(arg.Substring(2).ToLowerInvariant());
			}
			else if (cmd == null)
			{
				cmd = arg.ToLowerInvariant().Trim();
			}
			else
			{
				argList.Add(arg);
			}
		}
		return new CommandArgs(cmd, flags, argList, orgLine);
	}

	public override string ToString()
	{
		return "[" + Name + "] Args: " + string.Join(",", Args) + " Flags: " + string.Join(",", Flags);
	}
}
