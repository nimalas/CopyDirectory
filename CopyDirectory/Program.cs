using Autofac;
using CopyDirectory.AutfacModules;
using CopyUtility.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CopyDirectory
{
    /*
    
    Copies all the contents from one directory to the other including sub directories.
    Operation can be cancelled at any time during copy process.
    When started it will ignore the files copied already and continue from where it left off.

    Future improvements:
        1) When a file is not copied it will show as not copied but does not say why.
        2) Does not verify the contents once copied. Can be improved with some checksum verification. 
        3) The command line parsing is very basic. Can be improved with further enhancements.
        4) Should have a configuration file for any custom paths etc

    These can be improved up on further work.

    Projects:
        1) CopyDirectory - This command line utility
        2) CopyUtility - Seperate assembly containing the copy process
        3) CopyUtilityTests - Unit tests for CopyUtility
    
     */

    class Program
    {
        static CancellationTokenSource _cancellationSource;
        static bool _copyCompleted;
        static void Main(string[] args)
        {
            try
            {
                IContainer container = GetContainer(); //Using autofac to demonstrate decoupled interfaces
                var fileManager = container.Resolve<IFileManager>();
                _cancellationSource = new CancellationTokenSource();

                (var sourceDirectory, var destinationDirectory) = GetArguments(args);

                if (ShowInitText())
                {
                    ExecuteCopy(fileManager, _cancellationSource, sourceDirectory, destinationDirectory).Wait();
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Usage:\ncopydirectory <source directory> <destination directory>");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is OperationCanceledException)
                {
                    Console.WriteLine("Copy cancelled.");
                }
                else if (ex.InnerException is ArgumentException)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                else
                {
                    Console.Write($"Error occoured when copying: {ex.Message}");
                }
            }
        }

        private static async Task ExecuteCopy(IFileManager fileManager, CancellationTokenSource cancellationSource, string sourceDirectory, string destinationDirectory)
        {
            Task checkExitTask = Task.Run(new Action(CheckExit));

            foreach (var file in fileManager.CopyDirectory(sourceDirectory, destinationDirectory, cancellationSource.Token))
            {
                Console.WriteLine($"{(file.CopySuccess ? "Copied" : "Did not copy")} - {file.FileName}");
            }

            _copyCompleted = true;

            Console.WriteLine("\n\nCopy completed. Pres Esc to finish.");

            await checkExitTask;
        }

        private static void CheckExit()
        {
            ConsoleKeyInfo lastKey = Console.ReadKey();
            
            while(lastKey.Key != ConsoleKey.Escape)
            {
                lastKey = Console.ReadKey();
            }

            if (!_copyCompleted)
            {
                Console.WriteLine("Cancel requested.");
                _cancellationSource.Cancel();
            }
        }

        /// <summary>
        /// Displays inititialisation text to the user and gets approval
        /// </summary>
        private static bool ShowInitText()
        {
            Console.WriteLine("\n\nCopy folder demo.\n\nPress any key to continue.\n" +
                "Press Esc to interrupt the copy process.\n\n" +
                "The process will start from where it left off when started again.\n");
            var key  = Console.ReadKey();

            if (key.Key == ConsoleKey.Escape)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Full path of source and target folders</returns>
        private static (string sourceDirectory, string destinationDirectory) GetArguments(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Insufficient arguments");

            return (args[0], args[1]);
        }

        /// <summary>
        /// Gets the compiled Autofac container
        /// </summary>
        /// <returns></returns>
        private static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CopyDirectoryModule());
            var container = builder.Build();
            return container;
        }
    }
}
