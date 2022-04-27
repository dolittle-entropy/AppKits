using Common.Commands;
using Common.Constants;
using Common.GraphQL;
using Common.Models;
using Common.PublicMessaging;
using Common.Repositories;
using Common.Validation;
using Configuration;
using Dolittle.SDK;
using Lamar;
using Sample.Products.GraphQL;
using Sample.Products.Products;
using StackExchange.Redis;

namespace Sample.Orders
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureContainer(ServiceRegistry services)
        {
            bool enableRedisGateway   = false;
            bool enableAuthentication = false;
            bool enableMessageBroker  =  true;

            services
                .ConfigureMicroservice(Configuration, enableAuthentication, enableMessageBroker)

                .ConfigureGraphQL(enableAuthentication, enableRedisGateway)
                    .AddQueryType<ProductQueries>()
                    .AddMutationType<ProductMutations>()
                    .AddSubscriptionType<ProductSubscriptions>()

                .PublishSchemaDefinition(schemaDefinition => 
                {
                     schemaDefinition
                        .SetName(BoundedContexts.Orders)
                        .IgnoreRootTypes();

                    if(enableRedisGateway)
                     {
                        schemaDefinition.AddTypeExtensionsFromFile("./stitching.graphql");
                        schemaDefinition.PublishToRedis(BoundedContexts.RedisDatabaseName, sp => sp.GetRequiredService<ConnectionMultiplexer>());
                     }
                }).InitializeOnStartup();

            services.For(typeof(IValidator<>)).Use(typeof(DefaultValidator<>)).Singleton();
            services.For(typeof(IBusinessValidator<>)).Use(typeof(DefaultBusinessValidator<>)).Singleton();
            services.For<INotificationPublisher>().Use<FrontendNotificationPublisher>();

            services.Scan(x =>
            {
                x.AssembliesAndExecutablesFromApplicationBaseDirectory();
                x.ConnectImplementationsToTypesClosing(typeof(IValidator<>), ServiceLifetime.Singleton);
                x.ConnectImplementationsToTypesClosing(typeof(ICorrelatedCommand<>), ServiceLifetime.Singleton);
                x.ConnectImplementationsToTypesClosing(typeof(ICommand<>), ServiceLifetime.Singleton);
                x.ConnectImplementationsToTypesClosing(typeof(IBusinessValidator<>), ServiceLifetime.Singleton);
                x.AddAllTypesOf(typeof(IEntity));
                x.ConnectImplementationsToTypesClosing(typeof(ICommandProcessor<>));
            });

            services.For<ProductEventHandler>().Use(container =>
            {
                return new ProductEventHandler( 
                    log: container.GetInstance<Serilog.ILogger>(),
                    notificationPublisher: container.GetInstance<INotificationPublisher>(),
                    messagePublisher: container.GetInstance<IMessagePublisher>(),
                    collectionService: container.GetInstance<ICollectionService>());
            });

            services.For<ICollectionService>().Use(Container =>
            {
                var client = Container.GetInstance<IDolittleClient>();
                var tenant = client.Tenants.First();
                var resources = client.Resources.ForTenant(tenant.Id);
                return new CollectionService(resources);
            });
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.StartMicroserviceContext(env, Configuration, enableAuthentication: false, enableMessageBrokering: false);
        }
    }
}
