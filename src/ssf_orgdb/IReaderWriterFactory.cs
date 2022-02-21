using System.IO;

namespace SSF.Timing.Tools.OrganizationDb;

public interface IReaderWriterFactory<T> where T : IReaderWriter
{
	bool TryGet(FileInfo fi, out T io);
}
