using CP2077SaveEditor;
using WolvenKit.Core.Compression;
using WolvenKit.RED4.Save.IO;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: SaveLoadProbe <sav.dat> [sav.dat ...]");
    return 2;
}

CompressionSettings.Get().UseOodle = false;

var failures = 0;
foreach (var path in args)
{
    try
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new CyberpunkSaveReader(stream);
        var status = reader.ReadFile(out var save);
        if (status != EFileReadErrorCodes.NoError || save == null)
        {
            Console.WriteLine($"{Path.GetFileName(Path.GetDirectoryName(path))}: {status}");
            failures++;
            continue;
        }

        var requiredNodes = new[]
        {
            "CharacetrCustomization_Appearances", "inventory", "questSystem",
            "StatsSystem", "ScriptableSystemsContainer", "PersistencySystem2"
        };
        var missingNodes = requiredNodes.Where(name => !save.Nodes.Any(node => node.Name == name)).ToArray();
        Console.WriteLine($"{Path.GetFileName(Path.GetDirectoryName(path))}: version={save.FileHeader.GameVersion}, nodes={reader.NodeEntries.Count}, roots={save.Nodes.Count}, missing=[{string.Join(", ", missingNodes)}]");
        if (missingNodes.Length > 0)
        {
            failures++;
        }

        var helper = new SaveFileHelper { SaveFile = save };
        var checks = new Dictionary<string, Func<object?>>
        {
            ["appearance"] = helper.GetAppearanceContainer,
            ["inventory"] = helper.GetInventoriesContainer,
            ["stats"] = helper.GetStatsContainer,
            ["player development"] = helper.GetPlayerDevelopmentData,
            ["quest facts"] = helper.GetFactsContainer,
            ["vehicles"] = helper.GetPSDataContainer,
            ["scriptable systems"] = helper.GetScriptableContainer
        };
        foreach (var (name, check) in checks)
        {
            try
            {
                if (check() == null)
                {
                    throw new InvalidDataException($"{name} data is missing");
                }
                Console.WriteLine($"  {name}: OK");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"  {name}: {exception.Message}");
                failures++;
            }
        }
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"{Path.GetFileName(Path.GetDirectoryName(path))}: {exception}");
        failures++;
    }
}

return failures == 0 ? 0 : 1;
