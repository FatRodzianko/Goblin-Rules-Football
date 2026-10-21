// FizzySteamworks (compiled into the Mirror assembly) calls the internal NativeMethods class directly.
// It worked when Steamworks.NET lived inside Mirror's folder; this keeps it working after the move.
// Remove this file once Mirror and FizzySteamworks are deleted.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Mirror")]
