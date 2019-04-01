using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Autofac.Integration.Mvc;

namespace AgentTransferBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
         protected void Application_Start()
            {
            RegisterBotDependencies();

            GlobalConfiguration.Configure(WebApiConfig.Register);

            var store = new InMemoryDataStore();

            Conversation.UpdateContainer(
                       builder =>
                       {
                           builder.Register(c => store)
                                     .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                                     .AsSelf()
                                     .SingleInstance();

                           builder.Register(c => new CachingBotDataStore(store,
                                      CachingBotDataStoreConsistencyPolicy
                                      .ETagBasedConsistency))
                                      .As<IBotDataStore<BotData>>()
                                      .AsSelf()
                                      .InstancePerLifetimeScope();


                       });
            }

        private void RegisterBotDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<AgentModule>();

            builder.RegisterControllers(typeof(WebApiApplication).Assembly);
            builder.RegisterApiControllers(typeof(WebApiApplication).Assembly);

            builder.Update(Conversation.Container);

            //DependencyResolver.SetResolver(new AutofacDependencyResolver(Conversation.Container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);
        }
    }
}
