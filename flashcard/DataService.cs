using Flashcards;
using System.IO;
using System.Text.Json;

public static class DataService
{
    private static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "data.json");

    public static AppData Load()
    {
        if (!File.Exists(Path)) return new AppData();
        var json = File.ReadAllText(Path);
        return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
    }

    public static void Save(AppData data)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path, json);
    }
}