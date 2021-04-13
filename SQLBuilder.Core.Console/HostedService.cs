using Elastic.Apm;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLBuilder.Core.Repositories;
using SQLBuilder.Core.Extensions;
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
                    var repository = new OracleRepository(@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=giteadev)));Persist Security Info=True;User ID=uid;Password=pwd;");

                    repository.PreCommitResultAsyncQueue.Enqueue(async repo =>
                    {
                        //return await repo.ExecuteBySqlAsync("DELETE FROM UC_USERS WHERE ID='2c22ef0a-ed9b-4cde-a3cf-d3fdb9b26fee'");
                        var res = await repo.FindEntityAsync<dynamic>("SELECT * FROM UC_USERS WHERE ID='2c22ef0a-ed9b-4cde-a3cf-d3fdb9b26fee'");
                        Console.WriteLine($"event1:{ObjectExtensions.ToJson(res)}");
                        return 1;
                    });

                    repository.PreCommitResultAsyncQueue.Enqueue(async repo =>
                    {
                        var res = await repo.FindEntityAsync<dynamic>("SELECT * FROM UC_USERS WHERE ROWNUM=1");
                        Console.WriteLine($"event2:{ObjectExtensions.ToJson(res)}");
                        return 1;
                    });

                    try
                    {
                        var res = await repository.CommitResultQueueAsync(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await Task.CompletedTask;
            });

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
