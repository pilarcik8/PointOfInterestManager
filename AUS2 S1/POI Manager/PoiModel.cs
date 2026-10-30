using System;
using System.Collections.Generic;
using System.Globalization;
using AUS2_S1;
using AUS2_S1.Hybrid_Quad_Tree;
using AUS2_S1.KD_Tree;
using NetTopologySuite.Geometries.Implementation;

namespace AUS2_S1.APP
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

        public void ImportPointsOfInterest(string pathToCSV)
        {
            throw new Exception("Not inplemeted yet");
        }

        public void ExportPointsOfInterest(string pathToCSV)
        {
            throw new Exception("Not inplemeted yet");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 55
        public void CreatePOI(string name, double lat, double lon, string desc, string? addr = null)
        {
            var poi = new PointOfInterest(id: UniqueID, latitude: lat, longitude: lon, name: name,
                                        description: desc, address: addr);

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
            UniqueID++;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 60
        public void UpdatePOI(PointOfInterest poi, double lat, double lon, string name, string desc, string? addr = null)
        {
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
                if (poi.Name != null && compareInfo.Compare(poi.Name, name, compareOptions) == 0)
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
