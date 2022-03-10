﻿using System.Text.Json;

using Directory.Generators;

var flags = new BoolsFromEnumFlags
{
    Cleaning = true,
    HelpWithPersonalHygiene = true,
};

var json = JsonSerializer.Serialize(flags, new JsonSerializerOptions
{
    WriteIndented = true
});

Console.WriteLine(json);