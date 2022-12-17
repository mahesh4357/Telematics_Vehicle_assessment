using NearestVehiclePosition.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using GeoCoordinatePortable;

namespace NearestVehiclePosition
{
    public interface IFindNearestVehicle
    {
        void CacheVehicles();
        (Position position, double minimumDistance, VehicleDetails nearestVehicle) Find(Position position);
    }

    public class FindNearestVehicle : IFindNearestVehicle
    {
        private bool isCached = false;
        private static ConcurrentBag<VehicleDetails> cachedVehicles = new ConcurrentBag<VehicleDetails>();

        public FindNearestVehicle()
        {
        }

        /// <summary>
        /// Below code is exicute once in the lifetime of the application.
        /// Below method is highly optimised to finish reading 4 million records within 2 second.
        /// Below method caches the binary data into a ConcurrentBag.
        /// Below logic ensures the heavy big size binary data reading operation is only performed
        /// Also to increase the reading speed, binary data is split in 4 different parts
        /// where reading start position and stop limit is calculated based on the total binary data size.
        /// Each of the part is triggered on seperate .NET Task executing in parallel,
        /// which ensures the read operation completes within 1 second.
        /// If not done in parts and in parallel, sequential read otherwise takes more than 6 seconds to
        /// read 2 million records.
        /// </summary>
        public void CacheVehicles()
        {
            if (isCached)
            {
                return;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Read vehicle details in parallel splitting data in 4 big chunks
            long dataSize;
            using (IVehicleIteratorDesign vehiclePositionsIterator = new VehicleIteratorDesign())
            {
                dataSize = vehiclePositionsIterator.Length;
            }
            Task part1 = Task.Run(() => ReadVehicleDetailsInChunks(0, dataSize / 4));
            Task part2 = Task.Run(() => ReadVehicleDetailsInChunks(dataSize / 4, (dataSize / 4) * 2));
            Task part3 = Task.Run(() => ReadVehicleDetailsInChunks((dataSize / 4) * 2, (dataSize / 4) * 3));
            Task part4 = Task.Run(() => ReadVehicleDetailsInChunks((dataSize / 4) * 3, dataSize));

            Task.WaitAll(new Task[] { part1, part2, part3, part4 });

            stopWatch.Stop();
            Console.WriteLine($"Cache file total Time (seconds): {stopWatch.ElapsedMilliseconds / 1000}");

            Console.WriteLine($"Vehicle Cached Count: {cachedVehicles.Count}");
            isCached = true;
        }

        /// <summary>
        /// Reads the next vehicle details in the given position and limit
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="limit">limit</param>
        private void ReadVehicleDetailsInChunks(long position, long limit)
        {
            using (IVehicleIteratorDesign vehiclePositionsIterator = new VehicleIteratorDesign(position, limit))
            {
                while (!vehiclePositionsIterator.HasLimitReached)
                {
                    VehicleDetails vehicleDetails = vehiclePositionsIterator.NextVehicle();
                    if (vehicleDetails != null)
                    {
                        cachedVehicles.Add(vehicleDetails);
                    }
                }
            }
        }

        /// <summary>
        /// Finds the nearest vehicle for given position
        /// </summary>
        /// <param name="position">input position</param>
        /// <returns>returns the nearest vehicle and its distance</returns>
        public (Position position, double minimumDistance, VehicleDetails nearestVehicle) Find(Position position)
        {
            VehicleDetails nearestVehicle = null;
            double minimumDistance = double.MaxValue;
            GeoCoordinate inputCoordinate = new GeoCoordinate(position.Latitude, position.Longitude);

            foreach (var vehicle in cachedVehicles)
            {
                GeoCoordinate compareToCoordinate = new GeoCoordinate(vehicle.Latitude, vehicle.Longitude);
                var distance = inputCoordinate.GetDistanceTo(compareToCoordinate);
                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                    nearestVehicle = vehicle;
                }
            }
            return new(position, minimumDistance, nearestVehicle);
        }
    }
}
