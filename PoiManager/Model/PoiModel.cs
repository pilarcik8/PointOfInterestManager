using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DataStructures.Hybrid_Quad_Tree;
using DataStructures.QTree;
using DataStructures.KD_Tree;

namespace PoiManager.Model
{
    public class PoiModel
    {
        private QTree<PointOfInterest, double>? _qTree;
        private HybridQTree<PointOfInterest, double>? _hybridQTree;
        private KDTree<double, PointOfInterest>? _kdTree;
        private int UniqueID = 0;

        // Vygenerovane / upravene pomocou AI - Kapitola 55 
        public PoiModel(
            double minLat, double minLon, double maxLat, double maxLon,
            int maxDepth = int.MaxValue,
            TreeType treeType = TreeType.QTree)
        {
            switch (treeType)
            {
                case TreeType.QTree:
                    _qTree = new QTree<PointOfInterest, double>(
                        minLon, minLat, maxLon, maxLat,
                        poi => (poi.Longitude, poi.Latitude),
                        maxDepth);
                    break;

                case TreeType.HybridQTree:
                    _hybridQTree = new HybridQTree<PointOfInterest, double>(
                        minLon, minLat, maxLon, maxLat,
                        poi => (poi.Longitude, poi.Latitude),
                        maxDepth);
                    break;

                case TreeType.KDTree:
                    _kdTree = new KDTree<double, PointOfInterest>(2);
                    break;
            }
        }

        private const string CsvHeader = "id,name,description,lat,lon,address,created_ad,updated_ad";

        // Vygenerovane / upravene pomocou AI - Kapitola 66
        public void ImportPointsOfInterest(string pathToCSV)
        {
            int lineNumber = 0;
            foreach (var line in File.ReadLines(pathToCSV))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var fields = ParseCsvLine(line);

                // Hlavicku preskocime
                if (lineNumber == 1 && fields[0].Trim().Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (fields.Count != 8)
                {
                    throw new FormatException($"Riadok {lineNumber}: ocakavanych 8 stlpcov, najdenych {fields.Count}.");
                }

                if (string.IsNullOrWhiteSpace(fields[2]))
                {
                    throw new FormatException($"Riadok {lineNumber}: opis bodu je povinný.");
                }

                var poi = new PointOfInterest(
                    id: int.Parse(fields[0], CultureInfo.InvariantCulture),
                    latitude: double.Parse(fields[3], CultureInfo.InvariantCulture),
                    longitude: double.Parse(fields[4], CultureInfo.InvariantCulture),
                    name: fields[1],
                    description: fields[2],
                    address: fields[5].Length == 0 ? null : fields[5],
                    createdAt: DateTime.Parse(fields[6], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    lastUpdatedAt: DateTime.Parse(fields[7], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

                InsertIntoTree(poi);

                // Nove POI musia dostat id vacsie ako vsetky importovane
                if (poi.Id >= UniqueID)
                {
                    UniqueID = poi.Id + 1;
                }
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 66
        public void ExportPointsOfInterest(string pathToCSV)
        {
            var pois = GetAllPointsOfInterest();

            using var writer = new StreamWriter(pathToCSV);
            writer.WriteLine(CsvHeader);
            foreach (var poi in pois)
            {
                writer.WriteLine(string.Join(",",
                    poi.Id.ToString(CultureInfo.InvariantCulture),
                    EscapeCsv(poi.Name),
                    EscapeCsv(poi.Description),
                    poi.Latitude.ToString("R", CultureInfo.InvariantCulture),
                    poi.Longitude.ToString("R", CultureInfo.InvariantCulture),
                    EscapeCsv(poi.Address ?? string.Empty),
                    poi.CreatedAt.ToString("o", CultureInfo.InvariantCulture),
                    poi.LastUpdatedAt.ToString("o", CultureInfo.InvariantCulture)));
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        public LinkedList<PointOfInterest> GetAllPointsOfInterest()
        {
            var pois = FindPointsRange(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue);
            if (_qTree != null)
            {
                // QTree.FindRange nevracia body mimo hranic stromu
                foreach (var poi in _qTree.OutOfBoundsData)
                {
                    pois.AddLast(poi);
                }
            }
            return pois;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 66
        private static string EscapeCsv(string value)
        {
            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 66
        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // Zdvojena uvodzovka vo vnutri pola znamena jednu uvodzovku
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return fields;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 55
        public void CreatePOI(string name, double lat, double lon, string desc, string? addr = null, int? id = null)
        {
            ValidateDescription(desc);

            if (id == null)
            {
                id = UniqueID;
                UniqueID++;
            }
            var poi = new PointOfInterest(id: id.Value, latitude: lat, longitude: lon, name: name,
                                        description: desc, address: addr);

            InsertIntoTree(poi);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 66
        private void InsertIntoTree(PointOfInterest poi)
        {
            if (_qTree != null)
            {
                _qTree.Insert(poi);
            }
            else if (_hybridQTree != null)
            {
                _hybridQTree.Insert(poi);
            }
            else if (_kdTree != null)
            {
                var keys = new LinkedList<KDKey<double>>();
                keys.AddLast(new KDKey<double>(poi.Longitude));
                keys.AddLast(new KDKey<double>(poi.Latitude));
                _kdTree.Insert(poi, keys);
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 60
        public void UpdatePOI(PointOfInterest poi, double lat, double lon, string name, string desc, string? addr = null)
        {
            ValidateDescription(desc);

            if (poi == null)
            {
                return;
            }

            bool positionChanged = poi.Latitude != lat || poi.Longitude != lon;

            if (positionChanged)
            {
                Delete(poi);
                poi.UpdateValues(lat, lon, name, desc, addr);

                if (_qTree != null)
                {
                    _qTree.Insert(poi);
                }
                else if (_hybridQTree != null)
                {
                    _hybridQTree.Insert(poi);
                }
                else if (_kdTree != null)
                {
                    var keys = new LinkedList<KDKey<double>>();
                    keys.AddLast(new KDKey<double>(poi.Longitude));
                    keys.AddLast(new KDKey<double>(poi.Latitude));
                    _kdTree.Insert(poi, keys);
                }
            }
            else
            {
                poi.UpdateValues(lat, lon, name, desc, addr);
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 60
        public void UpdatePOI(PointOfInterest poi, string name, double lat, double lon, string desc, string? addr = null)
        {
            UpdatePOI(poi, lat, lon, name, desc, addr);
        }

        private static void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new FormatException("Opis bodu je povinný.");
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 59
        private void Delete(PointOfInterest poi)
        {
            if (poi == null)
            {
                return;
            }

            if (_qTree != null)
            {
                _qTree.DeleteNode(poi);
            }
            else if (_hybridQTree != null)
            {
                _hybridQTree.DeleteNode(poi);
            }
            else if (_kdTree != null)
            {
                var keys = new LinkedList<KDKey<double>>();
                keys.AddLast(new KDKey<double>(poi.Longitude));
                keys.AddLast(new KDKey<double>(poi.Latitude));
                _kdTree.DeleteNode(keys);
            }
            else
            {
                throw new InvalidOperationException("Ziadny strom nie je inicializovany.");
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 59
        public void Delete(LinkedList<PointOfInterest> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (var poi in nodes)
            {
                Delete(poi);
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 63
        public LinkedList<PointOfInterest> FindPointsAroundCentre(double lat, double lon, double radius)
        {
            // 1. Získame kandidátov v ohraničujúcom štvorci cez už existujúci FindPointsRange
            var candidates = FindPointsRange(lat - radius, lon - radius, lat + radius, lon + radius);

            var result = new LinkedList<PointOfInterest>();
            double radiusSquared = radius * radius;

            // 2. Jemný filter – vyberieme len tie, ktoré reálne ležia v kruhu
            foreach (var poi in candidates)
            {
                double dLon = poi.Longitude - lon;
                double dLat = poi.Latitude - lat;

                if ((dLon * dLon) + (dLat * dLat) <= radiusSquared)
                {
                    result.AddLast(poi);
                }
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 64
        public LinkedList<PointOfInterest> FindRangeWithName(double minLat, double minLon, double maxLat, double maxLon, string name)
        {
            var candidates = FindPointsRange(minLat, minLon, maxLat, maxLon);
            var result = new LinkedList<PointOfInterest>();

            if (name == null)
            {
                return result;
            }

            // Nastavenie porovnávania textu: ignoruje diakritiku (napr. 'ž' == 'z') a veľkosť písmen (napr. 'A' == 'a')
            var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            var compareOptions = CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;

            foreach (var poi in candidates)
            {
                // Nazov musi text obsahovat, nie sa mu rovnat
                if (poi.Name != null && compareInfo.IndexOf(poi.Name, name, compareOptions) >= 0)
                {
                    result.AddLast(poi);
                }
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 55 
        public LinkedList<PointOfInterest> FindPointsRange(double minLat, double minLon, double maxLat, double maxLon)
        {
            if (_qTree != null)
            {
                return _qTree.FindRange(minLon, minLat, maxLon, maxLat);
            }

            if (_hybridQTree != null)
            {
                return _hybridQTree.FindRange(minLon, minLat, maxLon, maxLat);
            }

            if (_kdTree != null)
            {
                var minKeys = new LinkedList<KDKey<double>>();
                minKeys.AddLast(new KDKey<double>(minLon));
                minKeys.AddLast(new KDKey<double>(minLat));

                var maxKeys = new LinkedList<KDKey<double>>();
                maxKeys.AddLast(new KDKey<double>(maxLon));
                maxKeys.AddLast(new KDKey<double>(maxLat));

                var nodes = _kdTree.FindRange(minKeys, maxKeys);
                var result = new LinkedList<PointOfInterest>();

                foreach (var node in nodes)
                {
                    foreach (var poi in node.Data)
                    {
                        result.AddLast(poi);
                    }
                }
                return result;
            }

            throw new InvalidOperationException("Ziadny strom nie je inicializovany.");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 55 
        public enum TreeType
        {
            QTree,
            HybridQTree,
            KDTree
        }
    }
}
