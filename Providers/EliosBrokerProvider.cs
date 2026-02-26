using EliosBrokerManager.DBContext;
using EliosBrokerManager.Models.Elios;
using EliosBrokerManager.Models.Jibria;
using EliosBrokerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EliosBrokerManager.Providers
{
    public class EliosBrokerProvider
    {
        private readonly ILogger<QueueWorker> _logger;
        private readonly IConfiguration _configuration;
        private EliosDBContext dbContext;

        public EliosBrokerProvider(ILogger<QueueWorker> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
            dbContext = new EliosDBContext(config);
        }

        public bool InsertJibriaQueueItem(EliosQueueItem eliosQueueItem)
        {

            dbContext.Database.BeginTransaction();

            try
            {
                // Controllo se il paziente esiste già

                bool patientExists = dbContext.Paziente.Any(p => p.CodFisc != null && p.CodFisc == eliosQueueItem.CodiceFiscale);

                Paziente paz;

                if (patientExists)
                {
                    paz = dbContext.Paziente.FirstOrDefault(p => p.CodFisc != null && p.CodFisc == eliosQueueItem.CodiceFiscale);
                }
                else
                {
                    paz = new Paziente();
                    paz.InsData = DateTime.Now;
                }

                paz.IdPazienteEsterno = eliosQueueItem.IdPaziente;
                paz.Cognome = eliosQueueItem.Cognome;
                paz.Nome = eliosQueueItem.Nome;
                paz.TipoCf = "CFI";
                paz.CodFisc = eliosQueueItem.CodiceFiscale;
                paz.DataNascita = eliosQueueItem.DataNascita;
                paz.Email = string.Empty;
                paz.IstatComuneNas = string.Empty;
                paz.IstatComuneRes = string.Empty;
                paz.Privacy = DateTime.Now;
                paz.Sesso = string.Empty;

                if (!patientExists) dbContext.Paziente.Add(paz);

                dbContext.SaveChanges();

                // Controllo se l'esame esiste

                bool esameExists = dbContext.TabEsame.Any(a => a.IdEsameEsterno == eliosQueueItem.CodiceEsame);

                TabEsame esame;

                if (esameExists)
                {
                    esame = dbContext.TabEsame.FirstOrDefault(a => a.IdEsameEsterno == eliosQueueItem.CodiceEsame);
                }
                else
                {
                    esame = new TabEsame();
                }

                esame.CodMin = eliosQueueItem.CodiceEsame;
                esame.IdEsameEsterno = eliosQueueItem.CodiceEsame;
                esame.Descrizione = eliosQueueItem.DescrizioneEsame;

                if (!esameExists) dbContext.TabEsame.Add(esame);

                dbContext.SaveChanges();

                // Controllo se l'accettazione esiste

                bool accettazioneExists = dbContext.Accettazione.Any(a => a.IdAccEsterno == eliosQueueItem.IdAccettazione);

                Accettazione acc;

                //if (accettazioneExists)
                //{
                //    acc = dbContext.Accettazione.FirstOrDefault(a => a.IdAccEsterno == eliosQueueItem.IdAccettazione);
                //}
                //else
                //{
                //    acc = new Accettazione();
                //    acc.InsData = DateTime.Now;
                //}

                if (accettazioneExists) throw new Exception("Esame già esistente.");

                acc = new Accettazione();
                acc.InsData = DateTime.Now;
                acc.IdPazienteBroker = paz.IdPazienteBroker;
                acc.IdAccEsterno = eliosQueueItem.IdAccettazione;
                acc.FlgMod = 1;
                acc.DataAcc = eliosQueueItem.DataAccettazione;
                acc.EsternoStato = 10; //-->acc.EliosStato = 20; // Stato "Inviato"
                acc.EsternoStatoData = DateTime.Now;
                acc.DataImp = eliosQueueItem.DataImpegnativa;

                var imp1 = "XXXXX";
                var imp2 = "XXXXXXXXXX"; 

                if (!string.IsNullOrEmpty(eliosQueueItem.NumeroImpegnativa) && eliosQueueItem.NumeroImpegnativa.Length == 15)
                {
                    imp1 = eliosQueueItem.NumeroImpegnativa.Substring(0, 5);
                    imp2 = eliosQueueItem.NumeroImpegnativa.Substring(5, 10);
                }

                acc.NumImp1 = imp1;
                acc.NumImp2 = imp2;
                acc.CodPriorita = 'Z';

                if (!accettazioneExists) dbContext.Accettazione.Add(acc);

                dbContext.SaveChanges();

                //recupero di nuovo l'accettazione per avere l'id corretto
                acc = dbContext.Accettazione.FirstOrDefault(a => a.IdAccEsterno == eliosQueueItem.IdAccettazione);

                // controllo se il dettaglio accettazione esiste

                bool accettazioneDettExists = dbContext.AccettazioneDett.Any(ad => ad.IdEsameBroker == esame.IdEsameBroker && ad.IdAccBroker == acc.IdAccBroker);

                AccettazioneDett accDett;

                if (accettazioneDettExists)
                {
                    accDett = dbContext.AccettazioneDett.FirstOrDefault(ad => ad.IdEsameBroker == esame.IdEsameBroker && ad.IdAccBroker == acc.IdAccBroker);
                }
                else
                {
                    accDett = new AccettazioneDett();
                    accDett.InsData = DateTime.Now;
                }

                accDett.IdAccBroker = acc.IdAccBroker;
                accDett.IdEsameBroker = esame.IdEsameBroker;
                accDett.AccessionNumber = eliosQueueItem.IdAccettazione;
                accDett.EsternoStato = 70; //-->acc.EliosStato = 80; // Stato "Inviato"             
                accDett.DataPrenotazione = eliosQueueItem.DataAccettazione;
                accDett.EsternoStatoData = DateTime.Now;
                accDett.FlgInvioPortale = 0;

                if (!accettazioneDettExists) dbContext.AccettazioneDett.Add(accDett);

                dbContext.SaveChanges();

                _logger.LogDebug("Paziente salvato - IdPazienteBroker: {Id}, CF: {CF}", paz.IdPazienteBroker, paz.CodFisc);

                // Verifica che l'ID sia stato generato
                if (paz.IdPazienteBroker == 0)
                {
                    throw new Exception("IdPazienteBroker non generato correttamente");
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = false
                };

                _logger.LogDebug("Paziente: {Paz}", JsonSerializer.Serialize(paz, jsonOptions));
                _logger.LogDebug("Accettazione: {Acc}", JsonSerializer.Serialize(acc, jsonOptions));
                _logger.LogDebug("AccettazioneDett: {AccDett}", JsonSerializer.Serialize(accDett, jsonOptions));

                dbContext.Database.CommitTransaction();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertJibriaQueueItem - Errore durante l'inserimento. IdAccettazione: {IdAccettazione}, CF: {CF}, CodiceEsame: {CodiceEsame}",
                eliosQueueItem.IdAccettazione,
                eliosQueueItem.CodiceFiscale,
                eliosQueueItem.CodiceEsame);

                // Log dell'inner exception per dettagli specifici
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    _logger.LogError("Inner Exception: {Message}", innerEx.Message);
                    innerEx = innerEx.InnerException;
                }

                dbContext.Database.RollbackTransaction();

                return false;
            }

        }

        public List<EliosQueueItem> GetPACSFeedback()
        {
            List<EliosQueueItem> eliosQueueItems = new List<EliosQueueItem>();

            try
            {
                DateTime dtOffset = DateTime.Now.AddDays(-1);

                var accettazioni = dbContext.Accettazione.Where(a => a.EliosStatoData >= dtOffset).ToList();

                foreach (var acc in accettazioni)
                {
                    var paz = dbContext.Paziente.FirstOrDefault(p => p.IdPazienteBroker == acc.IdPazienteBroker);
                    var accDettList = dbContext.AccettazioneDett.Where(ad => ad.IdAccBroker == acc.IdAccBroker).ToList();

                    foreach (var accDett in accDettList)
                    {
                        var esame = dbContext.TabEsame.FirstOrDefault(e => e.IdEsameBroker == accDett.IdEsameBroker);

                        EliosQueueItem item = new EliosQueueItem
                        {
                            Codice = 0,
                            IdAccettazione = acc.IdAccEsterno,
                            StatoPacs = accDett.EliosStato.ToString(),
                            DataUltAggPacs = accDett.EliosStatoData,
                            ErrorePacs = accDett.EliosNote
                        };

                        eliosQueueItems.Add(item);
                    }

                }

            }
            catch (Exception ex)
            {
                // Gestione dell'eccezione (ad esempio, log dell'errore)
                eliosQueueItems = new List<EliosQueueItem>();
            }


            return eliosQueueItems;

        }

        public bool SetJibriaFeedback(EliosQueueItem eliosQueueItem)
        {

            dbContext.Database.BeginTransaction();

            try
            {
                // Controllo se l'esame esiste

                bool esameExists = dbContext.TabEsame.Any(a => a.IdEsameEsterno == eliosQueueItem.CodiceEsame);

                var esame = dbContext.TabEsame.FirstOrDefault(a => a.IdEsameEsterno == eliosQueueItem.CodiceEsame);

                // Controllo se l'accettazione esiste

                bool accettazioneExists = dbContext.Accettazione.Any(a => a.IdAccEsterno == eliosQueueItem.IdAccettazione);

                if (!accettazioneExists) throw new Exception("Accettazione non trovata");

                Accettazione acc = dbContext.Accettazione.FirstOrDefault(a => a.IdAccEsterno == eliosQueueItem.IdAccettazione);

                acc.EsternoStato = 120;
                acc.EsternoStatoData = DateTime.Now;

                dbContext.SaveChanges();

                // controllo se il dettaglio accettazione esiste

                bool accettazioneDettExists = dbContext.AccettazioneDett.Any(ad => ad.IdEsameBroker == esame.IdEsameBroker && ad.IdAccBroker == acc.IdAccBroker);

                if (!accettazioneDettExists) throw new Exception("Dettaglio accettazione non trovato");

                AccettazioneDett accDett = dbContext.AccettazioneDett.FirstOrDefault(ad => ad.IdEsameBroker == esame.IdEsameBroker && ad.IdAccBroker == acc.IdAccBroker);

                accDett.EsternoStato = 120;
                accDett.EsternoStatoData = DateTime.Now;

                dbContext.SaveChanges();

                dbContext.Database.CommitTransaction();

                return true;
            }
            catch (Exception ex)
            {

                dbContext.Database.RollbackTransaction();

                return false;
            }

        }

    }
}