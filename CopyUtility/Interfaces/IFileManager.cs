using System.Collections.Generic;
using System.Threading;

namespace CopyUtility.Interfaces
{
    public interface IFileManager
    {
        IEnumerable<(string FileName, bool CopySuccess)> CopyDirectory(string source, string target, CancellationToken cancellationToken);
    }
}