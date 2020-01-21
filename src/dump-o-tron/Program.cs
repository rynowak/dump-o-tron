using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace DumpOTron
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var command = new RootCommand("Dumps usage of PlatformNotSupportedException");

            var directoryArgument = new Argument<DirectoryInfo>();
            directoryArgument.SetDefaultValue(new DirectoryInfo(Environment.CurrentDirectory));
            directoryArgument.ExistingOnly();

            command.AddOption(new Option(new[] { "-d", "--directory", })
            {
                Argument = directoryArgument,
                Required = false,
            });

            command.Handler = CommandHandler.Create<IConsole, DirectoryInfo>(async (console, directory) =>
            {
                await Dumper.DumpAsync(console, directory);
            });

            var builder = new CommandLineBuilder(command);

            // Parsing behavior
            builder.UseHelp();
            builder.UseVersionOption();
            builder.UseDebugDirective();
            builder.UseParseErrorReporting();
            builder.ParseResponseFileAs(ResponseFileHandling.ParseArgsAsSpaceSeparated);
            builder.UsePrefixes(new[] { "-", "--", }); // disable garbage windows conventions

            builder.CancelOnProcessTermination();
            builder.UseExceptionHandler(HandleException);

            var parser = builder.Build();
            return await parser.InvokeAsync(args);
        }

        private static void HandleException(Exception exception, InvocationContext context)
        {
            if (exception is OperationCanceledException)
            {
                context.Console.Error.WriteLine("Oh dear! Operation canceled.");
            }
            else if (exception is CommandException command)
            {
                context.Console.Error.WriteLine($"Drats! '{context.ParseResult.CommandResult.Name}' failed:");
                context.Console.Error.WriteLine($"\t{command.Message}");

                if (command.InnerException != null)
                {
                    context.Console.Error.WriteLine();
                    context.Console.Error.WriteLine(command.InnerException.ToString());
                }
            }
            else
            {
                context.Console.Error.WriteLine("An unhandled exception has occurred, how unseemly: ");
                context.Console.Error.WriteLine(exception.ToString());
            }

            context.ResultCode = 1;
        }
    }
}
