using System.Text;
using CP2077SaveEditor;
using WolvenKit.Core.Compression;
using WolvenKit.RED4.Save;
using WolvenKit.RED4.Save.Classes;
using WolvenKit.RED4.Save.IO;

if (args.Length != 3 || !new[] { "facts", "inventory", "development" }.Contains(args[2]))
{
    Console.Error.WriteLine("Usage: SaveEditProbe <input-sav.dat> <new-output.dat> <facts|inventory|development>");
    return 2;
}

var inputPath = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);
var feature = args[2];
if (string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase) ||
    string.Equals(Path.GetDirectoryName(inputPath), Path.GetDirectoryName(outputPath), StringComparison.OrdinalIgnoreCase) ||
    File.Exists(outputPath))
{
    Console.Error.WriteLine("Output must be a new file outside the input save directory.");
    return 2;
}

try
{
    CompressionSettings.Get().UseOodle = false;
    using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new CyberpunkSaveReader(input);
    var status = reader.ReadFile(out var save);
    if (status != EFileReadErrorCodes.NoError || save == null)
    {
        throw new InvalidDataException($"Input read failed: {status}");
    }

    var originalNodes = reader.NodeEntries.Select(node => node.Name).ToArray();
    var helper = new SaveFileHelper { SaveFile = save };
    Func<SaveFileHelper, bool> verify;
    string description;
    switch (feature)
    {
        case "facts":
            var fact = helper.GetFactsContainer().Children
                .SelectMany(child => ((FactsTable)child.Value).FactEntries)
                .First();
            var factHash = (uint)fact.FactName;
            var factValue = fact.Value == 0 ? 1u : 0u;
            fact.Value = factValue;
            verify = reopened => reopened.GetFactsContainer().Children
                .SelectMany(child => ((FactsTable)child.Value).FactEntries)
                .Any(entry => (uint)entry.FactName == factHash && entry.Value == factValue);
            description = $"fact {factHash:X8}={factValue}";
            break;

        case "inventory":
            var item = helper.GetInventoriesContainer().SubInventories
                .SelectMany(inventory => inventory.Items)
                .First(entry => entry.IsQuantityOnly() && !entry.Flags.HasFlag(ItemFlag.IsQuestItem) && entry.Quantity < uint.MaxValue);
            var itemId = item.ItemInfo.ItemId;
            var quantity = item.Quantity + 1;
            item.Quantity = quantity;
            verify = reopened => reopened.GetInventoriesContainer().SubInventories
                .SelectMany(inventory => inventory.Items)
                .Any(entry => entry.ItemInfo.ItemId.Equals(itemId) && entry.Quantity == quantity);
            description = $"item quantity={quantity}";
            break;

        default:
            var devPoint = helper.GetPlayerDevelopmentData().DevPoints.First();
            var pointType = devPoint.Type;
            var unspent = (int)devPoint.Unspent + 1;
            devPoint.Unspent = unspent;
            verify = reopened => reopened.GetPlayerDevelopmentData().DevPoints
                .Any(entry => entry.Type.Equals(pointType) && (int)entry.Unspent == unspent);
            description = $"development point {pointType}={unspent}";
            break;
    }

    using var serialized = new MemoryStream();
    using (var writer = new CyberpunkSaveWriter(serialized, Encoding.UTF8, true))
    {
        writer.WriteFile(save, true);
    }

    serialized.Position = 0;
    using (var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
    {
        serialized.CopyTo(output);
        output.Flush(flushToDisk: true);
    }

    using var roundTripInput = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var roundTripReader = new CyberpunkSaveReader(roundTripInput);
    var roundTripStatus = roundTripReader.ReadFile(out var reopenedSave);
    if (roundTripStatus != EFileReadErrorCodes.NoError || reopenedSave == null ||
        save.FileHeader.GameVersion != reopenedSave.FileHeader.GameVersion ||
        !originalNodes.SequenceEqual(roundTripReader.NodeEntries.Select(node => node.Name)) ||
        !verify(new SaveFileHelper { SaveFile = reopenedSave }))
    {
        throw new InvalidDataException($"Edited save failed reparse or {feature} value verification: {roundTripStatus}");
    }

    Console.WriteLine($"Edited round-trip OK: {description}, nodes={originalNodes.Length}, output={outputPath}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
