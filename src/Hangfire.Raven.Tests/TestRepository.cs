using System;
using System.Collections.Generic;
using Raven.Abstractions.Data;
using Raven.Client.Embedded;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;

namespace Hangfire.Raven.Tests
{
    public class TestRepository : IRepository
    {
        private readonly EmbeddableDocumentStore _documentStore;

        public TestRepository()
        {
            // _documentStore = new EmbeddableDocumentStore
            // {
            //     RunInMemory = true,
            //     DefaultDatabase = "Hangfire-Raven-Tests",
            //     DataDirectory = @"~\Databases\Hangfire-Raven-Tests"
            // };
            //
            // _documentStore.Listeners.RegisterListener(new NoStaleQueriesListener());
            // _documentStore.Listeners.RegisterListener(new TakeNewestConflictResolutionListener());
            // _documentStore.Initialize();
            //
            // new RavenDocumentsByEntityName().Execute(_documentStore.DatabaseCommands, _documentStore.Conventions);
        }

        public void Create()
        {
        }

        public void Destroy()
        {
        }

        public void Dispose()
        {
            _documentStore.Dispose();
        }

        public IDisposable DocumentChange(Type documentType, Action<DocumentChangeNotification> action)
        {
            return null;
            // return _documentStore.Changes().ForDocumentsStartingWith(GetId(documentType, ""))
            //     .Subscribe(new RepositoryObserver<DocumentChangeNotification>(action));
        }

        public IDisposable DocumentChange(Type documentType, string suffix, Action<DocumentChangeNotification> action)
        {
            return null;
            // return _documentStore.Changes().ForDocumentsStartingWith(GetId(documentType, string.Format("{0}/", suffix)))
            //     .Subscribe(new RepositoryObserver<DocumentChangeNotification>(action));
        }

        public void ExecuteIndexes(List<AbstractIndexCreationTask> indexes)
        {
            // _documentStore.ExecuteIndexes(indexes);
        }

        public FacetResults GetFacets(string index, IndexQuery query, List<Facet> facets)
        {
            return null;
            // return _documentStore.DatabaseCommands.GetFacets(index, query, facets);
        }

        public string GetId(Type type, params string[] id)
        {
            return null;
            // return _documentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(string.Join("/", id), type, false);
        }

        public IAsyncDocumentSession OpenAsyncSession()
        {
            return null;
            // return _documentStore.OpenAsyncSession();
        }

        public IDocumentSession OpenSession()
        {
            return null;
            // return _documentStore.OpenSession();
        }
    }
}
