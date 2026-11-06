# Use Chartist 2 through the 1.x classes and the compat build

Version 4.0 moves from Chartist 0.11.4 to Chartist 2, pinned to an exact version (2.1.0). The Interop builds charts with the deprecated 1.x classes (`new Chartist.LineChart/BarChart/PieChart`) plus `Chartist.compat` instead of the new function API (`lineChart()` …). It depends on the 1.x `created` event, and the First-party plugins are 1.x-style plugin functions, so this is a Parity port: no change to the C# API.

## Consequences

Chartist marks the 1.x classes, the 1.x events and the 1.x plugin functions "Removed in 3.0". This package must not take Chartist 3 until the Interop and the First-party plugins are rebuilt on the function API and the v2 plugin pipeline. That rebuild is also the route to the 2.x features (composed charts, live updates), which this decision defers.

## Considered Options

- **Function API in 4.0.** Rejected for now: it means a C# API redesign and a breaking change for every Consumer in the same release as the Chartist swap.
