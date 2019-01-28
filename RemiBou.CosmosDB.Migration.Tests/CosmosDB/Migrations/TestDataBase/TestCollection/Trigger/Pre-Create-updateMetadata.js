﻿function updateMetadata() {
    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();
    var createdDocument = response.getBody();

    // query for metadata document  
    var filterQuery = 'SELECT * FROM root r WHERE r.id = "_metadata"';
    var accept = collection.queryDocuments(collection.getSelfLink(), filterQuery,
        updateMetadataCallback);
    if (!accept) throw "Unable to update metadata, abort";

    function updateMetadataCallback(err, documents, responseOptions) {
        if (err) throw new Error("Error" + err.message);
        if (documents.length != 1) throw 'Unable to find metadata document';
        var metadataDocument = documents[0];

        // update metadata  
        metadataDocument.createdDocuments += 1;
        metadataDocument.createdNames += " " + createdDocument.id;
        var accept = collection.replaceDocument(metadataDocument._self,
            metadataDocument, function (err, docReplaced) {
                if (err) throw "Unable to update metadata, abort";
            });
        if (!accept) throw "Unable to update metadata, abort";
        return;
    }
}  