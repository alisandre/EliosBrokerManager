using EliosBrokerManager.Models.Jibria;
using EliosBrokerManager.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EliosBrokerService
{
    public class QueueWorker : BackgroundService
    {
        private readonly ILogger<QueueWorker> _logger;               
        private readonly int POLLING_SECONDS = 30;
        private readonly TimeSpan _pollInterval;
        private JibriaQueueProvider _jibriaQueueProvider;
        private EliosBrokerProvider _eliosBrokerProvider;
        private readonly IConfiguration _configuration;

        public QueueWorker(ILogger<QueueWorker> logger, IConfiguration config)
        {

            _logger = logger;

            _configuration = config;

            _pollInterval = TimeSpan.FromSeconds(POLLING_SECONDS);

            _jibriaQueueProvider = new JibriaQueueProvider(_logger, config);

            _eliosBrokerProvider = new EliosBrokerProvider(_logger, config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QueueWorker avviato, intervallo: {Interval}s", _pollInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<EliosQueueItem> jibriaEliosQueue;
                    List<EliosQueueItem> eliosFeedbackQueue;

                    //trova quelli da inviare

                    jibriaEliosQueue = _jibriaQueueProvider.GetJibriaEliosQueueToSend();

                    foreach (EliosQueueItem eliosQueueItem in jibriaEliosQueue)
                    {
                        bool insertResult = _eliosBrokerProvider.InsertJibriaQueueItem(eliosQueueItem);
                        if (insertResult)
                        {
                            _jibriaQueueProvider.SetAsSent(eliosQueueItem);
                        }
                    }

                    Task.Delay(1000).Wait(); //pausa di 1 secondo tra invii

                    //trova quelli da leggere dal pacs

                    eliosFeedbackQueue = _eliosBrokerProvider.GetPACSFeedback();

                    foreach (EliosQueueItem eliosFeedbackQueueItem in eliosFeedbackQueue)
                    {
                        _jibriaQueueProvider.SetEliosStatus(eliosFeedbackQueueItem);

                        if (eliosFeedbackQueueItem.StatoPacs == "110")
                        {
                            _jibriaQueueProvider.SetAsDone(eliosFeedbackQueueItem);
                        }
                        else
                        {
                            _jibriaQueueProvider.SetAsError(eliosFeedbackQueueItem);
                        }
                    }

                    Task.Delay(1000).Wait(); //pausa di 1 secondo tra invii

                    //trova quelli da completare

                    jibriaEliosQueue = _jibriaQueueProvider.GetJibriaEliosFeedbackToSend();

                    foreach (EliosQueueItem eliosQueueItem in jibriaEliosQueue)
                    {
                        bool updateResult = _eliosBrokerProvider.SetJibriaFeedback(eliosQueueItem);
                        if (updateResult)
                        {
                            _jibriaQueueProvider.SetAsCompleted(eliosQueueItem);
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante l'elaborazione della coda");
                }

                try
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
                catch (TaskCanceledException) { /* stop requested */ }
            }

            _logger.LogInformation("QueueWorker arrestato");
        }
    }
}