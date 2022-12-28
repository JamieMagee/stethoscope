namespace JamieMagee.Stethoscope.Catalogers.Windows;

public enum InstallationState
{
    Absent = 0,
    UninstallPending = 5,
    Resolving = 16,
    Resolved = 32,
    Staging = 48,
    Staged = 64,
    Superseeded = 80,
    PartiallyInstalled = 101,
    Installed = 112,
    Permanent = 128,
}
