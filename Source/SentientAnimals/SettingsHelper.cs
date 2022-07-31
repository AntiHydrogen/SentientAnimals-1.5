using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SentientAnimals;

internal static class SettingsHelper
{
    public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f,
        float max = 100f, string tooltip = null)
    {
        float num = val;
        ls.SliderLabeled(label, ref num, format, min, max, tooltip);
        val = (int)num;
    }

    public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format,
        float min = 0f, float max = 1f, string tooltip = null)
    {
        var rect = ls.GetRect(Text.LineHeight);
        var rect2 = rect.LeftPart(0.5f).Rounded();
        var rect3 = rect.RightPart(0.3f).Rounded().LeftPart(0.67f).Rounded();
        var rect4 = rect.RightPart(0.1f).Rounded();
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect2, label);
        var num = Widgets.HorizontalSlider(rect3, val, min, max, true);
        val = num;
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(rect4, string.Format(format, val));
        if (!tooltip.NullOrEmpty())
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        Text.Anchor = anchor;
        ls.Gap(ls.verticalSpacing);
    }

    public static void FloatRange(this Listing_Standard ls, string label, ref FloatRange range, float min = 0f,
        float max = 1f, string tooltip = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
    {
        var rect = ls.GetRect(Text.LineHeight);
        var rect2 = rect.LeftPart(0.7f).Rounded();
        var rect3 = rect.RightPart(0.3f).Rounded().LeftPart(0.9f).Rounded();
        rect3.yMin -= 5f;
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect2, label);
        Text.Anchor = TextAnchor.MiddleRight;
        var hashCode = ls.CurHeight.GetHashCode();
        Widgets.FloatRange(rect3, hashCode, ref range, min, max, null, valueStyle);
        if (!tooltip.NullOrEmpty())
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        Text.Anchor = anchor;
        ls.Gap(ls.verticalSpacing);
    }

    public static Rect GetRect(this Listing_Standard listing_Standard, float? height = null)
    {
        return listing_Standard.GetRect(height ?? Text.LineHeight);
    }

    public static void AddLabeledRadioList(this Listing_Standard listing_Standard, string header, string[] labels,
        ref string val, float? headerHeight = null)
    {
        if (header != string.Empty)
        {
            Widgets.Label(listing_Standard.GetRect(headerHeight), header);
        }

        listing_Standard.AddRadioList(GenerateLabeledRadioValues(labels), ref val);
    }

    private static void AddRadioList<T>(this Listing_Standard listing_Standard, List<LabeledRadioValue<T>> items,
        ref T val, float? height = null)
    {
        foreach (var labeledRadioValue in items)
        {
            if (Widgets.RadioButtonLabeled(listing_Standard.GetRect(height), labeledRadioValue.Label,
                    EqualityComparer<T>.Default.Equals(labeledRadioValue.Value, val)))
            {
                val = labeledRadioValue.Value;
            }
        }
    }

    private static List<LabeledRadioValue<string>> GenerateLabeledRadioValues(string[] labels)
    {
        var list = new List<LabeledRadioValue<string>>();
        foreach (var text in labels)
        {
            list.Add(new LabeledRadioValue<string>(text, text));
        }

        return list;
    }

    public static void AddLabeledTextField(this Listing_Standard listing_Standard, string label,
        ref string settingsValue, float leftPartPct = 0.5f)
    {
        Rect rect;
        Rect rect2;
        listing_Standard.LineRectSpilter(out rect, out rect2, leftPartPct);
        Widgets.Label(rect, label);
        var text = settingsValue;
        settingsValue = Widgets.TextField(rect2, text);
    }

    public static void AddLabeledNumericalTextField<T>(this Listing_Standard listing_Standard, string label,
        ref T settingsValue, float leftPartPct = 0.5f, float minValue = 1f, float maxValue = 100000f) where T : struct
    {
        Rect rect;
        Rect rect2;
        listing_Standard.LineRectSpilter(out rect, out rect2, leftPartPct);
        Widgets.Label(rect, label);
        var text = settingsValue.ToString();
        Widgets.TextFieldNumeric(rect2, ref settingsValue, ref text, minValue, maxValue);
    }

    public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf,
        float leftPartPct = 0.5f, float? height = null)
    {
        var rect = listing_Standard.GetRect(height);
        leftHalf = rect.LeftPart(leftPartPct).Rounded();
        return rect;
    }

    public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, out Rect rightHalf,
        float leftPartPct = 0.5f, float? height = null)
    {
        var rect = listing_Standard.LineRectSpilter(out leftHalf, leftPartPct, height);
        rightHalf = rect.RightPart(1f - leftPartPct).Rounded();
        return rect;
    }

    public class LabeledRadioValue<T>
    {
        public LabeledRadioValue(string label, T val)
        {
            Label = label;
            Value = val;
        }

        public string Label { get; set; }

        public T Value { get; set; }
    }
}