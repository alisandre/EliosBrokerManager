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

            _logger.LogInformation("QueueWorker creato, intervallo: {Interval}s", _pollInterval.TotalSeconds);

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
                    _logger.LogDebug("Inizio ciclo di elaborazione coda");
                    
                    List<EliosQueueItem> jibriaEliosQueue;
                    List<EliosQueueItem> eliosFeedbackQueue;

                    //trova quelli da inviare
                    _logger.LogDebug("FASE 1: Recupero elementi da inviare a Elios");
                    jibriaEliosQueue = _jibriaQueueProvider.GetJibriaEliosQueueToSend();
                    _logger.LogInformation("Trovati {Count} elementi da inviare a Elios", jibriaEliosQueue.Count);

                    int sentCount = 0;
                    int failedCount = 0;
                    foreach (EliosQueueItem eliosQueueItem in jibriaEliosQueue)
                    {
                        _logger.LogDebug("Invio elemento - IdAccettazione: {IdAccettazione}, Codice: {Codice}", 
                            eliosQueueItem.IdAccettazione, eliosQueueItem.Codice);
                        
                        bool insertResult = _eliosBrokerProvider.InsertJibriaQueueItem(eliosQueueItem);
                        if (insertResult)
                        {
                            _jibriaQueueProvider.SetAsSent(eliosQueueItem);
                            sentCount++;
                            _logger.LogDebug("Elemento inviato con successo - IdAccettazione: {IdAccettazione}", 
                                eliosQueueItem.IdAccettazione);
                        }
                        else
                        {
                            _jibriaQueueProvider.SetAsError(eliosQueueItem);
                            failedCount++;
                            _logger.LogWarning("Fallito invio elemento - IdAccettazione: {IdAccettazione}",  eliosQueueItem.IdAccettazione);
                        }
                    }
                    _logger.LogInformation("FASE 1 completata: {Sent} inviati, {Failed} falliti", sentCount, failedCount);

                    _logger.LogDebug("Pausa di 1 secondo tra le fasi");
                    Task.Delay(1000).Wait();

                    //trova quelli da leggere dal pacs
                    _logger.LogDebug("FASE 2: Recupero feedback dal PACS");
                    eliosFeedbackQueue = _eliosBrokerProvider.GetPACSFeedback();
                    _logger.LogInformation("Trovati {Count} feedback dal PACS", eliosFeedbackQueue.Count);

                    int doneCount = 0;
                    int errorCount = 0;
                    foreach (EliosQueueItem eliosFeedbackQueueItem in eliosFeedbackQueue)
                    {
                        _logger.LogDebug("Elaborazione feedback - IdAccettazione: {IdAccettazione}, StatoPacs: {StatoPacs}", 
                            eliosFeedbackQueueItem.IdAccettazione, eliosFeedbackQueueItem.StatoPacs);
                        
                        _jibriaQueueProvider.SetEliosStatus(eliosFeedbackQueueItem);

                        if (eliosFeedbackQueueItem.StatoPacs == "110")
                        {
                            _jibriaQueueProvider.SetAsDone(eliosFeedbackQueueItem);
                            doneCount++;
                            _logger.LogDebug("Feedback completato - IdAccettazione: {IdAccettazione}", 
                                eliosFeedbackQueueItem.IdAccettazione);
                        }
                        else
                        {
                            _jibriaQueueProvider.SetAsError(eliosFeedbackQueueItem);
                            errorCount++;
                            _logger.LogWarning("Feedback con errore - IdAccettazione: {IdAccettazione}, StatoPacs: {StatoPacs}, Errore: {Errore}", 
                                eliosFeedbackQueueItem.IdAccettazione, eliosFeedbackQueueItem.StatoPacs, eliosFeedbackQueueItem.ErrorePacs);
                        }
                    }
                    _logger.LogInformation("FASE 2 completata: {Done} completati, {Error} con errori", doneCount, errorCount);

                    _logger.LogDebug("Pausa di 1 secondo tra le fasi");
                    Task.Delay(1000).Wait();

                    //trova quelli da completare
                    _logger.LogDebug("FASE 3: Recupero feedback da inviare a Jibria");
                    jibriaEliosQueue = _jibriaQueueProvider.GetJibriaEliosFeedbackToSend();
                    _logger.LogInformation("Trovati {Count} feedback da inviare a Jibria", jibriaEliosQueue.Count);

                    int completedCount = 0;
                    int updateFailedCount = 0;
                    foreach (EliosQueueItem eliosQueueItem in jibriaEliosQueue)
                    {
                        _logger.LogDebug("Invio feedback a Jibria - IdAccettazione: {IdAccettazione}, Codice: {Codice}", 
                            eliosQueueItem.IdAccettazione, eliosQueueItem.Codice);
                        
                        bool updateResult = _eliosBrokerProvider.SetJibriaFeedback(eliosQueueItem);
                        if (updateResult)
                        {
                            _jibriaQueueProvider.SetAsCompleted(eliosQueueItem);
                            completedCount++;
                            _logger.LogDebug("Feedback inviato e completato - IdAccettazione: {IdAccettazione}", 
                                eliosQueueItem.IdAccettazione);
                        }
                        else
                        {
                            updateFailedCount++;
                            _logger.LogWarning("Fallito invio feedback - IdAccettazione: {IdAccettazione}", 
                                eliosQueueItem.IdAccettazione);
                        }
                    }
                    _logger.LogInformation("FASE 3 completata: {Completed} completati, {Failed} falliti", completedCount, updateFailedCount);
                    
                    _logger.LogDebug("Fine ciclo di elaborazione coda");
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