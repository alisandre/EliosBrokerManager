using EliosBrokerManager.DBContext;
using EliosBrokerManager.Models.Elios;
using EliosBrokerManager.Models.Jibria;
using EliosBrokerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace EliosBrokerManager.Providers
{
    public class JibriaQueueProvider
    {                
        static string STATUS_DA_INVIARE = "IN_ATTESA";
        static string STATUS_INVIATO = "INVIATO";
        static string STATUS_ESEGUITO= "ESEGUITO";
        static string STATUS_IN_ERRORE = "IN_ERRORE";
        static string STATUS_COMPLETATO = "COMPLETATO";

        private readonly ILogger<QueueWorker> _logger;
        private readonly IConfiguration _configuration;
        private JibriaDBContext dbContext;

        public JibriaQueueProvider(ILogger<QueueWorker> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
            dbContext = new JibriaDBContext(config);
        }

        public List<EliosQueueItem> GetJibriaEliosQueue()
        {
           return dbContext.EliosQueue.ToList();
        }
        
        public List<EliosQueueItem> GetJibriaEliosQueueToSend()
        {
            var queueItems = dbContext.EliosQueue.Where(qi => qi.StatoInvio.ToUpper() == STATUS_DA_INVIARE).ToList();
            foreach (var item in queueItems)
            {
                var imp = dbContext.Impegnative.Where(i => i.CodiceImpegnativa.ToString() == item.IdAccettazione);
                item.NumeroImpegnativa = imp.Count()>0 ? imp.FirstOrDefault().NumeroImpegnativa : string.Empty;
                item.DataImpegnativa = imp.Count() > 0 ? imp.FirstOrDefault().DataImpegnativa : DateTime.MinValue;
                _logger.LogInformation($"EliosQueueItem da inviare: Codice: {item.Codice}, IdAccettazione: {item.IdAccettazione}, DataAccettazione: {item.DataAccettazione}, IdPaziente: {item.IdPaziente}, Cognome: {item.Cognome}, Nome: {item.Nome}, DataNascita: {item.DataNascita}, CodiceFiscale: {item.CodiceFiscale}, CodiceEsame: {item.CodiceEsame}, DescrizioneEsame: {item.DescrizioneEsame}, DataInserimento: {item.DataInserimento}, StatoInvio: {item.StatoInvio}, DataInvio: {item.DataInvio}, NoteErrore: {item.NoteErrore}, StatoPacs: {item.StatoPacs}, DataUltAggPacs: {item.DataUltAggPacs}, ErrorePacs: {item.ErrorePacs}");
            }

            return dbContext.EliosQueue.Where(qi=> qi.StatoInvio.ToUpper() == STATUS_DA_INVIARE).ToList();
        }

        public List<EliosQueueItem> GetJibriaEliosQueueSent()
        {
            DateTime dtOffset = DateTime.Now.AddDays(-1);

            return dbContext.EliosQueue.Where(qi => qi.StatoInvio.ToUpper() != STATUS_DA_INVIARE && (qi.DataInvio >= dtOffset || qi.DataInserimento >= dtOffset || qi.DataUltAggPacs>= dtOffset)).ToList();
        }

        public List<EliosQueueItem> GetJibriaEliosFeedbackToSend()
        {
            return dbContext.EliosQueue.Where(qi => qi.StatoPacs.ToUpper() == "110").ToList();
        }

        public void SetAsSent(EliosQueueItem eliosQueueItem)
        {
            dbContext.Database.BeginTransaction();

            dbContext.EliosQueue.Where(qi => qi.Codice == eliosQueueItem.Codice).ToList().ForEach(qi =>
            {
                qi.StatoInvio = STATUS_INVIATO;
                qi.DataInvio = DateTime.Now;
            });

            dbContext.SaveChanges();

            dbContext.Database.CommitTransaction();
        }

        public void SetEliosStatus(EliosQueueItem eliosQueueItem)
        {
            dbContext.Database.BeginTransaction();

            dbContext.EliosQueue.Where(qi => qi.IdAccettazione == eliosQueueItem.IdAccettazione).ToList().ForEach(qi =>
            {
                qi.StatoPacs = eliosQueueItem.StatoPacs;
                qi.DataUltAggPacs = eliosQueueItem.DataUltAggPacs;
                qi.ErrorePacs = eliosQueueItem.ErrorePacs;
            });

            dbContext.SaveChanges();

            dbContext.Database.CommitTransaction();
        }

        public void SetAsDone(EliosQueueItem eliosQueueItem)
        {
            dbContext.Database.BeginTransaction();

            dbContext.EliosQueue.Where(qi => qi.IdAccettazione == eliosQueueItem.IdAccettazione).ToList().ForEach(qi =>
            {
                qi.StatoInvio = STATUS_ESEGUITO;                
            });

            dbContext.SaveChanges();

            dbContext.Database.CommitTransaction();
        }

        public void SetAsError(EliosQueueItem eliosQueueItem)
        {
            dbContext.Database.BeginTransaction();

            dbContext.EliosQueue.Where(qi => qi.IdAccettazione == eliosQueueItem.IdAccettazione).ToList().ForEach(qi =>
            {
                qi.StatoInvio = STATUS_IN_ERRORE;
            });

            dbContext.SaveChanges();

            dbContext.Database.CommitTransaction();
        }

        public void SetAsCompleted(EliosQueueItem eliosQueueItem)
        {
            dbContext.Database.BeginTransaction();

            dbContext.EliosQueue.Where(qi => qi.IdAccettazione == eliosQueueItem.IdAccettazione).ToList().ForEach(qi =>
            {
                qi.StatoInvio = STATUS_COMPLETATO;
            });

            dbContext.SaveChanges();

            dbContext.Database.CommitTransaction();
        }
    }
}
