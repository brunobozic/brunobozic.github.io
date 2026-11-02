using System.Runtime.CompilerServices;

// The editor generator and the edit-mode tests author definitions through internal fields.
// Runtime game code only ever sees the read-only public surface.
[assembly: InternalsVisibleTo("QuietHull.Data.Editor")]
[assembly: InternalsVisibleTo("QuietHull.Data.Tests.Editor")]
