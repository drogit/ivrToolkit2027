using Microsoft.Extensions.Logging;
using SystemRecordingsGenerator;

var loggerFactory = BuildLoggerFactory();

Console.WriteLine("Select the language of the system recordings to generate:");
Console.WriteLine("  1) English (en-US)");
Console.WriteLine("  2) French Canadian (fr-CA)");
Console.Write("Choice: ");

switch (Console.ReadLine()?.Trim())
{
    case "1":
        new English(loggerFactory).Generate();
        break;
    case "2":
        new FrenchCanadian(loggerFactory).Generate();
        break;
    default:
        Console.WriteLine("Invalid choice.");
        break;
}

return;

ILoggerFactory BuildLoggerFactory()
{
    var factory =
        LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "hh:mm:ss ";
            }).AddFilter("*", LogLevel.Debug));

    SIPSorcery.LogFactory.Set(factory);
    return factory;
}
