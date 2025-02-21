﻿using Raven.Client;
using Raven.Client.Documents.Session;
using System;
using System.Globalization;

namespace Hangfire.Raven.Extensions {
    public static class IDocumentSessionExtensions {
        private static IMetadataDictionary GetMetadataForId<T>(this IDocumentSession session, string id) => session.Advanced.GetMetadataFor(session.Load<T>(id));

        private static IMetadataDictionary GetMetadataForObject<T>(this IDocumentSession session, T obj) => session.Advanced.GetMetadataFor(obj);

        public static void SetExpiry<T>(this IDocumentSession session, string id, TimeSpan expireIn) => SetExpiry(session.GetMetadataForId<T>(id), expireIn);

        public static void SetExpiry<T>(this IDocumentSession session, T obj, TimeSpan expireIn) => SetExpiry(session.GetMetadataForObject(obj), expireIn);

        public static void SetExpiry<T>(this IDocumentSession session, T obj, DateTime expireAt) => SetExpiry(session.GetMetadataForObject(obj), expireAt);

        private static void SetExpiry(IMetadataDictionary metadata, DateTime expireAt) => metadata[Constants.Documents.Metadata.Expires] = expireAt.ToString("O");

        private static void SetExpiry(IMetadataDictionary metadata, TimeSpan expireIn) => metadata[Constants.Documents.Metadata.Expires] = (DateTime.UtcNow + expireIn).ToString("O");

        public static void RemoveExpiry<T>(this IDocumentSession session, string id) => RemoveExpiry(session.GetMetadataForId<T>(id));

        public static void RemoveExpiry<T>(this IDocumentSession session, T obj) => RemoveExpiry(session.GetMetadataForObject(obj));

        public static void RemoveExpiry(IMetadataDictionary metadata) => metadata.Remove(Constants.Documents.Metadata.Expires);

        public static DateTime? GetExpiry<T>(this IDocumentSession session, string id) => GetExpiry(session.GetMetadataForId<T>(id));

        public static DateTime? GetExpiry<T>(this IDocumentSession session, T obj) => GetExpiry(session.GetMetadataForObject(obj));

        private static DateTime? GetExpiry(IMetadataDictionary metadata) => metadata.TryGetValue(Constants.Documents.Metadata.Expires, out var value)
            ? (DateTime?)DateTime.Parse(value, CultureInfo.InvariantCulture)
            : null;
    }
}
