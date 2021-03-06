﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NEventLite.Core;
using NEventLite.Core.Domain;
using NEventLite.Storage;

namespace NEventLite.StorageProviders.InMemory
{
    public class InMemorySnapshotStorageProvider<TSnapshot> : 
        InMemorySnapshotStorageProvider<TSnapshot, Guid, Guid>, ISnapshotStorageProvider<TSnapshot>
        where TSnapshot : ISnapshot<Guid, Guid>
    {
        public InMemorySnapshotStorageProvider(int frequency, string memoryDumpFile) : base(frequency, memoryDumpFile)
        {
        }
    }

    public class InMemorySnapshotStorageProvider<TSnapshot, TAggregateKey, TSnapshotKey> : ISnapshotStorageProvider<TSnapshot, TAggregateKey, TSnapshotKey>
        where TSnapshot : ISnapshot<TAggregateKey, TSnapshotKey>
    {
        private readonly Dictionary<TAggregateKey, TSnapshot> _items = new Dictionary<TAggregateKey, TSnapshot>();

        private readonly string _memoryDumpFile;
        public int SnapshotFrequency { get; }

        public InMemorySnapshotStorageProvider(int frequency) : this(frequency, string.Empty)
        {
        }

        public InMemorySnapshotStorageProvider(int frequency, string memoryDumpFile)
        {
            SnapshotFrequency = frequency;
            _memoryDumpFile = memoryDumpFile;

            if (!string.IsNullOrWhiteSpace(_memoryDumpFile) && File.Exists(_memoryDumpFile))
            {
                _items = SerializerHelper.LoadListFromFile<Dictionary<TAggregateKey, TSnapshot>>(_memoryDumpFile).First();
            }
        }

        public Task<TSnapshot> GetSnapshotAsync(TAggregateKey aggregateId)
        {
            if (_items.ContainsKey(aggregateId))
            {
                return Task.FromResult(_items[aggregateId]);
            }

            return Task.FromResult(default(TSnapshot));
        }

        public Task SaveSnapshotAsync(TSnapshot snapshot)
        {
            if (_items.ContainsKey(snapshot.AggregateId))
            {
                _items[snapshot.AggregateId] = snapshot;
            }
            else
            {
                _items.Add(snapshot.AggregateId, snapshot);
            }

            if (!string.IsNullOrWhiteSpace(_memoryDumpFile))
            {
                SerializerHelper.SaveListToFile(_memoryDumpFile, new[] { _items });
            }

            return Task.CompletedTask;
        }
    }
}
