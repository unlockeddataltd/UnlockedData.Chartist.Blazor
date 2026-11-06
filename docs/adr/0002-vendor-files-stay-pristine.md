# Vendor files stay byte-for-byte upstream

Up to 3.x the package shipped a patched Chartist 0.11.4. It renamed the `ct:value`/`ct:meta`/`ct:series-name` attributes to `ct-*`, and the legend, tooltip, point-label and zoom plugins were appended to `chartist.js`, with their CSS appended to `chartist.css`. From 4.0 the Vendor files are shipped exactly as upstream publishes them, under `wwwroot/vendor/chartist/`. Everything we add lives in first-party files (`chartist-plugins.js`/`.css`, `blazorChartistInterop.js`) that read Chartist's native attributes. That makes a Chartist update a file swap with nothing to re-apply, and makes it visible which code is ours.

## Consequences

Consumers reference each file in a documented order instead of one `chartist.js`. The old file names are no longer shipped, so a stale tag fails with a 404 instead of loading the wrong code.
