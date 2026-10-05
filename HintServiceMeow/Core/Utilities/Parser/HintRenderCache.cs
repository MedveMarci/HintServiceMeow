using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models.Parser.ValueObject;

namespace HintServiceMeow.Core.Utilities.Parser;

internal class HintRenderCache
{
    private Dictionary<Hint, Entry> current = new();
    private Dictionary<Hint, Entry> previous = new();
    private Dictionary<(string Text, float FontSize, float LineHeight), (float Width, float Height)> currentSizes = new();
    private Dictionary<(string Text, float FontSize, float LineHeight), (float Width, float Height)> previousSizes = new();
    private Dictionary<DynamicHint, Hint> currentConverted = new();
    private Dictionary<DynamicHint, Hint> previousConverted = new();

    public void BeginUpdate()
    {
        (previous, current) = (current, previous);
        current.Clear();
        (previousSizes, currentSizes) = (currentSizes, previousSizes);
        currentSizes.Clear();
        (previousConverted, currentConverted) = (currentConverted, previousConverted);
        currentConverted.Clear();
    }

    public Hint GetConvertedHint(DynamicHint dynamicHint)
    {
        if (currentConverted.ContainsKey(dynamicHint))
            return new Hint();

        if (!previousConverted.TryGetValue(dynamicHint, out Hint hint))
            hint = new Hint();

        currentConverted[dynamicHint] = hint;
        return hint;
    }

    public bool TryGetSize(string text, float fontSize, float lineHeight, out (float Width, float Height) size)
    {
        if (currentSizes.TryGetValue((text, fontSize, lineHeight), out size))
            return true;

        if (!previousSizes.TryGetValue((text, fontSize, lineHeight), out size))
            return false;

        currentSizes[(text, fontSize, lineHeight)] = size;
        return true;
    }

    public void AddSize(string text, float fontSize, float lineHeight, (float Width, float Height) size)
    {
        currentSizes[(text, fontSize, lineHeight)] = size;
    }

    public Entry Get(Hint hint, float xyRatio)
    {
        if (current.TryGetValue(hint, out Entry entry))
            return entry;

        if (!previous.TryGetValue(hint, out entry) || !entry.Matches(hint, xyRatio))
            entry = new Entry(hint, xyRatio);

        current[hint] = entry;
        return entry;
    }

    internal sealed class Entry(Hint hint, float ratio)
    {
        private readonly string? text = hint.Content.GetText();
        private readonly float fontSize = hint.FontSize;
        private readonly float lineHeight = hint.LineHeight;
        private readonly HintAlignment alignment = hint.Alignment;
        private readonly float xCoordinate = hint.XCoordinate;
        private readonly float yCoordinate = hint.YCoordinate;
        private readonly HintVerticalAlign yCoordinateAlign = hint.YCoordinateAlign;
        private readonly ResolutionOption resolutionOption = hint.ResolutionOption;
        private readonly float edgeMargin = hint.EdgeMargin;
        private readonly ParameterCollection parameters = hint.Parameters;
        private readonly int parametersVersion = hint.Parameters.Version;

        public TextArea? Area { get; set; }

        public string? Fragment { get; set; }

        public IParameter[] FragmentParameters { get; set; } = [];

        public int FirstParameterIndex { get; set; }

        public bool Matches(Hint hint, float xyRatio)
        {
            return ratio == xyRatio && fontSize == hint.FontSize && lineHeight == hint.LineHeight && alignment == hint.Alignment && xCoordinate == hint.XCoordinate && yCoordinate == hint.YCoordinate && yCoordinateAlign == hint.YCoordinateAlign && resolutionOption == hint.ResolutionOption && edgeMargin == hint.EdgeMargin && ReferenceEquals(parameters, hint.Parameters) && parametersVersion == hint.Parameters.Version && string.Equals(text, hint.Content.GetText());
        }
    }
}