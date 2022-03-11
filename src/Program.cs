using System.Text.Json;

logObject(new TestApp.HomeCareProviderPersonalCareServicesFlags
{
    Cleaning = true,
    HelpWithPersonalHygiene = true,
});

logObject(new ProjectsFlags
{
    Project2 = true,
    Project3 = true,
});

static void logObject(object obj)
{
    Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}
