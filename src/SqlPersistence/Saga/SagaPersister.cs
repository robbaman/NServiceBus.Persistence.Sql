﻿using System;
using System.Data.Common;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Sagas;

partial class SagaPersister : ISagaPersister
{
    SagaInfoCache sagaInfoCache;

    public SagaPersister(SagaInfoCache sagaInfoCache)
    {
        this.sagaInfoCache = sagaInfoCache;
    }

    static void AddTransitionalParameter(IContainSagaData sagaData, RuntimeSagaInfo sagaInfo, DbCommand command)
    {
        if (!sagaInfo.HasTransitionalCorrelationProperty)
        {
            return;
        }
        var transitionalId = sagaInfo.TransitionalAccessor(sagaData);
        if (transitionalId == null)
        {
            //TODO: validate non default for value types
            throw new Exception($"Null transitionalCorrelationProperty is not allowed. SagaDataType: {sagaData.GetType().FullName}.");
        }
        command.AddParameter("TransitionalCorrelationId", transitionalId);
    }

    static int GetConcurrency(ContextBag context)
    {
        int concurrency;
        if (!context.TryGet("NServiceBus.Persistence.Sql.Concurrency", out concurrency))
        {
            throw new Exception("Cannot save saga because optimistic concurrency version is missing in the context.");
        }
        return concurrency;
    }

    internal struct Concurrency<TSagaData>
        where TSagaData : IContainSagaData
    {
        public TSagaData Data { get; }
        public int Version { get; }

        public Concurrency(TSagaData data, int version)
        {
            Data = data;
            Version = version;
        }
    }
}