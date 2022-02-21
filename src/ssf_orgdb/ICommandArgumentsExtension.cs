using System.IO;
using System.Linq;

namespace SSF.Timing.Tools.OrganizationDb;

public static class ICommandArgumentsExtension
{
	public static FileInfo FirstAsFile(this ICommandArgs args)
	{
		string path = args?.Args.FirstOrDefault();
		if (!string.IsNullOrWhiteSpace(path))
		{
			return new FileInfo(path);
		}
		return null;
	}
}
