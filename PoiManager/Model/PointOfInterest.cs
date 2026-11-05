using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PoiManager.Model
{
    public class PointOfInterest
    {
        public long Id { get; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public String Name { get; private set; }
        public String Description { get; private set; }
        public String? Address { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime LastUpdatedAt { get; private set; }
        public bool WasDeleted { get; private set; }

        public PointOfInterest(long id, double latitude, double longitude, String name, String description, String? address)
        {
            Id = id;
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
            Description = description;
            Address = address;
            CreatedAt = DateTime.Now;
            LastUpdatedAt = DateTime.Now;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 66 (import z CSV so zachovanim casov)
        public PointOfInterest(long id, double latitude, double longitude, String name, String description, String? address,
                               DateTime createdAt, DateTime lastUpdatedAt)
            : this(id, latitude, longitude, name, description, address)
        {
            CreatedAt = createdAt;
            LastUpdatedAt = lastUpdatedAt;
        }

        // Keďže adresa môže byť prázdna, pri aktualizácii potrebujem všetky hodnoty
        public void UpdateValues(double latitude, double longitude, String name, String description, String? address)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
            Description = description;
            Address = address;
            LastUpdatedAt = DateTime.Now;
        }

        internal void MarkDeleted()
        {
            WasDeleted = true;
        }
    }
}
