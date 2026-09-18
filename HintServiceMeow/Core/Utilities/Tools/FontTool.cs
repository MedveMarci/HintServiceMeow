using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.Tools;

internal class FontTool : IFontTool
{
    private const float DefaultFontWidth = 67.81861f;
    private static readonly float[] ChWidth = CreateWidthTable();

    public static IFontTool Instance { get; } = new FontTool();

    static FontTool()
    {
        ConcurrentTaskDispatcher.Instance.Enqueue(() =>
        {
            try
            {
                using Stream? infoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HintServiceMeow.TextWidth");

                if (infoStream is null)
                    throw new FileNotFoundException("Could not find text width");

                using ZipArchive archive = new(infoStream, ZipArchiveMode.Read);
                using Stream entryStream = archive.Entries.First(x => x.Name == "TextWidth").Open();
                using StreamReader reader = new(entryStream);

                while (reader.ReadLine() is { } line)
                {
                    if (line == string.Empty)
                        continue;

                    int sep = line.IndexOf(':');
                    if (sep <= 0)
                        continue;

                    char key = (char)int.Parse(line.Substring(0, sep));
                    float value = float.Parse(line.Substring(sep + 1).TrimStart(), CultureInfo.InvariantCulture);
                    
                    ChWidth[key] = value;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }

            return Task.CompletedTask;
        });
    }

    public float GetCharWidth(char c, float fontSize)
    {
        if (char.IsControl(c))
            return 0f;

        return ChWidth[c];
    }

    private static float[] CreateWidthTable()
    {
        float[] table = new float[char.MaxValue + 1];

        for (int i = 0; i < table.Length; i++)
            table[i] = DefaultFontWidth;

        return table;
    }
}