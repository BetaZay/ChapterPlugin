console.log("ChapterInjector: Client script loaded and active.");

let currentItemId = null;
let chaptersFetched = false;

// Observer to detect when the video player OSD is added to the DOM
const observer = new MutationObserver((mutations) => {
    // Check if we are on the video OSD page
    const osdPage = document.querySelector('#videoOsdPage');
    const videoElement = document.querySelector('video.htmlvideoplayer');

    if (osdPage && videoElement && !chaptersFetched) {
        // We are likely in playback
        const urlParams = new URLSearchParams(window.location.hash.split('?')[1]);
        const itemId = urlParams.get('id');

        if (itemId && itemId !== currentItemId) {
            console.log("ChapterInjector: Detected Video OSD for Item " + itemId);
            currentItemId = itemId;
            fetchChapters(itemId);
        }
    } else if (!osdPage) {
        // Reset if we leave the OSD
        if (currentItemId) {
            console.log("ChapterInjector: Left Video OSD");
            currentItemId = null;
            chaptersFetched = false;
        }
    }
});

observer.observe(document.body, { childList: true, subtree: true });

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
                chaptersFetched = true;

                // TODO: Render chapters in the OSD
                injectChaptersUI(chapters);
            }
        })
        .catch(function (error) {
            console.error("ChapterInjector: Error fetching chapters: ", error);
        });
}

function injectChaptersUI(chapters) {
    // Simple verification UI injection
    const osdControls = document.querySelector('.osdControls .buttons');
    if (osdControls) {
        if (document.getElementById('chapterInjectorBtn')) return; // Already injected

        const btn = document.createElement('button');
        btn.id = 'chapterInjectorBtn';
        btn.type = 'button';
        btn.className = 'paper-icon-button-light';
        btn.title = 'Chapters';
        btn.innerHTML = '<span class="material-icons">list</span>';
        btn.onclick = function () {
            alert('Chapters: \\n' + chapters.map(c => c.Name + ' (' + c.StartPositionTicks + ')').join('\\n'));
        };

        // Insert as first button for visibility
        osdControls.insertBefore(btn, osdControls.firstChild);
        console.log("ChapterInjector: Injected chapter button into OSD.");
    } else {
        console.warn("ChapterInjector: Could not find .osdControls .buttons to inject UI.");
    }
}
