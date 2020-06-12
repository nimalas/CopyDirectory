using Autofac;
using CopyUtility.Implementation;
using CopyUtility.Interfaces;
using SystemInterface.IO;
using SystemWrapper.IO;

namespace CopyDirectory.AutfacModules
{
    public class CopyDirectoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new DirectoryWrap()).As<IDirectory>();
            builder.RegisterInstance(new FileInfoFactory()).As<IFileInfoFactory>();            
            builder.RegisterInstance(new Logger(@"c:\work1\dcsl.log")).As<ILogger>();
            builder.RegisterType<FileManager>().As<IFileManager>();
        }
    }
}
