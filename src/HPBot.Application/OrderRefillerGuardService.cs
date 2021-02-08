using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class OrderRefillerGuardService
    {
        private readonly OrderRefillerService orderRefillerService;
        private readonly ILogger logger;

        public OrderRefillerGuardService(OrderRefillerService orderRefillerService, ILoggerFactory loggerFactory)
        {
            this.orderRefillerService = orderRefillerService ?? 
                throw new ArgumentNullException(nameof(orderRefillerService));
            logger = loggerFactory?.CreateLogger<OrderRefillerGuardService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await orderRefillerService.RefillRunningOrderIfApplicableAsync();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "An error has occurred on OrderRefillerGuardService main loop.");
                }

                await Task.Delay(60000);
            }

            logger.LogWarning("OrderRefillerGuardService was cancelled!");
        }
    }
}
