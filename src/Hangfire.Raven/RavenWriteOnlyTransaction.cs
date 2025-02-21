using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Annotations;
using Hangfire.Raven.Entities;
using Hangfire.Raven.Storage;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Hangfire.Logging;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Commands.Batches;
using Hangfire.Raven.Extensions;
using Hangfire.Raven.JobQueues;

namespace Hangfire.Raven
{
    public class RavenWriteOnlyTransaction : JobStorageTransaction
    {
        private static readonly ILog Logger = LogProvider.For<RavenWriteOnlyTransaction>();

        private readonly RavenStorage _storage;
        private readonly IDocumentSession _session;
        private readonly List<KeyValuePair<string, PatchRequest>> _patchRequests;

        private readonly Queue<Action> _afterCommitCommandQueue = new Queue<Action>();

        public RavenWriteOnlyTransaction([NotNull] RavenStorage storage)
        {
            storage.ThrowIfNull(nameof(storage));
            _storage = storage;

            _patchRequests = new List<KeyValuePair<string, PatchRequest>>();
            _session = _storage.Repository.OpenSession();
            _session.Advanced.UseOptimisticConcurrency = true;
            _session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;
        }

        public override void Commit()
        {
            var toPatch = _patchRequests.ToLookup(a => a.Key, a => a.Value);
            foreach (var item in toPatch)
            {
                foreach (var patch in item.Select(x => new PatchCommandData(item.Key, null, x, null)))
                {
                    _session.Advanced.Defer(patch);
                }
            }

            try
            {
                _session.SaveChanges();
                _session.Dispose();
            }
            catch
            {
                Logger.Error("- Concurrency exception");
                _session.Dispose();
                throw;
            }

            foreach (var command in _afterCommitCommandQueue)
            {
                command();
            }
        }

        public override void ExpireJob(string jobId, TimeSpan expireIn)
        {
            var id = _storage.Repository.GetId(typeof(RavenJob), jobId);

            _session.SetExpiry<RavenJob>(id, expireIn);
        }

        public override void PersistJob(string jobId)
        {
            var id = _storage.Repository.GetId(typeof(RavenJob), jobId);

            _session.RemoveExpiry<RavenJob>(id);
        }

        public override void SetJobState(string jobId, IState state)
        {
            var id = _storage.Repository.GetId(typeof(RavenJob), jobId);
            var result = _session.Load<RavenJob>(id);

            result.History.Insert(
                0,
                new StateHistoryDto()
                {
                    StateName = state.Name,
                    Data = state.SerializeData(),
                    Reason = state.Reason,
                    CreatedAt = DateTime.UtcNow
                });

            result.StateData = new StateData()
            {
                Name = state.Name,
                Data = state.SerializeData(),
                Reason = state.Reason
            };
        }

        public override void AddJobState(string jobId, IState state) => SetJobState(jobId, state);

        public override void AddRangeToSet(string key, IList<string> items)
        {
            key.ThrowIfNull(nameof(key));
            items.ThrowIfNull(nameof(items));

            var id = _storage.Repository.GetId(typeof(RavenSet), key);

            var set = FindOrCreateSet(id);

            foreach (var item in items)
            {
                set.Scores[item] = 0.0;
            }
        }

        public override void AddToQueue(string queue, string jobId)
        {
            var provider = _storage.QueueProviders.GetProvider(queue);
            var persistentQueue = provider.GetJobQueue();

            persistentQueue.Enqueue(queue, jobId);

            if (persistentQueue.GetType() == typeof(RavenJobQueue))
            {
                _afterCommitCommandQueue.Enqueue(() => RavenJobQueue.NewItemInQueueEvent.Set());
            }
        }

        public override void IncrementCounter(string key) => IncrementCounter(key, TimeSpan.MinValue);

        public override void IncrementCounter(string key, TimeSpan expireIn) => UpdateCounter(key, expireIn, 1);

        public override void DecrementCounter(string key) => DecrementCounter(key, TimeSpan.MinValue);

        public override void DecrementCounter(string key, TimeSpan expireIn) => UpdateCounter(key, expireIn, -1);

        public void UpdateCounter(string key, TimeSpan expireIn, int value)
        {
            var id = _storage.Repository.GetId(typeof(Counter), key);

            if (_session.Load<Counter>(id) == null)
            {
                var counter = new Counter
                {
                    Id = id,
                    Value = value
                };

                _session.Store(counter);

                if (expireIn != TimeSpan.MinValue)
                    _session.SetExpiry(counter, expireIn);
            }
            else
            {
                _patchRequests.Add(
                    new KeyValuePair<string, PatchRequest>(
                        id,
                        new PatchRequest()
                        {
                            Script = $@"this.Value += {value}"
                        }));
            }
        }

        public override void AddToSet(string key, string value) => AddToSet(key, value, 0.0);

        public override void AddToSet(string key, string value, double score)
        {
            var id = _storage.Repository.GetId(typeof(RavenSet), key);
            var set = FindOrCreateSet(id);

            set.Scores[value] = score;
        }

        public override void RemoveFromSet(string key, string value)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenSet), key);
            var set = FindOrCreateSet(id);

            set.Scores.Remove(value);

            if (set.Scores.Count == 0)
            {
                _session.Delete(set);
            }
        }

        public override void RemoveSet(string key)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenSet), key);

            _session.Delete(id);
        }

        public override void ExpireSet([NotNull] string key, TimeSpan expireIn)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenSet), key);
            var set = FindOrCreateSet(id);

            _session.SetExpiry(set, expireIn);
        }

        public override void PersistSet([NotNull] string key)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenSet), key);
            var set = FindOrCreateSet(id);

            _session.RemoveExpiry(set);
        }

        public override void InsertToList(string key, string value)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenList), key);
            var list = FindOrCreateList(id);

            list.Values.Add(value);
        }

        public override void RemoveFromList(string key, string value)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenList), key);
            var list = FindOrCreateList(id);

            list.Values.RemoveAll(v => v == value);

            if (list.Values.Count == 0)
            {
                _session.Delete(list);
            }
        }

        public override void TrimList(string key, int keepStartingFrom, int keepEndingAt)
        {
            var id = _storage.Repository.GetId(typeof(RavenList), key);
            var list = FindOrCreateList(id);

            list.Values = list.Values.Skip(keepStartingFrom).Take(keepEndingAt - keepStartingFrom + 1).ToList();

            if (list.Values.Count == 0)
            {
                _session.Delete(list);
            }
        }

        public override void ExpireList(string key, TimeSpan expireIn)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenList), key);
            var list = FindOrCreateList(id);

            _session.SetExpiry(list, expireIn);
        }

        public override void PersistList(string key)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenList), key);
            var list = FindOrCreateList(id);

            _session.RemoveExpiry(list);
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            key.ThrowIfNull(nameof(key));
            keyValuePairs.ThrowIfNull(nameof(keyValuePairs));

            var id = _storage.Repository.GetId(typeof(RavenHash), key);
            var result = FindOrCreateHash(id);

            foreach (var keyValuePair in keyValuePairs)
            {
                result.Fields[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public override void RemoveHash(string key)
        {
            key.ThrowIfNull(nameof(key));

            _session.Delete(_storage.Repository.GetId(typeof(RavenHash), key));
        }

        public override void ExpireHash(string key, TimeSpan expireIn)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenHash), key);
            var hash = FindOrCreateHash(id);

            _session.SetExpiry(hash, expireIn);
        }

        public override void PersistHash([NotNull] string key)
        {
            key.ThrowIfNull(nameof(key));

            var id = _storage.Repository.GetId(typeof(RavenHash), key);
            var hash = FindOrCreateHash(id);

            _session.RemoveExpiry(hash);
        }

        private RavenSet FindOrCreateSet(string id) => FindOrCreate(
            id,
            () => new RavenSet
            {
                Id = id
            });

        private RavenHash FindOrCreateHash(string id) => FindOrCreate(
            id,
            () => new RavenHash
            {
                Id = id
            });

        private RavenList FindOrCreateList(string id) => FindOrCreate(
            id,
            () => new RavenList
            {
                Id = id
            });

        private T FindOrCreate<T>(string id, Func<T> builder)
        {
            var obj = _session.Load<T>(id);
            if (obj != null)
            {
                return obj;
            }

            obj = builder.Invoke();
            _session.Store(obj);

            return obj;
        }
    }
}
