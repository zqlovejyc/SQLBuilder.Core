using Elastic.Apm;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLBuilder.Core.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SQLBuilder.Core
{
    public class HostedService : IHostedService
    {
        private readonly IApmAgent _apmAgent;
        private readonly ILogger _logger;

        public HostedService(IApmAgent apmAgent, ILogger<HostedService> logger) => (_apmAgent, _logger) = (apmAgent, logger);

        public async Task StartAsync(CancellationToken cancellationToken) =>
            await _apmAgent.Tracer.CaptureTransaction("Console .Net Core Test", "background", async () =>
            {
                Console.WriteLine("HostedService running");

                var currentTransaction = Agent.Tracer.CurrentTransaction;
                if (currentTransaction == null) 
                    throw new Exception("Agent.Tracer.CurrentTransaction returns null");

                try
                {
                    var repository = new OracleRepository(@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=db)));Persist Security Info=True;User ID=database;Password=pwd;");
                    var res = await repository.FindEntityAsync<dynamic>("SELECT * FROM USERS WHERE ROWNUM=1");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
