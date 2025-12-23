# H.Necessaire.MQ
H's Necessaire Message Queue / Service Bus / Pub Sub solution



## How to use

> The minimal, default usage, described below register the MQ with the most basic bus and storage runtime:
>   - **InMemory** storage and bus
>   - New messages notification via storage **polling**
>   - Obviously work **only In-Process**
>   - Therefore only usable for dev/play/debug/test scenarios

1. **Reference** at least the following **NuGets**:

    - `H.Necessaire.MQ.Abstractions`
    - `H.Necessaire.MQ.Core`
    - `H.Necessaire.MQ`

1. **Wireup the MQ** on the App Runtime DependencyRegistry

    ```csharp
    public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
    {
        dependencyRegistry

            .WithHmq()

            //Listener can obviously be started somewhere else as well, in any ImADependency class
            .StartHmqPeriodicPollingExternalListener()

            ;
    }
    ```
1. Use **MQ Actors** _(message **senders**)_ and **MQ ReActors** _(message **listeners**)_

    1. **MQ Actors** _(message **senders**)_
        
        - In any class that needs to send a message inject a new `ImAnHmqActor` via:
            
            ```csharp
            ImAnHmqActor myActor;
            public void ReferDependencies(ImADependencyProvider dependencyProvider)
            {
                myActor = dependencyProvider.GetHmqActor("MyUniqueActorID");
            }

            [...]

            OperationResult raiseResult = await myActor.Raise(new SomeFancyPayload().ToHmqEvent());
            ```

    1. **MQ ReActors** _(message **listeners**)_

        - In any class, register a new `ImAnHmqReActor` via:

            ```csharp
            ImAnHmqReActor reActor;
            public void ReferDependencies(ImADependencyProvider dependencyProvider)
            {
                reActor = dependencyProvider.GetHmqReActor(ReactAction, "MyUniqueReActorID", cfg => { });
                /*
                GetCatchAllHmqReActor
                GetCatchAllInternalHmqReActor
                GetCatchAllExternalHmqReActor

                GetTargetedHmqReActor
                GetTargetedInternalHmqReActor
                GetTargetedExternalHmqReActor

                GetHmqReActor
                */
            }

            async Task<OperationResult> ReactAction(HmqEvent hmqEvent)
            {
                //Process the hmqEvent
                await Task.CompletedTask;
                return OperationResult.Win();
            }

            ```

    1. Notes
        - **MQ Actors** and **MQ ReActors** are **reused**, their instances are kept in an **internal registry**.
        - That means that **MQ ReActors** _(message **listeners**)_ can safely be defined **anywhere**:
            - either in some random dependency class
            - or in a startup class
            - or in a dedicated class that handles ReActors initialization
            - or anywhere else you prefer.
        - Obviously, same goes for **MQ Actors** _(message **senders**)_
        - **ALWAYS** construct/inject actors and reactors via these dedicated methods:
            - dependencyProvider.**GetHmqActor**
            - dependencyProvider.**GetHmqReActor**
            - **NEVER** via `dependencyProvider.Get<ImAnHmqActor>()` / `dependencyProvider.Get<ImAnHmqReActor>()`



## "Real-World" usage: message buses and storage runtimes


For **actual usage** we need to use some **persistent storage** and **dedicated, external message brokers**, InMemory obviously doesn't cut it

**Storage Runtimes** keep a log of all the messages and their statuses.

 - ⚠️ **NOTE THAT** Housekeeping/archiving/cleanup/deletion is NOT handled by default and this log adds up fast

**Buses** just make sure that the messages are delivered

⚠️ **ANY storage** can be **used as a bus** as well, by default, without any extra bus-specific implementation

 - BUT very inefficiently via the default periodic polling mechanism




### How to register and use "Real-World" storage runtimes and buses

The Actor and ReActor usage doesn't change at all, so zero change in the business logic; only the wireup needs to be adjusted

1. **Reference** desired **NuGets**

    1. Available **Storage Runtimes**:

        - H.Necessaire.MQ.Runtime.**Azure.CosmosDb**
        - H.Necessaire.MQ.Runtime.**FileSystem**
        - H.Necessaire.MQ.Runtime.**Google.FirestoreDB**
        - H.Necessaire.MQ.Runtime.**RavenDb**
        - H.Necessaire.MQ.Runtime.**SqlServer**

    1. Available **Buses**:

        - H.Necessaire.MQ.Bus.**AzureServiceBus**
        - H.Necessaire.MQ.Bus.**FileSystem**
        - H.Necessaire.MQ.Bus.**NamedPipe**
        - H.Necessaire.MQ.Bus.**RabbitOrLavinMQ**
        - H.Necessaire.MQ.Bus.**RavenDB**

1. **Wireup** the **runtime** and **bus** to override the default in-memory ones
    
    ```csharp
    public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
    {
        dependencyRegistry

            .WithHmq()

            .WithHmqFileSystemRuntime()

            .WithHmqNamedPipeMessageBus()
            .StartHmqNamedPipeExternalListener()

            ;
    }
    ```

1. Obviously, some **runtimes** and **buses** need specific **runtime configs**, detailed below

    1. **Runtime storages** use the **config structure** from **H.Necessaire**, so won't detail it here
    1. **Buses** configs:


        1. H.Necessaire.MQ.Bus.**AzureServiceBus**

            - HMQ.Azure.ServiceBus.**ConnectionString**
            - HMQ.Azure.ServiceBus.**TopicName**
            - HMQ.Azure.ServiceBus.**SubscriptionName**


        1. H.Necessaire.MQ.Runtime.**FileSystem**

            - NONE all folders are created under the running folder with convention-based namings, just like H.Necessaire


        1. H.Necessaire.MQ.Bus.**NamedPipe**

            - HMQ.NamedPipe.**PipeName** - optional, defaults to `hmq`
            - HMQ.NamedPipe.**ServerName** - optional, defaults to `.`


        1. H.Necessaire.MQ.Bus.**RabbitOrLavinMQ**

            - HMQ.RabbitMQ.**URL** - if this is provided the rest are ignored
            - HMQ.RabbitMQ.**HostName**
            - HMQ.RabbitMQ.**VirtualHost**
            - HMQ.RabbitMQ.**UserName**
            - HMQ.RabbitMQ.**Password**
            ---
            - HMQ.RabbitMQ.**RoutingKey** - optional, defaults to `hmq`
            - HMQ.RabbitMQ.**Exchange** - optional, defaults to `hmq`


        1. H.Necessaire.MQ.Bus.**RavenDB** (same as H.Necessaire)

            - RavenDbConnections.**ClientCertificateName**
            - RavenDbConnections.**DatabaseUrls**
            - RavenDbConnections.**ClientCertificatePassword**
            - _**NOTE**: Client Certificate (PFX) must be an embedded resource_



## How to implement a new Storage Runtime

1. Create assembly
    1. Reference MQ Abstractions and Core
    1. Reference desired storage packs
    1. Implement and expose IoC extension for registering:
        1. `ImAStorageService<Guid, HmqEvent>`
        1. `ImAStorageBrowserService<HmqEvent, HmqEventFilter>`
        1. `ImAStorageService<Guid, HmqEventReactionLog>>`
        1. `ImAStorageBrowserService<HmqEventReactionLog, HmqEventReActionFilter>>`
    1. Register it in the App Wireup



## How to implement a new Bus

1. Create assembly
    1. Reference MQ Abstractions and Core
    1. Reference desired storage packs
    1. Implement and register:
        1. `ImAnHmqEventRiser`
        1. `ImAnHmqExternalEventListener`
            1. NOT by overriding the interface registration, but: `.Register<AzureServiceBusHmqExternalEventListener>(() => new AzureServiceBusHmqExternalEventListener())`
            1. Publish the start bus extension:
                - `public static T StartHmqAzureServiceBusExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider
            => dependencyProvider.StartHmqExternalListener("AzureServiceBus");`
        1. Publish the deps registration:
        ```csharp
        public static T WithHmqAzureServiceBusMessageBus<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<AzureServiceBusHmqDependencyGroup>(() => new AzureServiceBusHmqDependencyGroup());
            return dependencyRegistry;
        }
        ```
        1. Register it in the App Wireup


----


## QD Actions

Some buses include improved implementations for H.Necessaire QD Actions, described below.

### Wireup

```csharp
dependencyRegistry
    .WithRabbitMqQdActions()
    .StartRabbitMqQdActionsProcessor()
```
> Or equivalet _(e.g.: `.WithAzureServiceBusQdActions().StartAzureServiceBusQdActionsProcessor()`)_


### H.Necessaire.MQ.Bus.**RabbitOrLavinMQ**

 - Configs
    - QdActions.**MaxProcessingAttempts** - optional, defaults to `3`
    - QdActions.RabbitMQ.**URL** - if this is provided, the rest are ignored
    - QdActions.RabbitMQ.**HostName**
    - QdActions.RabbitMQ.**VirtualHost**
    - QdActions.RabbitMQ.**UserName**
    - QdActions.RabbitMQ.**Password**
    ---
    - QdActions.RabbitMQ.**QueueName** - optional, defaults to `h-qd-action-queue`
    - QdActions.RabbitMQ.**RoutingKey** - optional, defaults to `h-qd-action-queue`
    - QdActions.RabbitMQ.**PrefetchCount** - optional, defaults to `optimalNumberOfProcessingThreadsPerCpu(=8) * Environment.ProcessorCount`


### H.Necessaire.MQ.Bus.**AzureServiceBus**

 - Configs
    - QdActions.**MaxProcessingAttempts** - optional, defaults to `3`
    - QdActions.Azure.ServiceBus.**ConnectionString**
    - QdActions.Azure.ServiceBus.**QueueName**
    - QdActions.Azure.ServiceBus.**MaxConcurrentCalls** - optional, defaults to `optimalNumberOfProcessingThreadsPerCpu(=8) * Environment.ProcessorCount`