const ENDPOINT = "http://127.0.0.1:51823/active-domain";

function reportDomain(url) {
  if (!url) return;

  let hostname;
  try {
    hostname = new URL(url).hostname;
  } catch {
    return; // ignore URLs like chrome://, about:blank
  }
  if (!hostname) return;

  fetch(ENDPOINT, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ domain: hostname, url, timestamp: Date.now() }),
  }).catch(() => {
    // swallow silently if ScreenVigil isn't running
  });
}

function reportActiveTabInWindow(windowId) {
  chrome.tabs.query({ active: true, windowId }, (tabs) => {
    if (tabs.length > 0) reportDomain(tabs[0].url);
  });
}

// Tab switched within the same window
chrome.tabs.onActivated.addListener(({ tabId }) => {
  chrome.tabs.get(tabId, (tab) => reportDomain(tab.url));
});

// Navigation (new URL loaded) in the active tab
chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
  if (changeInfo.status === "complete" && tab.active) {
    reportDomain(tab.url);
  }
});

// Focus moved to a different browser window
chrome.windows.onFocusChanged.addListener((windowId) => {
  if (windowId === chrome.windows.WINDOW_ID_NONE) return;
  reportActiveTabInWindow(windowId);
});
