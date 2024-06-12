
using LibraryApi.Data;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    public class ValidateBookingService : IHostedService, IDisposable
    {
        private Timer? _timer = null;
        private readonly ILogger<ValidateBookingService> _logger;
        private readonly DataContextDapperMaster _dapperAdd;
        private readonly DataContextDapperSlave _dapperRead;

        public ValidateBookingService(ILogger<ValidateBookingService> logger, IConfiguration config)
        {
            _logger = logger;
            _dapperAdd = new DataContextDapperMaster(config);
            _dapperRead = new DataContextDapperSlave(config);
        }
        private void DoWork(object? state)
        {
            _logger.LogInformation("Checking bookings");
            List<Booking> bookings = _dapperRead.LoadData<Booking>("SELECT * FROM booking").ToList();
            foreach(var booking in bookings)
            {
                if (booking.Booking_date.AddDays(booking.Booking_length) < DateTime.Now)
                {
                    _dapperAdd.ExecuteSql($"DELETE FROM BOOKING WHERE booking_id = {booking.Booking_id}");
                    _logger.LogInformation("BOOKING DELETED");
                }
            }
        }
        void IDisposable.Dispose()
        {
            _timer?.Dispose();
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validate booking service is running.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
