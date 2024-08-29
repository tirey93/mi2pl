using mi2se_classic_injector.Commands;
using mi2se_classic_injector.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

var serviceProvider = new ServiceCollection()

    .Configure<MainSettings>(configuration.GetSection(nameof(MainSettings)))
    .AddTransient<InjectClassicToSeCommand>()
    .BuildServiceProvider();

var injectClassicToSeCommand = serviceProvider.GetRequiredService<InjectClassicToSeCommand>();

try
{
    if (!injectClassicToSeCommand.HasErrors)
        injectClassicToSeCommand.Execute();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadKey();
}
