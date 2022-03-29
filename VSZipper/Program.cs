using Microsoft.Extensions.Configuration;

using ZipperLibrary;

IConfiguration _config;

IConfigurationBuilder builder = new ConfigurationBuilder();
string currentDirectory = Directory.GetCurrentDirectory();
builder.SetBasePath(currentDirectory);
builder.AddJsonFile("appsettings.json");

_config = builder.Build();

Zipper zipper = new(_config);

zipper.Zip();

Console.WriteLine("Process Complete");