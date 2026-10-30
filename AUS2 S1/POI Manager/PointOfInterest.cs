using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AUS2_S1.APP
{
    public class PointOfInterest
    {
        public int Id { get; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public String Name { get; private set; }
        public String Description { get; private set; }
        public String? Address { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime LastUpdatedAt { get; private set; }

        public PointOfInterest(int id, double latitude, double longitude, String name, String description, String? address)
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

        // Since address can be empthy, I need all the values for update because I cannot be sure if address is set to null or not to be changed
        public void UpdateValues(double latitude, double longitude, String name, String description, String? address)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
            Description = description;
            Address = address;
            LastUpdatedAt = DateTime.Now;
        }
    }
}
