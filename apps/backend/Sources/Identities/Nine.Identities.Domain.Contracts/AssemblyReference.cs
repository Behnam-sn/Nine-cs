using System.Reflection;

namespace Nine.Identities.Domain.Contracts;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
