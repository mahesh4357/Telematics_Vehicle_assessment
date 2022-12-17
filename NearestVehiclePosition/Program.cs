using NearestVehiclePosition.Models;
using System.Diagnostics;

namespace NearestVehiclePosition
{
    
    /// Entry point of this console application 
    class MainClass
    {
        /// <summary>
        /// Provided assessment input data here prepares by main function.
        /// 10 given positions is added and passed to the method that uses FinderNearestVehicle.
        /// </summary>
        /// <param name="args">args</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Exicution start time " + DateTime.Now);
            try
            {
                List<Position> positions = new List<Position>
                {
                    new Position { PositionId = 1, Latitude = 34.544909F, Longitude = -102.100843F },
                    new Position { PositionId = 2, Latitude = 32.345544F, Longitude = -99.123124F },
                    new Position { PositionId = 3, Latitude = 33.234235F, Longitude = -100.214124F },
                    new Position { PositionId = 4, Latitude = 35.195739F, Longitude = -95.348899F },
                    new Position { PositionId = 5, Latitude = 31.895839F, Longitude = -97.789573F },
                    new Position { PositionId = 6, Latitude = 32.895839F, Longitude = -101.789573F },
                    new Position { PositionId = 7, Latitude = 34.115839F, Longitude = -100.225732F },
                    new Position { PositionId = 8, Latitude = 32.335839F, Longitude = -99.992232F },
                    new Position { PositionId = 9, Latitude = 33.535339F, Longitude = -94.792232F },
                    new Position { PositionId = 10, Latitude = 32.234235F, Longitude = -100.222222F }
                };
                DisplayNearestVehicles(positions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error occured: {ex.Message}");
            }

            Console.WriteLine("Stop by Press any key");
            Console.WriteLine("Exicution End time " + DateTime.Now);
            Console.ReadLine();
        }

        private static void DisplayNearestVehicles(List<Position> positions)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // First cache the vehicle details from the dat file
            IFindNearestVehicle nearestVehicleFinder = new FindNearestVehicle();
            nearestVehicleFinder.CacheVehicles();

            // Execute find on multiple tasks in parallel for each of the position
            List<Task<(Position position, double minimumDistance, VehicleDetails nearestVehicle)>> tasks
                = new List<Task<(Position position, double minimumDistance, VehicleDetails nearestVehicle)>>();
            foreach (var position in positions)
            {
                Task<(Position position, double minimumDistance, VehicleDetails nearestVehicle)> task
                    = Task.Run(() => nearestVehicleFinder.Find(position));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                (Position position, double minimumDistance, VehicleDetails nearestVehicle) result = task.Result;
                if (result.nearestVehicle != null)
                {
                    Console.WriteLine($"Position: {result.position.PositionId} ({result.position.Latitude}, {result.position.Longitude})" +
                        $" has nearest Vehicle: {result.nearestVehicle.PositionId} {result.nearestVehicle.VehicleRegistration} " +
                        $"{result.nearestVehicle.Latitude} {result.nearestVehicle.Longitude}" +
                        $" at Minimum Distance of: {result.minimumDistance} meters");
                }
                else
                {
                    Console.WriteLine($"No nearest vehicle found for: " +
                        $"Position: {result.position.PositionId} ({result.position.Latitude}, {result.position.Longitude})");
                }
            }

            stopWatch.Stop();
            Console.WriteLine($"Total Time taken (seconds): {stopWatch.ElapsedMilliseconds / 1000}");
        }
    }
}