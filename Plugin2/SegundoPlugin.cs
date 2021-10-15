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

            Entity ConviteDoEvento = GetConviteDoEventoEntity(context);//Pegando tabela a partir do contexto de execução
            try
            {
                if (ConviteDoEvento.Contains("fyi_conta")) //Se o campo tiver preenchido
                {
                    UpdateAccount(context, service, ConviteDoEvento, ConviteDoEvento.Id); //Execução do metodo de atualizar o campo ultimo evento
                }              
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());

            }
        }
        private static void UpdateAccount(IPluginExecutionContext context, IOrganizationService service, Entity ConviteDoEvento, Guid ConviteDoEventoID) //Contexto, serviço, tabela convite do evento, e o ID do convite
        {
            Guid IDcontaDoConvite = ((EntityReference)ConviteDoEvento["fyi_conta"]).Id; //Pegando Id da conta a partir do campo Conta do Formulario Convite do Evento

            Entity account = RetriveAccount(service, IDcontaDoConvite); //Pegando formulario Conta usando o serviço e o ID

            if (context.MessageName == "Create")
            {
                account["fyi_ultimoevento"] = new EntityReference("fyi_convitedoevento", ConviteDoEventoID); // Atualizando campo ultimo evento da tabela conta             
                service.Update(account); 
            }
            //else
            //{
            //    if (context.MessageName == "Delete")
            //    {
            //        account["fyi_totaldeoportunidades"] = totalDeOportunidades - 1;

            //    }
            //}
            //service.Update(account);
        }
        private static Entity RetriveAccount(IOrganizationService service, Guid IDcontaDoConvite)
        {
            return service.Retrieve("account", IDcontaDoConvite, new ColumnSet("fyi_ultimoevento"));
        }       
        private static Entity GetConviteDoEventoEntity(IPluginExecutionContext context)
        {
            Entity ConviteDoEvento = (Entity)context.InputParameters["Target"]; //Atribuindo parametro "Target" para a Entidade 

            if (context.MessageName == "Create" || context.MessageName == "Update") //Obrigátório o uso do Target para Message Update e Create
            {
                ConviteDoEvento = (Entity)context.InputParameters["Target"];
            }
            //else
            //{
            //    if (context.MessageName == "Delete")
            //    {
            //        ConviteDoEvento = (Entity)context.PreEntityImages["PreImage"];
            //    }
            //}
            return ConviteDoEvento;
        }
    }
}
