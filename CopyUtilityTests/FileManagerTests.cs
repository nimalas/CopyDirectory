using CopyUtility.Implementation;
using CopyUtility.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemInterface.IO;
using SystemWrapper.IO;

namespace FileCopierTest
{
    public class FileManagerTests
    {
        Mock<IDirectory> _direcoryMock;
        Mock<ILogger> _loggerMock;
        Mock<IFileInfoFactory> _fileInfoFactoryMock;

        [SetUp]
        public void Setup()
        {
            _direcoryMock = new Mock<IDirectory>();
            _loggerMock = new Mock<ILogger>();
            _fileInfoFactoryMock = new Mock<IFileInfoFactory>();
        }

        [Test]
        public void CanCreate()
        {
            Assert.That(() => new FileManager(_direcoryMock.Object, _fileInfoFactoryMock.Object, _loggerMock.Object), Throws.Nothing);
        }

        [Test]
        public void CanmotCreateWithNullArguments()
        {
            Assert.That(() => new FileManager(null, _fileInfoFactoryMock.Object, _loggerMock.Object), Throws.ArgumentNullException);
            Assert.That(() => new FileManager(_direcoryMock.Object, null, _loggerMock.Object), Throws.ArgumentNullException);
            Assert.That(() => new FileManager(_direcoryMock.Object, _fileInfoFactoryMock.Object, null), Throws.ArgumentNullException);
        }

        [Test]
        public void CopyFilesReturnsFileNames()
        {
            var fm = new FileManager(_direcoryMock.Object, _fileInfoFactoryMock.Object, _loggerMock.Object, testMode: true);

            List<string> files = new List<string> { "f1", "f2" };
            _direcoryMock.Setup(d => d.GetFiles(It.IsAny<string>())).Returns(files.ToArray<string>());
            _direcoryMock.Setup(d => d.GetDirectoryRoot(It.IsAny<string>())).Returns("d1");
            var fileInfoMock = new Mock<FileInfoWrap>("f1");
            _fileInfoFactoryMock.Setup(f => f.Create(It.IsAny<string>())).Returns(fileInfoMock.Object);

            var fileItemsData = fm.CopyFiles("testPath", "targetFolder", new CancellationToken());
            var fileItems = fileItemsData.Select(x => x.fileName);
            Assert.AreEqual(files, fileItems.ToList());
        }


        [Test]
        public void CopyFilesCanBeCancelled()
        {
            var fm = new FileManager(_direcoryMock.Object, _fileInfoFactoryMock.Object, _loggerMock.Object, testMode: true);

            List<string> files = new List<string> { "f1", "f2" };
            _direcoryMock.Setup(d => d.GetFiles(It.IsAny<string>())).Returns(files.ToArray<string>());
            _direcoryMock.Setup(d => d.GetDirectoryRoot(It.IsAny<string>())).Returns("d1");

            var tokenSource = new CancellationTokenSource();            
            var cancellationToken = tokenSource.Token;

            var fileInfoMock = new Mock<FileInfoWrap>("f1");
            _fileInfoFactoryMock.Setup(f => f.Create(It.IsAny<string>())).Returns(fileInfoMock.Object);

            Assert.That(() => { 
                var fileItems = fm.CopyFiles("testPath", "targetFolder", cancellationToken);
                var fileList = fileItems.ToList();
            }, Throws.Nothing);

            Assert.That(() => {
                tokenSource.Cancel();
                var fileItems = fm.CopyFiles("testPath", "targetFolder", cancellationToken);
                var fileList = fileItems.ToList();
            }, Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public void CopyDirectoryReturnsFileNames()
        {
            var fm = new FileManager(_direcoryMock.Object, _fileInfoFactoryMock.Object, _loggerMock.Object, testMode:true);

            var directories = new List<string> { "d1", "d2" };
            var filesD1 = new List<string> { "d1f1", "d1f2" };
            var filesD2 = new List<string> { "d2f1", "d2f2" };

            _direcoryMock.Setup(d => d.GetDirectories("testPath")).Returns(directories.ToArray<string>());
            _direcoryMock.Setup(d => d.GetFiles("d1")).Returns(filesD1.ToArray<string>());
            _direcoryMock.Setup(d => d.GetFiles("d2")).Returns(filesD2.ToArray<string>());
            _direcoryMock.Setup(d => d.GetDirectoryRoot(It.IsAny<string>())).Returns("d1");
            
            var fileInfoMock = new Mock<FileInfoWrap>("f1");            
            _fileInfoFactoryMock.Setup(f => f.Create(It.IsAny<string>())).Returns(fileInfoMock.Object);
            
            var fileItemsData = fm.CopyDirectory("testPath", "targetFolder", new CancellationToken());

            var fileItems = fileItemsData.Select(x => x.FileName);

            filesD1.ForEach(x => Assert.Contains(x, fileItems.ToList()));
            filesD2.ForEach(x => Assert.Contains(x, fileItems.ToList()));
        }


        [Test]
        public void CopyDirectoryCanBeCancelled()
        {
            var fm = new FileManager(_direcoryMock.Object, _fileInfoFactoryMock.Object, _loggerMock.Object, testMode: true);

            var directories = new List<string> { "d1", "d2" };
            var filesD1 = new List<string> { "d1f1", "d1f2" };
            var filesD2 = new List<string> { "d2f1", "d2f2" };

            _direcoryMock.Setup(d => d.GetDirectories("testPath")).Returns(directories.ToArray<string>());
            _direcoryMock.Setup(d => d.GetFiles("d1")).Returns(filesD1.ToArray<string>());
            _direcoryMock.Setup(d => d.GetFiles("d2")).Returns(filesD2.ToArray<string>());
            _direcoryMock.Setup(d => d.GetDirectoryRoot(It.IsAny<string>())).Returns("d1");

            var fileInfoMock = new Mock<FileInfoWrap>("f1");
            _fileInfoFactoryMock.Setup(f => f.Create(It.IsAny<string>())).Returns(fileInfoMock.Object);

            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            Assert.That(() => {
                var fileItems = fm.CopyDirectory("testPath", "targetFolder", cancellationToken);
                var fileList = fileItems.ToList();
            }, Throws.Nothing);

            Assert.That(() => {
                tokenSource.Cancel();
                var fileItems = fm.CopyDirectory("testPath", "targetFolder", cancellationToken);
                var fileList = fileItems.ToList();
            }, Throws.InstanceOf<OperationCanceledException>());
        }

    }
}