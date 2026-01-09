using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsersAndPosts.Shared;

static string MapType(Type t)
{
  // Hantera Nullable<T>
  var underlying = Nullable.GetUnderlyingType(t);
  if (underlying is not null) t = underlying;

  if (t == typeof(int)) return "int";
  if (t == typeof(string)) return "string";
  if (t == typeof(DateTime)) return "string(date-time)";

  // Om du vill utöka senare (bool, decimal, etc.) lägg till här.
  return "unknown";
}

static Dictionary<string, Dictionary<string, Dictionary<string, string>>> BuildContract(Assembly asm)
{
  var result = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);

  var dtoTypes =
      asm.GetTypes()
         .Where(t => t.IsClass && !t.IsAbstract)
         .Select(t => (Type: t, Attr: t.GetCustomAttribute<DtoContractAttribute>()))
         .Where(x => x.Attr is not null)
         .Select(x => (x.Type, Attr: x.Attr!))
         .ToList();

  foreach (var (type, attr) in dtoTypes)
  {
    var group = attr.Group; // "User", "Post" osv
    if (!result.TryGetValue(group, out var groupDict))
    {
      groupDict = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
      result[group] = groupDict;
    }

    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy(p => p.MetadataToken) // stabil ordning
                    .ToDictionary(
                        p => char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1),
                        p => MapType(p.PropertyType),
                        StringComparer.Ordinal
                    );

    groupDict[type.Name] = props;
  }

  return result;
}

static int Main(string[] args)
{
  // Default: skriv till ../UsersAndPosts/dtos.json (räknat från tool-projektet)
  var outPath = args.Length > 0
      ? args[0]
      : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../UsersAndPosts/dtos.json"));

  var asm = typeof(DtoContractAttribute).Assembly; // UsersAndPosts-assembly

  var contract = BuildContract(asm);

  var options = new JsonSerializerOptions
  {
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never
  };

  var json = JsonSerializer.Serialize(contract, options);

  Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
  File.WriteAllText(outPath, json);

  Console.WriteLine($"Wrote DTO contract to: {outPath}");
  return 0;
}

return Main(args);
