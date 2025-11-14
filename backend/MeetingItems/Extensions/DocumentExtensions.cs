#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MeetingItemsApp.MeetingItems.DTOs;
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Extensions;

public static class DocumentExtensions
{
    /// <summary>
    /// Converts a collection of Documents to DocumentUploadResponse DTOs
    /// </summary>
    public static List<DocumentUploadResponse> ToUploadResponses(this IEnumerable<Document> documents)
    {
        return documents.Select(d => new DocumentUploadResponse(
            d.Id,
            d.FileName,
            d.OriginalFileName,
            d.FileSize,
            d.ContentType,
            d.Version)).ToList();
    }

    /// <summary>
    /// Gets all versions of a document including the base document
    /// </summary>
    public static List<Document> GetAllVersions(this IEnumerable<Document> documents, string baseDocumentId)
    {
        return documents
            .Where(d => (d.Id == baseDocumentId || d.BaseDocumentId == baseDocumentId) && !d.IsDeleted)
            .ToList();
    }

    /// <summary>
    /// Gets the next version number for a document
    /// </summary>
    public static int GetNextVersionNumber(this IEnumerable<Document> documents, string baseDocumentId)
    {
        var allVersions = documents.GetAllVersions(baseDocumentId);
        if (!allVersions.Any())
            return 1;

        return allVersions.Max(d => d.Version) + 1;
    }

    /// <summary>
    /// Finds a non-deleted document by ID
    /// </summary>
    public static Document? FindActiveDocument(this IEnumerable<Document> documents, string documentId)
    {
        return documents.FirstOrDefault(d => d.Id == documentId && !d.IsDeleted);
    }
}
