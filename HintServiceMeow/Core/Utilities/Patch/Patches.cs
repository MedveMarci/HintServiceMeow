using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Hints;
using HintServiceMeow.ApiFeatures;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UnityAdaptors.Parameters;
using LabApi.Features.Wrappers;

namespace HintServiceMeow.Core.Utilities.Patch;

internal static class Patches
{
    private static readonly Func<TextHint, string> TextGetter = (Func<TextHint, string>)GetTextGetter();
    private static readonly Func<Hint, HintParameter[]?>? ParametersGetter = GetParametersGetter();
    private static readonly Func<Hint, HintEffect[]?>? EffectsGetter = GetEffectsGetter();

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
        "System",
        "mscorlib",
        "netstandard",
        "Mono.",
        "Microsoft."
    ];

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

    private static Func<Hint, HintParameter[]?>? GetParametersGetter()
    {
        try
        {
            ParameterExpression objParam = Expression.Parameter(typeof(Hint), "obj");
            Expression? access = null;

            PropertyInfo? prop = typeof(Hint).GetProperty("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.GetGetMethod(true) is MethodInfo getMethod)
            {
                access = Expression.Call(objParam, getMethod);
            }
            else
            {
                FieldInfo? field = typeof(Hint).GetField("_parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? typeof(Hint).GetField("parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                    access = Expression.Field(objParam, field);
            }

            if (access == null)
                return null;

            UnaryExpression body = Expression.Convert(access, typeof(HintParameter[]));
            return Expression.Lambda<Func<Hint, HintParameter[]?>>(body, objParam).Compile();
        }
        catch (Exception ex)
        {
            LogManager.Error($"Failed to build TextHint parameter getter: {ex}");
            return null;
        }
    }

    private static Func<Hint, HintEffect[]?>? GetEffectsGetter()
    {
        try
        {
            ParameterExpression objParam = Expression.Parameter(typeof(Hint), "obj");
            Expression? access = null;

            PropertyInfo? prop = typeof(Hint).GetProperty("Effects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.GetGetMethod(true) is MethodInfo getMethod)
            {
                access = Expression.Call(objParam, getMethod);
            }
            else
            {
                FieldInfo? field = typeof(Hint).GetField("_effects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? typeof(Hint).GetField("effects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                    access = Expression.Field(objParam, field);
            }

            if (access == null)
                return null;

            UnaryExpression body = Expression.Convert(access, typeof(HintEffect[]));
            return Expression.Lambda<Func<Hint, HintEffect[]?>>(body, objParam).Compile();
        }
        catch (Exception ex)
        {
            LogManager.Error($"Failed to build TextHint effect getter: {ex}");
            return null;
        }
    }

    private static void LogDroppedEffects(string assemblyName, HintEffect[]? effects)
    {
        if (effects is not { Length: > 0 })
            return;

        LogManager.Debug($"[Patches] Dropped {effects.Length} hint effect(s) from the compatibility hint of {assemblyName}; showing the plain hint instead.");
    }

    private static IParameter[]? WrapParameters(HintParameter[]? rawParameters)
    {
        if (rawParameters == null || rawParameters.Length == 0)
            return null;

        IParameter[] wrapped = new IParameter[rawParameters.Length];
        for (int i = 0; i < rawParameters.Length; i++) wrapped[i] = new ScpslHintParameterWrapper(rawParameters[i]);

        return wrapped;
    }

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
            LogManager.Error($"Failed to resolve source assembly for compatibility hint: {ex}");
        }

        return typeof(Patches).Assembly.FullName;
    }

    private static bool IsIgnoredAssembly(string simpleName)
    {
        foreach (string prefix in IgnoredAssemblyPrefixes)
            if (simpleName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

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

                LogDroppedEffects(assemblyName, EffectsGetter?.Invoke(textHint));

                PlayerDisplay.Get(referenceHub).ShowCompatibilityHint(assemblyName, content, duration, parameters);
            }
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString());
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
            LogManager.Error(ex.ToString());
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

            LogDroppedEffects(assemblyName, effects);

            __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration);
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString());
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

            LogDroppedEffects(assemblyName, effects);

            __instance.GetPlayerDisplay().ShowCompatibilityHint(assemblyName, text, duration, WrapParameters(parameters));
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString());
        }

        return false;
    }

#pragma warning restore SA1313
}