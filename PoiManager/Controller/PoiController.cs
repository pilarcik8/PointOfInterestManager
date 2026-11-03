using System.Globalization;
using Bogus;
using PoiManager.Model;

namespace PoiManager.Controller
{
    // Controller - spracuje vstupy z view (texty z formularov), zvaliduje ich a zavola PoiModel
    public class PoiController
    {
        private const double EarthRadiusMeters = 6_371_000.0;

        // Hranice Slovenska s rezervou - body mimo nich model stale eviduje
        public const double AreaMinLat = 47.0;
        public const double AreaMinLon = 16.0;
        public const double AreaMaxLat = 50.0;
        public const double AreaMaxLon = 23.0;

        private readonly PoiModel _model;

        public PoiModel.TreeType TreeType { get; }


        // Vygenerovane / upravene pomocou AI - Kapitola 67

        public PoiController(PoiModel.TreeType treeType)
        {
            _model = new PoiModel(AreaMinLat, AreaMinLon, AreaMaxLat, AreaMaxLon, treeType: treeType);
            TreeType = treeType;
        }

        // 1. Vyhladanie bodov v obdlznikovej oblasti
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public List<PointOfInterest> FindAtPosition(string lat1Text, string lon1Text, string lat2Text, string lon2Text)
        {
            double lat1 = ParseLatitude(lat1Text);
            double lon1 = ParseLongitude(lon1Text);
            double lat2 = ParseLatitude(lat2Text);
            double lon2 = ParseLongitude(lon2Text);

            return _model.FindPointsRange(
                Math.Min(lat1, lat2), Math.Min(lon1, lon2),
                Math.Max(lat1, lat2), Math.Max(lon1, lon2)).ToList();
        }

        // 2., 4., 6. Vyhladanie bodov v okoli - polomer v stupnoch
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public List<PointOfInterest> FindAround(string latText, string lonText, string radiusText)
        {
            double lat = ParseLatitude(latText);
            double lon = ParseLongitude(lonText);
            double radius = ParseDouble(radiusText, "Polomer");
            if (radius < 0 || radius > 180)
            {
                throw new FormatException("Polomer musí byť v rozsahu 0 až 180 stupňov.");
            }

            // Polomer je uhlová vzdialenosť v stupňoch; kandidátov obmedzí ohraničujúci obdĺžnik.
            double angular = radius * Math.PI / 180.0;
            double dLat = radius;
            double dLon = 180.0;

            if (radius < 90.0 && lat - radius > -90.0 && lat + radius < 90.0)
            {
                double sinRatio = Math.Sin(angular) / Math.Cos(lat * Math.PI / 180.0);
                dLon = sinRatio >= 1.0 ? 180.0 : Math.Asin(sinRatio) * 180.0 / Math.PI;
            }

            double minLon = lon - dLon;
            double maxLon = lon + dLon;
            if (minLon < -180.0 || maxLon > 180.0)
            {
                minLon = -180.0;
                maxLon = 180.0;
            }

            var candidates = _model.FindPointsRange(
                Math.Max(-90.0, lat - dLat), minLon,
                Math.Min(90.0, lat + dLat), maxLon);
            double radiusMeters = angular * EarthRadiusMeters;

            var result = new List<PointOfInterest>();
            foreach (var poi in candidates)
            {
                if (DistanceMeters(lat, lon, poi.Latitude, poi.Longitude) <= radiusMeters)
                {
                    result.Add(poi);
                }
            }
            return result;
        }

        // 3. Vytvorenie noveho bodu - id a datumy generuje model
        // Vygenerovane / upravene pomocou AI - Kapitola 67, 70
        public void Create(string name, string description, string latText, string lonText, string address)
        {
            ValidateName(name);
            ValidateDescription(description);
            double lat = ParseLatitude(latText);
            double lon = ParseLongitude(lonText);
            _model.CreatePOI(name.Trim(), lat, lon, description.Trim(), NullIfEmpty(address));
        }

        // 4. Editacia vybraneho bodu vratane GPS pozicie
        // Vygenerovane / upravene pomocou AI - Kapitola 67, 70
        public void Update(PointOfInterest poi, string name, string description, string latText, string lonText, 
                            string address)
        {
            ValidateName(name);
            ValidateDescription(description);
            double lat = ParseLatitude(latText);
            double lon = ParseLongitude(lonText);
            _model.UpdatePOI(poi, lat, lon, name.Trim(), description.Trim(), NullIfEmpty(address));
        }

        // 5. Vyhladanie podla nazvu v obdlzniku (diakritika sa ignoruje v modeli)
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public List<PointOfInterest> FindByName(string lat1Text, string lon1Text, string lat2Text, string lon2Text, string text)
        {
            double lat1 = ParseLatitude(lat1Text);
            double lon1 = ParseLongitude(lon1Text);
            double lat2 = ParseLatitude(lat2Text);
            double lon2 = ParseLongitude(lon2Text);

            return _model.FindRangeWithName(
                Math.Min(lat1, lat2), Math.Min(lon1, lon2),
                Math.Max(lat1, lat2), Math.Max(lon1, lon2),
                text.Trim()).ToList();
        }

        // 6. Vyradenie vybraneho bodu
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public void Delete(PointOfInterest poi)
        {
            var nodes = new LinkedList<PointOfInterest>();
            nodes.AddLast(poi);
            _model.Delete(nodes);
        }

        // Vypis vsetkych evidovanych bodov
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public List<PointOfInterest> GetAll()
        {
            return _model.GetAllPointsOfInterest().ToList();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public void Import(string path)
        {
            _model.ImportPointsOfInterest(path);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public void Export(string path)
        {
            _model.ExportPointsOfInterest(path);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 71
        public static void GenerateTestData(string path, int count)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Cesta k súboru je povinná.", nameof(path));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Počet záznamov musí byť väčší ako 0.");
            }

            var faker = new Faker();
            using var writer = new StreamWriter(path);
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
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversinov vzorec
            double toRad = Math.PI / 180.0;
            double dLat = (lat2 - lat1) * toRad;
            double dLon = (lon2 - lon1) * toRad;
            double a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
                       (Math.Cos(lat1 * toRad) * Math.Cos(lat2 * toRad) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
            return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 71
        private static string EscapeCsv(string value)
        {
            if (value.IndexOfAny([',', '"', '\n', '\r']) < 0)
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new FormatException("Názov bodu je povinný.");
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 70
        private static void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new FormatException("Opis bodu je povinný.");
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private static string? NullIfEmpty(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private static double ParseLatitude(string text)
        {
            double value = ParseDouble(text, "Zemepisná šírka");
            if (value < -90 || value > 90)
            {
                throw new FormatException("Zemepisná šírka musí byť v rozsahu -90 až 90.");
            }
            return value;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private static double ParseLongitude(string text)
        {
            double value = ParseDouble(text, "Zemepisná dĺžka");
            if (value < -180 || value > 180)
            {
                throw new FormatException("Zemepisná dĺžka musí byť v rozsahu -180 až 180.");
            }
            return value;
        }

        // Akceptuje desatinnu bodku aj ciarku
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private static double ParseDouble(string text, string fieldName)
        {
            string normalized = text.Trim().Replace(',', '.');
            if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                throw new FormatException($"{fieldName}: '{text}' nie je platné číslo.");
            }
            return value;
        }
    }
}
