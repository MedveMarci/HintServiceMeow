using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using HintServiceMeow.ApiFeatures;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.Tools;

internal class FontTool : IFontTool
{
    private const float DefaultFontWidth = 67.81861f;

    private const float TableFontSize = 63.05188f;

    private static readonly float[] ChWidth = LoadWidthTable();

    public static IFontTool Instance { get; } = new FontTool();

    public float GetCharWidth(char c, float fontSize)
    {
        if (char.IsControl(c))
            return 0f;

        return ChWidth[c] * fontSize / TableFontSize;
    }

    private static float[] LoadWidthTable()
    {
        float[] table = new float[char.MaxValue + 1];

        for (int i = 0; i < table.Length; i++)
            table[i] = DefaultFontWidth;

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

                table[key] = value;
            }
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString());
        }

        return table;
    }
}