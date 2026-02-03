console.log("ChapterInjector: Client script loaded and active.");

// Hook into Jellyfin events
document.addEventListener('viewshow', function (e) {
    var itemId = e.detail.itemId;
    var itemType = e.detail.itemType;

    // Only interest in Video types (Movie, Episode)
    if (itemType === 'Movie' || itemType === 'Episode') {
        console.log("ChapterInjector: Detected viewshow for " + itemType + " " + itemId);
        fetchChapters(itemId);
    }
});

function fetchChapters(itemId) {
    var url = '/ExternalChapters/' + itemId;
    console.log("ChapterInjector: Fetching chapters from " + url);

    fetch(url)
        .then(function (response) {
            if (!response.ok) {
                if (response.status === 404) {
                    console.log("ChapterInjector: No external chapters found for " + itemId);
                    return null;
                }
                throw new Error("HTTP error " + response.status);
            }
            return response.json();
        })
        .then(function (chapters) {
            if (chapters && chapters.length > 0) {
                console.log("ChapterInjector: Received " + chapters.length + " chapters.");
                console.table(chapters);
                // TODO: Inject into player when playback starts
                // For now, we just verifying data retrieval.
            }
        })
        .catch(function (error) {
            console.error("ChapterInjector: Error fetching chapters: ", error);
        });
}
