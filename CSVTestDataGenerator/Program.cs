using System.Globalization;
using Bogus;

var projectPath = Directory.GetCurrentDirectory();
var outputPath = Path.Combine(projectPath, "..\\..\\..\\generated_data.csv");

var count = 100;

const double AreaMinLat = 47.0;
const double AreaMinLon = 16.0;
const double AreaMaxLat = 50.0;
const double AreaMaxLon = 23.0;

var faker = new Faker();
using var writer = new StreamWriter(outputPath);
writer.WriteLine("id,name,description,lat,lon,address,created_ad,updated_ad");

for (var id = 0; id < count; id++)
{
    var createdAt = faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow);
    var updatedAt = faker.Date.Between(createdAt, DateTime.UtcNow);
    var address = id % 5 == 0 ? string.Empty : faker.Address.FullAddress();

    writer.WriteLine(string.Join(",",
        id.ToString(CultureInfo.InvariantCulture),
        EscapeCsv(faker.Address.City()),
        EscapeCsv(faker.Lorem.Sentence()),
        faker.Random.Double(AreaMinLat, AreaMaxLat).ToString("R", CultureInfo.InvariantCulture),
        faker.Random.Double(AreaMinLon, AreaMaxLon).ToString("R", CultureInfo.InvariantCulture),
        EscapeCsv(address),
        createdAt.ToString("o", CultureInfo.InvariantCulture),
        updatedAt.ToString("o", CultureInfo.InvariantCulture)));
}

Console.WriteLine($"Generated {count} rows in {outputPath}.");

static string EscapeCsv(string value)
{
    if (value.IndexOfAny([',', '"', '\n', '\r']) < 0)
    {
        return value;
    }

    return $"\"{value.Replace("\"", "\"\"")}\"";
}
