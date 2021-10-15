using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Plugin1
{
    public class PrimeiroPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity evento = GetEventoEntity(context);

            if (evento.Contains("fyi_tabelaeventosid"))
            {
                EntityCollection eventos = RetrieveNumeroUltimoEvento(service, evento.Id);

                int ultimoEvento = 0;

                tracingService.Trace($"PLUGIN 01: ANTES DE VARRER O ARRAY");

                foreach (Entity cadaEvento in eventos.Entities)
                {
                    int numerodoevento = cadaEvento.Contains("fyi_numerodoevento") ? (int)cadaEvento["fyi_numerodoevento"] : 0;
                    if (numerodoevento > ultimoEvento)
                    {
                        ultimoEvento = numerodoevento;
                    }
                }

                tracingService.Trace($"PLUGIN 01: {ultimoEvento} é o maior evento registrado");
                
                UpdateNumeroUltimoEvento(context, service, evento, ultimoEvento);
            }
            else
            {
                throw new InvalidPluginExecutionException("Event is missing...");
            }
        }

        private static void UpdateNumeroUltimoEvento(IPluginExecutionContext context, IOrganizationService service, Entity evento, int totalEventos)
        {
            if (context.MessageName == "Create" || context.MessageName == "Update")
            {
                evento["fyi_numerodoevento"] = totalEventos + 1;
            }

            service.Update(evento);
        }

        private static Entity RetrieveEvento(IOrganizationService service, Guid numeroDoEventoId)
        {
            return service.Retrieve("fyi_numerodoevento", numeroDoEventoId, new ColumnSet("fyi_numeroultimoevento"));
        }

        private static int GetNumeroUltimoEvento(IOrganizationService service, Guid numeroDoEventoId, Entity numeroDoEvento)
        {
            return numeroDoEvento.Contains("fyi_numeroultimoevento") ? (int)numeroDoEvento["fyi_numeroultimoevento"] : 0;
        }

        private static Entity GetEventoEntity(IPluginExecutionContext context)
        {
            Entity evento = new Entity();

            if (context.MessageName == "Create" || context.MessageName == "Update")
            {
                evento = (Entity)context.InputParameters["Target"];
            }

            return evento;
        }

        private static EntityCollection RetrieveNumeroUltimoEvento(IOrganizationService service, Guid idEvento)
        {
            QueryExpression queryEventos = new QueryExpression("fyi_tabelaeventos");
            queryEventos.ColumnSet.AddColumns("createdon", "fyi_numerodoevento");

            return service.RetrieveMultiple(queryEventos);
        }
    }
}
