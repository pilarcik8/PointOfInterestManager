using System.Globalization;
using Bogus;

var outputPath = args.Length > 0 ? args[0] : "C:\\Users\\Admin\\source\\repos\\AUS2 S1\\PoiManager\\Data\\poi-test-data.csv";
var count = args.Length > 1 ? int.Parse(args[1], CultureInfo.InvariantCulture) : 100;

if (count < 0)
{
    throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
}

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
        faker.Random.Double(-90, 90).ToString("R", CultureInfo.InvariantCulture),
        faker.Random.Double(-180, 180).ToString("R", CultureInfo.InvariantCulture),
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
