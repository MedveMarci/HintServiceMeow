using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models.Parser;
using HintServiceMeow.Core.Models.Parser.Style;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Utilities;

internal class CompatibilityAdaptor : ICompatibilityAdaptor
{
    internal static readonly HashSet<string> RegisteredAssemblies = []; // All assemblies that used compatibility adaptor
    private static readonly ICache<string, IReadOnlyList<Hint>> HintCache = new Cache<string, IReadOnlyList<Hint>>(500);

    private const float LineSpacing = 1.2f;

    private const float MaxDuration = 86400f;

    private readonly Dictionary<string, ICoroutine> removeHandles = new();
    private readonly PlayerDisplay playerDisplay; // Initialize in constructor
    private readonly IPool<RichTextParser> richTextParserPool; // Initialize in constructor
    private readonly ICoroutineRunner coroutineRunner; // Initialize in constructor

    private readonly RichTextParserSetting settingTemplate = new(TextMeshStyle.Default, [], [], ["a", "allcaps", "alpha", "b", "color", "font", "font-weight", "gradient", "i", "lowercase", "mark", "noparse", "s", "smallcaps", "style", "u", "uppercase", "link"], true); // Tags that does not affect the position of the hint are ignored

    private bool destructed; // To prevent multiple destruct calls

    internal CompatibilityAdaptor(PlayerDisplay playerDisplay, IPool<RichTextParser>? richTextParserPool = null, ICoroutineRunner? coroutineRunner = null)
    {
        this.playerDisplay = playerDisplay ?? throw new ArgumentNullException(nameof(playerDisplay));
        this.richTextParserPool = richTextParserPool ?? RichTextParserPool.Instance;
        this.coroutineRunner = coroutineRunner ?? new UnityCoroutineRunner();
    }

    void IDisposable.Dispose()
    {
        if (destructed)
            return;

        foreach (ICoroutine coroutine in removeHandles.Values) coroutine.Kill(); // Stop all running coroutines

        removeHandles.Clear(); // Clear the dictionary

        destructed = true; // Mark as destructed
    }

    public void ShowHint(CompatibilityAdaptorArg ev)
    {
        if (ev is null)
            throw new ArgumentNullException(nameof(ev));

        string assemblyName = ev.AssemblyName;
        string content = ev.Content ?? string.Empty;
        float duration = SanitizeDuration(ev.Duration);

        // Record the assembly that is using the compatibility adaptor
        RegisteredAssemblies.Add(assemblyName);

        if (Plugin.Plugin.Instance.Config.DisabledCompatAssemblies.Any(x => assemblyName.Contains(x)) // Config limitation
            || content.Length > ushort.MaxValue) // Length limitation
            return;

        // Use internal assembly name to ensure safety
        string internalAssemblyName = "CompatibilityAdaptor-" + assemblyName;

        // For negative duration or empty content, clear hint
        if (duration <= 0f || string.IsNullOrEmpty(content))
        {
            playerDisplay.InternalClearHint(internalAssemblyName);
            playerDisplay.ForceUpdate();
            return;
        }

        // Clear hint after duration + 0.1 seconds
        if (destructed) // If the adaptor has been destructed, do not proceed
            return;

        if (removeHandles.TryGetValue(internalAssemblyName, out ICoroutine oldHandle))
            oldHandle.Kill(); // Stop the previous coroutine if exists

        // Start a new coroutine to remove the hint after the duration
        removeHandles[internalAssemblyName] = coroutineRunner.CallAfter(TimeSpan.FromSeconds(duration + 0.1f), () =>
        {
            playerDisplay.InternalClearHint(internalAssemblyName);
            playerDisplay.ForceUpdate();
            removeHandles.Remove(internalAssemblyName);
        });

        DateTime expireTime = DateTime.Now.AddSeconds(Math.Min(duration, 5f)); // Wait for at most 5 second and at least the duration

        // Start new remove action, remove after the Duration
        _ = InternalShowHint(internalAssemblyName, content, ev.Parameters, expireTime);
    }

    private static float SanitizeDuration(float duration)
    {
        if (float.IsNaN(duration))
            return 0f; // Treated as "no duration", which clears the hint

        // Covers positive infinity as well. Negative values fall through and are handled by
        // the non-positive duration branch, which clears the hint.
        return duration > MaxDuration ? MaxDuration : duration;
    }

    private async Task InternalShowHint(string internalAssemblyName, string content, IParameter[] parameters, DateTime expireTime)
    {
        try
        {
            bool useCache = parameters.Length == 0;

            if (useCache && HintCache.TryGet(content, out IReadOnlyList<Hint> cachedHintList))
            {
                ReplaceHint(internalAssemblyName, cachedHintList);
                return;
            }

            // Parse the content to hints
            IReadOnlyList<Hint> hintList = await ConcurrentTaskDispatcher.Instance.Enqueue(() => Task.FromResult(ParseRichTextToHints(content, parameters))).ConfigureAwait(false);

            // Add result to cache
            if (useCache)
                HintCache.Add(content, hintList);

            // Update if the content is not outdated
            if (DateTime.Now < expireTime) ReplaceHint(internalAssemblyName, hintList);
        }
        catch (Exception ex)
        {
            // Make sure to clear hint if error occurs
            playerDisplay.InternalClearHint(internalAssemblyName);
            Logger.Instance.Error($"Error while generating hint for {internalAssemblyName}: {ex}");
        }
    }

    private void ReplaceHint(string assemblyName, IReadOnlyList<Hint> hints)
    {
        playerDisplay.InternalClearHint(assemblyName);
        foreach (Hint hint in hints)
            playerDisplay.InternalAddHint(assemblyName, hint);
        playerDisplay.ForceUpdate(); // Since all the CompatibilityAdaptor hint is not synced, we need to force update
    }

    private IReadOnlyList<Hint> ParseRichTextToHints(string content, IParameter[] parameters)
    {
        RichTextParserSetting setting = settingTemplate;
        if (parameters.Length > 0)
        {
            Tuple<string, IParameter>[] parameterTuples = new Tuple<string, IParameter>[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                parameterTuples[i] = Tuple.Create(i.ToString(), parameters[i]);

            setting = new RichTextParserSetting(settingTemplate.DefaultStyle, parameterTuples, settingTemplate.IllegalTags, settingTemplate.IgnoreTags, settingTemplate.CloseUnclosedTags);
        }

        RichTextParser parser = richTextParserPool.Rent();
        RichTextParserResult parserResult = parser.ParseText(content, setting);
        LineInfo[] lineInfoList = parserResult.LineInfos;

        List<LineInfo> lines = WrapLinesToWidth(lineInfoList, parser);

        richTextParserPool.Return(parser);

        if (lines.Count == 0) return new List<Hint>();
        
        IParameter[] resolvedParameters = parserResult.Parameters;

        // Add the natural line height (leading) between lines so multi-line content does not
        // run together; see the LineSpacing constant for details.
        float totalHeight = lines.Sum(x => x.Height * LineSpacing);
        float accumulatedHeight = 0f;
        List<Hint> result = new(lines.Count);

        foreach (LineInfo lineInfo in lines)
        {
            // If not empty line, then add hint
            if (!string.IsNullOrEmpty(lineInfo.CleanText.Trim()) && !lineInfo.CharacterInfos.IsEmpty())
            {
                Hint hint = new()
                {
                    Text = lineInfo.CleanText,
                    YCoordinate = 700 - totalHeight / 2 + lineInfo.Height + accumulatedHeight,
                    YCoordinateAlign = HintVerticalAlign.Bottom,
                    Alignment = lineInfo.Style.Alignment,
                    FontSize = (int)lineInfo.CharacterInfos.First().Style.FontSize,
                    ResolutionOption = ResolutionOption.None,
                    SyncSpeed = HintSyncSpeed.UnSync // To make sure that when the compatibility adaptor is clearing the previous hint, the player display will not be updated
                };

                for (int i = 0; i < resolvedParameters.Length; i++)
                    hint.Parameters.Add(i.ToString(), resolvedParameters[i]);

                result.Add(hint);
            }

            accumulatedHeight += lineInfo.Height * LineSpacing;
        }

        return result.AsReadOnly();
    }

    private List<LineInfo> WrapLinesToWidth(LineInfo[] lineInfoList, RichTextParser parser)
    {
        float maxWidth = settingTemplate.DefaultStyle.LineStyle.MaxWidth;
        List<LineInfo> result = new(lineInfoList.Length);

        foreach (LineInfo lineInfo in lineInfoList)
        {
            if (lineInfo.CharacterInfos.Length == 0 || lineInfo.Width <= maxWidth)
            {
                result.Add(lineInfo);
                continue;
            }

            WrapSingleLine(lineInfo.CleanText, maxWidth, parser, result);
        }

        return result;
    }

    private void WrapSingleLine(string cleanText, float maxWidth, RichTextParser parser, List<LineInfo> output)
    {
        string[] words = cleanText.Split(' ');
        StringBuilder current = StringBuilderPool.Instance.Rent();

        try
        {
            foreach (string word in words)
            {
                int lengthBefore = current.Length;
                if (lengthBefore > 0)
                    current.Append(' ');
                current.Append(word);

                if (lengthBefore == 0)
                    continue;

                if (MeasureLineWidth(parser, current.ToString()) > maxWidth)
                {
                    current.Length = lengthBefore;
                    AddParsedLine(parser, current.ToString(), output);
                    current.Clear();
                    current.Append(word);
                }
            }

            if (current.Length > 0)
                AddParsedLine(parser, current.ToString(), output);
        }
        finally
        {
            StringBuilderPool.Instance.Return(current);
        }
    }

    private float MeasureLineWidth(RichTextParser parser, string text)
    {
        LineInfo[] measured = parser.ParseText(text, settingTemplate).LineInfos;
        return measured.Length > 0 ? measured[0].Width : 0f;
    }

    private void AddParsedLine(RichTextParser parser, string text, List<LineInfo> output)
    {
        LineInfo[] parsed = parser.ParseText(text, settingTemplate).LineInfos;
        if (parsed.Length > 0 && parsed[0].CharacterInfos.Length > 0)
            output.Add(parsed[0]);
    }
}