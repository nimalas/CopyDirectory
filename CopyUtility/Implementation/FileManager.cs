using CopyUtility.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SystemInterface.IO;

namespace CopyUtility.Implementation
{
    public class FileManager : IFileManager
    {
        private IDirectory _directoryFactory;
        private ILogger _logger;
        private IFileInfoFactory _fileInfoFactory;
        private bool _testMode;

        public FileManager(IDirectory directoryFactory, IFileInfoFactory fileInfoFactory, ILogger logger)
        {
            _directoryFactory = directoryFactory ?? throw new ArgumentNullException(nameof(directoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(directoryFactory));
            _fileInfoFactory = fileInfoFactory ?? throw new ArgumentNullException(nameof(fileInfoFactory));
        }

        public FileManager(IDirectory directoryFactory, IFileInfoFactory fileInfoFactory, ILogger logger, bool testMode) : this(directoryFactory, fileInfoFactory, logger)
        {
            _testMode = testMode; //Used for non mockable items
        }

        /// <summary>
        /// Copies files and sub folders with files from source folder to target
        /// </summary>
        /// <param name="source">Full path of the source folder</param>
        /// <param name="target">Full path of the target folder</param>
        /// <param name="cancellationToken">Interrupts the copy process if set to cancel</param>
        /// <returns>Name of tht file and the outcome of the copy process</returns>
        public IEnumerable<(string FileName, bool CopySuccess)> CopyDirectory(string source, string target, CancellationToken cancellationToken)
        {
            if (!_directoryFactory.Exists(source) && !_testMode)
            {
                throw new ArgumentException($"Invalid source directory {source}");
            }

            var directories = _directoryFactory.GetDirectories(source).ToList();

            //Copy the files in the folder
            foreach (var file in CopyFiles(source, target, cancellationToken))
            {
                yield return file;
            }

            //Copy the sub folders
            foreach (var directory in directories)
            {
                foreach(var file in CopyDirectory(directory, target, cancellationToken))
                {
                    yield return file;
                }
            }
        }

        /// <summary>
        /// Copies files from source folder to target
        /// </summary>
        /// <param name="source">Full path of the source folder</param>
        /// <param name="target">Full path of the target folder</param>
        /// <param name="cancellationToken">Interrupts the copy process if set to cancel</param>
        /// <returns>Name of tht file and the outcome of the copy process</returns>
        public IEnumerable<(string fileName, bool copySuccess)> CopyFiles(string source, string target, CancellationToken cancellationToken)
        {
            var files = _directoryFactory.GetFiles(source);
            
            foreach (var file in files)
            {
                var fileNameCopied = "";
                var fileCopySuccess = false;

                cancellationToken.ThrowIfCancellationRequested();

                var fileInfo = _fileInfoFactory.Create(file);
                var sourceFileName = fileInfo.FullName;
                var sourceFileTransposed = sourceFileName.Replace(_directoryFactory.GetDirectoryRoot(fileInfo.DirectoryName),"");

                var destinationFile = _fileInfoFactory.Create($@"{target}\{sourceFileTransposed}");

                if (!destinationFile.Exists && !_testMode)
                {
                    if (!MakeFolderAvailable(destinationFile.DirectoryName))
                    {
                        throw new ArgumentException($"Inavlid destination directory {destinationFile.DirectoryName}");
                    }

                    if (fileInfo.Exists)
                    {
                        fileInfo.CopyTo(destinationFile.FullName);
                        fileCopySuccess = true;
                    }
                }

                fileNameCopied = file;

                yield return (fileNameCopied, fileCopySuccess);
            }
        }

        /// <summary>
        /// Ensures the folder exists by creating it if missing
        /// </summary>
        /// <param name="targetFolder">Full path of the folder in question</param>
        /// <returns></returns>
        private bool MakeFolderAvailable(string targetFolder)
        {
            try
            {
                if (!_directoryFactory.Exists(targetFolder))
                {
                    //_directoryFactory.CreateDirectory(targetFolder); - This does not work with .Net core. Hence the following line.
                    Directory.CreateDirectory(targetFolder);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message);
                return false;
            }

            return true;
        }

    }
}
