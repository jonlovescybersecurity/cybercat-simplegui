using System.Text;
using WolvenKit.Core.Compression;
using WolvenKit.RED4.Save.IO;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: SaveRoundTripProbe <input-sav.dat> <new-output.dat>");
    return 2;
}

var inputPath = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);
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
        Console.Error.WriteLine($"Input read failed: {status}");
        return 1;
    }

    var originalNodes = reader.NodeEntries.Select(node => node.Name).ToArray();
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
    var roundTripStatus = roundTripReader.ReadFile(out var roundTripSave);
    if (roundTripStatus != EFileReadErrorCodes.NoError || roundTripSave == null)
    {
        Console.Error.WriteLine($"Round-trip read failed: {roundTripStatus}");
        return 1;
    }

    var roundTripNodes = roundTripReader.NodeEntries.Select(node => node.Name).ToArray();
    if (save.FileHeader.GameVersion != roundTripSave.FileHeader.GameVersion || !originalNodes.SequenceEqual(roundTripNodes))
    {
        Console.Error.WriteLine("Round-trip changed the save version or node names/order.");
        return 1;
    }

    Console.WriteLine($"Round-trip OK: version={save.FileHeader.GameVersion}, nodes={originalNodes.Length}, output={outputPath}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
