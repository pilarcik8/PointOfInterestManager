using System;
using System.Collections.Generic;
using AUS2_S1;
using AUS2_S1.Hybrid_Quad_Tree;
using AUS2_S1.KD_Tree;
using NetTopologySuite.Geometries.Implementation;

namespace AUS2_S1.APP
{
    internal class Controler
    {
        private QTree<PointOfInterest, double>? _qTree;
        private HybridQTree<PointOfInterest, double>? _hybridQTree;
        private KDTree<double, PointOfInterest>? _kdTree;
        private int UniqueID = 0;

        public Controler(
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

        public LinkedList<PointOfInterest> FindPointsAroundCentre(double lat, double lon, double halfDistance)
        {
            if (_qTree != null)
            {
                //return _qTree.FindRange(minLon, minLat, maxLon, maxLat);
            }

            if (_hybridQTree != null)
            {
                //return _hybridQTree.FindRange(minLon, minLat, maxLon, maxLat);
            }

            if (_kdTree != null)
            {
                var keyLon = new KDKey<double>(lon);
                var keyLat = new KDKey<double>(lat);

                var keys = new List<KDKey<double>>
                {
                    keyLon,
                    keyLat
                };

                var nodes = _kdTree.FindRadius(center: keys, halfDistance);

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

        public enum TreeType
        {
            QTree,
            HybridQTree,
            KDTree
        }
    }
}
