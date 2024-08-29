# Usage

1. How to implement a new BUS
    1. Create assembly
    1. Reference MQ Abstractions and Core
    1. Reference desired storage packs
    1. Implement and register:
        1. `ImAnHmqEventRiser`
        1. `ImAnHmqExternalEventListener`
            1. NOT by overriding the interface registrtion, but: `.Register<AzureServiceBusHmqExternalEventListener>(() => new AzureServiceBusHmqExternalEventListener())`
            1. Publish the start bus extension:
                - `public static T StartHmqAzureServiceBusExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider`
            => dependencyProvider.StartHmqExternalListener("AzureServiceBus");
        1. Publish the deps registration:
```
        public static T WithHmqAzureServiceBusMessageBus<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<AzureServiceBusHmqDependencyGroup>(() => new AzureServiceBusHmqDependencyGroup());
            return dependencyRegistry;
        }
```

1. How to implement a new storage runtime
    1. Create assembly
    1. Reference MQ Abstractions and Core
    1. Reference desired storage packs
    1. Implement and register:
        1. `ImAStorageService<Guid, HmqEvent>`
        1. `ImAStorageBrowserService<HmqEvent, HmqEventFilter>`
        1. `ImAStorageService<Guid, HmqEventReactionLog>>`
        1. `ImAStorageBrowserService<HmqEventReactionLog, HmqEventReActionFilter>>`
1. How to use in services / actors / managers / ...
    1. To listen for messages
        - ALWAYS Inject ReActor via `ImADependencyProvider.Get___HmqReActor`
        - This will track it in the internal `ImAnHmqActorAndReActorBookkeeper`
    1. To send messages
        - ALWAYS Inject Actor via `ImADependencyProvider.GetHmqActor`
        - This will track it in the internal `ImAnHmqActorAndReActorBookkeeper`
1. How to wireup
    1. Refer and Register desired storage runtime
    1. Refer and Register+Start desired bus


## Notes

1. InMemory Bus and Storage - by default (no other pack needed)
1. SQL Server Bus needs no explicit pack, just the storage runtime
    1. ANY storage that doesn't support server-to-client events can be used as a BUS, just like SQL Server
        - That's because the default `📨♻️PeriodicPollingHmqExternalEventListener` acts as the bus by pinging the registered runtime storage 