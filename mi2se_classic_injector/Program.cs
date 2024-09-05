using mi2se_classic_injector.Commands;
using mi2se_classic_injector.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;

var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

var serviceProvider = new ServiceCollection()

    .Configure<MainSettings>(configuration.GetSection(nameof(MainSettings)))
    .Configure<LiteralSettings>(configuration.GetSection(nameof(LiteralSettings)))
    .AddTransient<InjectClassicToSeCommand>()
    .AddTransient<ErrorsToPoCommand>()
    .AddTransient<ErrorsFromPoCommand>()
    .AddTransient<DiffCommand>()
    .BuildServiceProvider();




var options = serviceProvider.GetService<IOptions<MainSettings>>();
try
{
    switch (options.Value.Mode)
    {
        case Mode.Default:
            var injectClassicToSeCommand = serviceProvider.GetRequiredService<InjectClassicToSeCommand>();
            if (!injectClassicToSeCommand.HasErrors)
                injectClassicToSeCommand.Execute();
            break;
        case Mode.ErrorToPo:
            var errorsToPoCommand = serviceProvider.GetRequiredService<ErrorsToPoCommand>();
            if (!errorsToPoCommand.HasErrors)
                errorsToPoCommand.Execute();
            break;
        case Mode.ErrorFromPo:
            var errorsFromPoCommand = serviceProvider.GetRequiredService<ErrorsFromPoCommand>();
            if (!errorsFromPoCommand.HasErrors)
                errorsFromPoCommand.Execute();
            break;
        case Mode.Diff:
            var diffCommand = serviceProvider.GetRequiredService<DiffCommand>();
            if (!diffCommand.HasErrors)
                diffCommand.Execute();
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadKey();
}
