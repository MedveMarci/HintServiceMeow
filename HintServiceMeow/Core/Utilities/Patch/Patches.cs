using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Hints;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UnityAdaptors.Parameters;
using HintServiceMeow.Core.Utilities.Tools;
using LabApi.Features.Wrappers;

namespace HintServiceMeow.Core.Utilities.Patch;

using Logger = Logger;

internal static class Patches
{
    private static readonly Func<TextHint, string> TextGetter = (Func<TextHint, string>)GetTextGetter();
    private static readonly Func<TextHint, HintParameter[]?>? ParametersGetter = GetParametersGetter();
    private static readonly Func<TextHint, HintEffect[]?>? EffectsGetter = GetEffectsGetter();

#pragma warning disable SA1313
    public static bool HintDisplayPatch(ref Hint hint, ref HintDisplay __instance)
    {
        try
        {
            if (!Plugin.Plugin.Instance.Config.UseHintCompatibilityAdapter)
                return false;

            if (hint is TextHint textHint && ReferenceHub.TryGetHubNetID(__instance.connectionToClient.identity.netId, out ReferenceHub referenceHub))
            {
                string assemblyName = ResolveSourceAssemblyName();
                string content = TextGetter(textHint);
                float duration = textHint.DurationScalar;
                IParameter[]? parameters = WrapParameters(ParametersGetter?.Invoke(textHint));

                // Hint effects are deliberately dropped here; only the plain hint is shown.
                LogDroppedEffects(assemblyName, EffectsGetter?.Invoke(textHint));

                PlayerDisplay.Get(referenceHub).ShowCompatibilityHint(assemblyName, content, duration, parameters);
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
        }

        return false;
    }

    public static bool SendHintPatch1(ref string text, ref float duration, ref Player __instance)
    {
        try
        {
            if (!Plugin.Plugin.Instance.Config.UseHintCompatibilityAdapter)
                return false;

            string assemblyName = ResolveSourceAssemblyName();
            __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration);
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
        }

        return false;
    }

    public static bool SendHintPatch2(ref string text, ref HintEffect[] effects, ref float duration, ref Player __instance)
    {
        try
        {
            if (!Plugin.Plugin.Instance.Config.UseHintCompatibilityAdapter)
                return false;

            string assemblyName = ResolveSourceAssemblyName();

            // Hint effects are deliberately dropped here; only the plain hint is shown.
            LogDroppedEffects(assemblyName, effects);

            __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration);
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
        }

        return false;
    }

    public static bool SendHintPatch3(ref string text, ref HintParameter[] parameters, ref HintEffect[] effects, ref float duration, ref Player __instance)
    {
        try
        {
            if (!Plugin.Plugin.Instance.Config.UseHintCompatibilityAdapter)
                return false;

            string assemblyName = ResolveSourceAssemblyName();

            // Hint effects are deliberately dropped here; only the plain hint is shown.
            LogDroppedEffects(assemblyName, effects);

            __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration, WrapParameters(parameters));
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
        }

        return false;
    }

#pragma warning restore SA1313

    private static Delegate GetTextGetter()
    {
        PropertyInfo? prop = typeof(TextHint).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (prop == null)
            throw new MissingMemberException(typeof(TextHint).FullName, "Text");

        MethodInfo? getMethod = prop.GetGetMethod(true);
        if (getMethod == null)
            throw new InvalidOperationException("Property 'Text' has no getter.");

        ParameterExpression objParam = Expression.Parameter(typeof(TextHint), "obj");
        MethodCallExpression call = Expression.Call(objParam, getMethod);
        UnaryExpression body = Expression.Convert(call, typeof(string));

        return Expression.Lambda<Func<TextHint, string>>(body, objParam).Compile();
    }

    private static Func<TextHint, HintParameter[]?>? GetParametersGetter()
    {
        try
        {
            ParameterExpression objParam = Expression.Parameter(typeof(TextHint), "obj");
            Expression? access = null;

            PropertyInfo? prop = typeof(TextHint).GetProperty("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.GetGetMethod(true) is MethodInfo getMethod)
            {
                access = Expression.Call(objParam, getMethod);
            }
            else
            {
                FieldInfo? field = typeof(TextHint).GetField("_parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? typeof(TextHint).GetField("parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                    access = Expression.Field(objParam, field);
            }

            if (access == null)
                return null;

            UnaryExpression body = Expression.Convert(access, typeof(HintParameter[]));
            return Expression.Lambda<Func<TextHint, HintParameter[]?>>(body, objParam).Compile();
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Failed to build TextHint parameter getter: {ex}");
            return null;
        }
    }

    private static Func<TextHint, HintEffect[]?>? GetEffectsGetter()
    {
        try
        {
            ParameterExpression objParam = Expression.Parameter(typeof(TextHint), "obj");
            Expression? access = null;

            PropertyInfo? prop = typeof(TextHint).GetProperty("Effects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.GetGetMethod(true) is MethodInfo getMethod)
            {
                access = Expression.Call(objParam, getMethod);
            }
            else
            {
                FieldInfo? field = typeof(TextHint).GetField("_effects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? typeof(TextHint).GetField("effects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                    access = Expression.Field(objParam, field);
            }

            if (access == null)
                return null;

            UnaryExpression body = Expression.Convert(access, typeof(HintEffect[]));
            return Expression.Lambda<Func<TextHint, HintEffect[]?>>(body, objParam).Compile();
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Failed to build TextHint effect getter: {ex}");
            return null;
        }
    }

    private static void LogDroppedEffects(string assemblyName, HintEffect[]? effects)
    {
        if (effects is not { Length: > 0 } || !Logger.Instance.IsDebugEnabled)
            return;

        Logger.Instance.Debug($"[Patches] Dropped {effects.Length} hint effect(s) from the compatibility hint of {assemblyName}; showing the plain hint instead.");
    }

    private static IParameter[]? WrapParameters(HintParameter[]? rawParameters)
    {
        if (rawParameters == null || rawParameters.Length == 0)
            return null;

        IParameter[] wrapped = new IParameter[rawParameters.Length];
        for (int i = 0; i < rawParameters.Length; i++) wrapped[i] = new ScpslHintParameterWrapper(rawParameters[i]);

        return wrapped;
    }

    private static readonly string[] IgnoredAssemblyPrefixes =
    [
        "HintServiceMeow",
        "0Harmony",
        "HarmonyLib",
        "LabApi",
        "Northwood",
        "Assembly-CSharp",
        "Mirror",
        "UnityEngine",
        "Unity.",
        "CommandSystem",
        "PluginAPI",
        "System",
        "mscorlib",
        "netstandard",
        "Mono.",
        "Microsoft."
    ];

    private static string ResolveSourceAssemblyName()
    {
        try
        {
            StackFrame[]? frames = new StackTrace(false).GetFrames();
            if (frames != null)
                foreach (StackFrame frame in frames)
                {
                    // Harmony's generated dynamic methods have no declaring type; skip them.
                    Assembly? assembly = frame.GetMethod()?.DeclaringType?.Assembly;
                    if (assembly == null)
                        continue;

                    string simpleName = assembly.GetName().Name ?? string.Empty;
                    if (IsIgnoredAssembly(simpleName))
                        continue;

                    return assembly.FullName ?? simpleName;
                }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Failed to resolve source assembly for compatibility hint: {ex}");
        }

        // Last resort: fall back to this assembly so the hint is still displayed. It will not
        // match any plugin keyword in DisabledCompatAssemblies, which is the safe default.
        return typeof(Patches).Assembly.FullName ?? nameof(HintServiceMeow);
    }

    private static bool IsIgnoredAssembly(string simpleName)
    {
        foreach (string prefix in IgnoredAssemblyPrefixes)
            if (simpleName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }
}