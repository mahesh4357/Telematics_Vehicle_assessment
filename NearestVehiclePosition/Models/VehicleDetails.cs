using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestVehiclePosition.Models
{
    //internal class VehicleDetails
    //{
    //}

    public class VehicleDetails
    {
        public int PositionId { get; set; }
        public string VehicleRegistration { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public DateTimeOffset RecordedDate { get; set; }
    }

    /// <summary>
    /// Model class to represent input positions
    /// </summary>
    public class Position
    {
        public int PositionId { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
