namespace UsersAndPosts.Shared;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DtoContractAttribute : Attribute
{
    public string Group { get; }
    public DtoContractAttribute(string group) => Group = group;
}
