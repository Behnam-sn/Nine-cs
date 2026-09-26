using System.Reflection;

namespace Nine.Identity.Domain.Contracts;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
