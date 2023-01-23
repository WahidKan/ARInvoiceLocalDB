using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net.NetworkInformation;
using System.Timers;
using Timer = System.Threading.Timer;

namespace SAP_ARInvoice.Service
{

    public class DIService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly ILogger<DIService> logger;

        public DIService(ILogger<DIService> logger)
        {
            this.logger = logger;
        }

     

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(o => {
                logger.LogInformation($"Background Service");
            },
          null,
          TimeSpan.Zero,
          TimeSpan.FromHours(2));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}

