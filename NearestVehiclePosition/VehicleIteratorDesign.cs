using NearestVehiclePosition.Models;
using System.Reflection;
using System.IO;

namespace NearestVehiclePosition
{
    /// Abstraction is used to read binary data in parts 
    public interface IVehicleIteratorDesign : IDisposable
    { 
        /// Identifies If read operation has reached its limit
        bool HasLimitReached { get; }

        /// Returns the vehicle details from binary data
        /// <returns></returns>
        VehicleDetails NextVehicle();

        /// Total binary data size/ Length
        long Length { get; }
    }
    public class VehicleIteratorDesign: IVehicleIteratorDesign
    {
        private bool _disposed = false;
        private FileStream _fsVehiclePositions;
        private long _postion;
        private long _limit;

        /// <summary>
        /// Default constructor used to get binary data size
        /// </summary>
        public VehicleIteratorDesign()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"VehiclePositions.dat");
            _fsVehiclePositions = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Constructor that initializes position and limit to read binary data in multiple parts
        /// </summary>
        /// <param name="postion">postion to start reading</param>
        /// <param name="limit">limit to stop reading</param>
        public VehicleIteratorDesign(long postion, long limit)
        {
            _postion = postion;
            _limit = limit;
            string fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"VehiclePositions.dat");
            _fsVehiclePositions = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            _fsVehiclePositions.Seek(_postion, SeekOrigin.Begin);
        }

        /// <summary>
        /// Identifies If read operation has reached its limit
        /// </summary>
        public bool HasLimitReached
        {
            get { return _postion >= _limit; }
        }

        /// <summary>
        /// Total binary data size/ Length
        /// </summary>
        public long Length
        {
            get { return _fsVehiclePositions.Length; }
        }

        /// <summary>
        /// Returns the next vehicle details from binary data
        /// </summary>
        /// <returns></returns>
        public VehicleDetails NextVehicle()
        {
            VehicleDetails vehicleDetails = null;
            if (_postion < _limit)
            {
                vehicleDetails = new VehicleDetails();

                // read position id
                byte[] positionArray = new byte[4];
                _fsVehiclePositions.Read(positionArray, 0, 4);
                vehicleDetails.PositionId = BitConverter.ToInt32(positionArray, 0);

                // read vehicle registration
                List<byte> vehicleRegistratonBytes = new List<byte>();
                byte readByte = (byte)_fsVehiclePositions.ReadByte();
                while (readByte != char.MinValue)
                {
                    vehicleRegistratonBytes.Add(readByte);
                    readByte = (byte)_fsVehiclePositions.ReadByte();
                }
                vehicleDetails.VehicleRegistration = BitConverter.ToString(vehicleRegistratonBytes.ToArray());

                // read latitude
                byte[] latArray = new byte[4];
                _fsVehiclePositions.Read(latArray, 0, 4);
                vehicleDetails.Latitude = BitConverter.ToSingle(latArray, 0);

                // read longitude
                byte[] longArray = new byte[4];
                _fsVehiclePositions.Read(longArray, 0, 4);
                vehicleDetails.Longitude = BitConverter.ToSingle(longArray, 0);

                // read recordedTime
                byte[] recTimeArray = new byte[8];
                _fsVehiclePositions.Read(recTimeArray, 0, 8);
                var recordedTime = (long)BitConverter.ToUInt64(recTimeArray, 0);
                vehicleDetails.RecordedDate = DateTimeOffset.FromUnixTimeSeconds(recordedTime);
                _postion = _fsVehiclePositions.Position;
            }
            return vehicleDetails;
        }

        /// <summary>
        /// Implements the IDisposable to take care of freeing the file resources from memory
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implements the IDisposable to take care of freeing the file resources from memory
        /// Closes and disposes the file stream object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                _fsVehiclePositions.Close();
                _fsVehiclePositions.Dispose();
                _fsVehiclePositions = null;

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer takes care of closing the file stream
        /// incase consumer doesn't call dispose explicitely
        /// or doesn't use this class with using statement
        /// </summary>
        ~VehicleIteratorDesign()
        {
            Dispose(disposing: false);
        }
    }
}
