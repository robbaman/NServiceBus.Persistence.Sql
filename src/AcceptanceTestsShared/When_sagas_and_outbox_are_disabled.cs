﻿using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTests;
using NServiceBus.AcceptanceTests.EndpointTemplates;
using NServiceBus.Features;
using NUnit.Framework;

[TestFixture]
//The SQL persistence is still configured for both outbox and sagas
public class When_sagas_and_outbox_are_disabled : NServiceBusAcceptanceTest
{
    [Test]
    public async Task Should_create_synchronized_storage_session()
    {
        var context = await Scenario.Define<Context>()
            .WithEndpoint<EndpointWithSagasDisabled>(bb => bb.When(s => s.SendLocal(new MyMessage())))
            .Done(c => c.Done)
            .Run()
            .ConfigureAwait(false);

        Assert.True(context.Done);
        Assert.True(context.SessionCreated);
    }

    public class Context : ScenarioContext
    {
        public bool SessionCreated { get; set; }
        public bool Done { get; set; }
    }

    public class EndpointWithSagasDisabled : EndpointConfigurationBuilder
    {
        public EndpointWithSagasDisabled()
        {
            EndpointSetup<DefaultServer>(c =>
            {
                c.DisableFeature<Sagas>();
            });
        }

        public class MyMessageHandler : IHandleMessages<MyMessage>
        {
            public Context Context { get; set; }
            public Task Handle(MyMessage message, IMessageHandlerContext context)
            {
                var session = context.SynchronizedStorageSession.SqlPersistenceSession();
                Context.SessionCreated = session != null;
                Context.Done = true;

                return Task.FromResult(0);
            }
        }
    }

    public class MyMessage : IMessage
    {
    }
}