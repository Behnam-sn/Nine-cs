using System.Reflection;

namespace Nine.Profiles.Domain.Contracts;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
