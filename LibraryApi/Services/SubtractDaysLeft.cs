
using LibraryApi.Data;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    public class SubtractDaysLeft : IHostedService, IDisposable
    {
        private Timer? _timer = null;
        private readonly ILogger<SubtractDaysLeft> _logger;
        private readonly DataContextDapperMaster _dapperAdd;
        private readonly DataContextDapperSlave _dapperRead;

        public SubtractDaysLeft(ILogger<SubtractDaysLeft> logger, IConfiguration config)
        {
            _logger = logger;
            _dapperAdd = new DataContextDapperMaster(config);
            _dapperRead = new DataContextDapperSlave(config);
        }
        private void DoWork(object? state)
        {
            _logger.LogInformation("Checking days");
            if(DateTime.Now.Hour == 0 && DateTime.Now.Minute <= 30)
            {
                _logger.LogInformation("SUBTRACTED 1 DAY FROM ALL BORROWINGS");
                _dapperAdd.ExecuteSql("UPDATE public.borrowing SET borrowing_length = borrowing_length - 1");
            }
        }
        void IDisposable.Dispose()
        {
            _timer?.Dispose();
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SubtractDaysLeft service is running.");
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
