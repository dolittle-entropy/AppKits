# Event Broker based solution AppKit



## Overview
In this AppKit, you will find examples on how to set up an EDA architecture where different microservices are attached to a central message broker in order to exchange messages and publish events. 

```mermaid
flowchart TD
    A1[Publisher 1] --> B[Message Broker];
    A2[Publisher 2] <--> B[Message Broker];    
    B[Message Broker] <--> C[Microservice 1]
    B[Message Broker] --> D[Microservice 2]    
    B[Message Broker] <--> E[Microservice 3]
    B[Message Broker] --> F[Microservice n]
```
*Diagram 1*: Conceptual flowchart of the solution template

----

## Time savers
The following are the highlighted parts of the AppKit that we believe could save you some time during implementation

### IMessageBroker with Kafka implementation
The AppKit includes message broker interfaces, with implementations for Kafka, but that can easilly be implemented to a different message broker. The kafka broker uses a custom delegate `AsyncEventHandler` in order to ***await*** processing. There is also retry policy implemented under the hood as well as a factory for creating new consumers.


### Understanding how public messages are handled over the broker
At the core of this implementation is a flow that runs between the message broker and "CommandProcessors". There are three actors involved: 
| Actor | Responsability |
| ------| -------------- |
| MessageConsumer | The code that consumes public messages from the event broker |
| PublicMessageHandler | The code that inspects the public message, and finds matching processors |
| PublicMessageProcessor | The code that actually does something to the Public message, i.e. `OrderDeletedProcessor` |

Here is a rought sequence diagram: 

```mermaid
sequenceDiagram
    participant A as IMessageConsumer
    participant B as IPublicMessageHandler
    participant C as IPublicMessageProcessor
    A->>B: MessageReceived (async)
    activate B
    B->>B: 
    note right of B: Identify all IPublicMessageProcessor instances
    B->>C: Invoke Process(Message) on all processors
    activate C
    note right of C: Process the message
    C->>B: Completed (true | false)
    deactivate C
    B->>A: Task Completed
    deactivate B
```
The message processors each follow a pattern where they are set up to respond to a particular message coming over the event broker. The `IPublicMessageProcessor` interface is thus declared as follows: 

```csharp
public interface IPublicMessageProcessor
{
    /// <summary>
    /// The MessageType for which this processor was made, i.e. 'UpsertOrder'
    /// </summary>
    string ForMessageType { get; }

    ///<summary>
    /// The actual processing of the public message
    ///</summary>
    Task<bool> ProcessMessage(PublicMessage publicMessage);
}
```
Each microservice can have zero, one or several PublicMessageProcessors, depending on your needs. This is why the implementation of the IPublicMessageHandler receives a dictionary of Processors in its constructor, so that each processor can execute against a recognized message. The messages are attached to the processors via the property `ForMessageType`. There must be an exact match for this processor to engage with the message. Future variants could allow for regular expressions, or for handling more than one type of message.

----

## Versions / Dependencies
This template builds on the following components and versions

| Area                 | Technology | Library      | Version | Comments | 
| -------------------- | ---------- | ------------ | ------- | -------- |
| Framework            | .Net       | -            | 6.0     | -        | 
| Language             | C#         | -            | 10.0    | -        | 
| Message Broker       | Kafka      | Confluent    | 1.8.2   | -        |
| Frontend/API         | GraphQL    | HotChocolate | 12.7    | -        |
| Inversion of Control | IoC        | Lamar        | 7.0.0   | -        | 

## Installing
Ensure that your .Net version is up to the required version in the table above, or higher, then simply apply the command: 

```cmd
> dotnet restore
```

To retrieve the referenced packages in the solution. You should then be able to run the sample using

```cmd
> dotnet run 
```

### GraphQL
The entry point for the microservices leverage `GraphQL` in order to provide a two-way communication channel between the microservices and the front-end designer. 

### Kafka broker
We chose the Kafka broker for this template as it supports permanent retention time out of the box. 

----



