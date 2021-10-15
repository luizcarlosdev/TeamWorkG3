using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin2
{
    public class SegundoPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity ConviteDoEvento = GetConviteDoEventoEntity(context, tracingService);
            try
            {
                if (ConviteDoEvento.Contains("fyi_conta"))
                {
                    UpdateAccount(context, service, ConviteDoEvento, tracingService);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }
        private static void UpdateAccount(IPluginExecutionContext context, IOrganizationService service, Entity ConviteDoEvento, ITracingService tracingService)
        {
            Guid IDcontaDoConvite = ((EntityReference)ConviteDoEvento["fyi_conta"]).Id;
            Guid IDdoEvento = ((EntityReference)ConviteDoEvento["fyi_evento"]).Id;

            Entity account = RetriveAccount(service, IDcontaDoConvite);

            if (context.MessageName == "Create")
            {
                account["fyi_ultimoevento2"] = new EntityReference("fyi_tabelaeventos", IDdoEvento);
                service.Update(account);
            }
            else if (context.MessageName == "Delete")
            {
                account["fyi_ultimoevento2"] = null;
            }

            service.Update(account);
        }
        private static Entity RetriveAccount(IOrganizationService service, Guid IDcontaDoConvite)
        {
            return service.Retrieve("account", IDcontaDoConvite, new ColumnSet("fyi_ultimoevento2"));
        }
        private static Entity GetConviteDoEventoEntity(IPluginExecutionContext context, ITracingService tracingService)
        {
            Entity ConviteDoEvento = new Entity();

            if (context.MessageName == "Create")
            {
                ConviteDoEvento = (Entity)context.InputParameters["Target"];
            }
            else if (context.MessageName == "Delete")
            {
                ConviteDoEvento = (Entity)context.PreEntityImages["PreImage"];

            }
            return ConviteDoEvento;
        }
    }
}
