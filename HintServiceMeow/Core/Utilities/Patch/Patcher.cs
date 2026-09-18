using System;
using System.Reflection;
using HarmonyLib;
using Hints;
using LabApi.Features.Wrappers;

namespace HintServiceMeow.Core.Utilities.Patch;

/// <summary>
///     Provides methods to apply and remove Harmony patches used by HintServiceMeow.
/// </summary>
public static class Patcher
{
    /// <summary>
    ///     Gets the active <see cref="HarmonyLib.Harmony" /> instance used to manage patches, or <see langword="null" /> if
    ///     patching has not been applied.
    /// </summary>
    public static Harmony? Harmony { get; private set; }

    /// <summary>
    ///     Applies all Harmony patches required by HintServiceMeow, including patches for hint display and hint sending
    ///     methods.
    /// </summary>
    public static void Patch()
    {
        Harmony = new Harmony("HintServiceMeowHarmony" + Guid.NewGuid());

        // Unpatch all other patches
        MethodInfo hintDisplayMethod = typeof(HintDisplay).GetMethod(nameof(HintDisplay.Show))!;
        MethodInfo sendHintMethod1 = typeof(Player).GetMethod(nameof(Player.SendHint), [typeof(string), typeof(float)])!;
        MethodInfo sendHintMethod2 = typeof(Player).GetMethod(nameof(Player.SendHint), [typeof(string), typeof(HintEffect[]), typeof(float)])!;
        MethodInfo? sendHintMethod3 = typeof(Player).GetMethod(nameof(Player.SendHint), [typeof(string), typeof(HintParameter[]), typeof(HintEffect[]), typeof(float)]);

        Harmony.Unpatch(hintDisplayMethod, HarmonyPatchType.All);
        Harmony.Unpatch(sendHintMethod1, HarmonyPatchType.All);
        Harmony.Unpatch(sendHintMethod2, HarmonyPatchType.All);

        if (sendHintMethod3 != null)
            Harmony.Unpatch(sendHintMethod3, HarmonyPatchType.All);

        Type patchType = typeof(Patches);

        // Patch the method
        Harmony.Patch(hintDisplayMethod, new HarmonyMethod(patchType.GetMethod(nameof(Patches.HintDisplayPatch))));
        Harmony.Patch(sendHintMethod1, new HarmonyMethod(patchType.GetMethod(nameof(Patches.SendHintPatch1))));
        Harmony.Patch(sendHintMethod2, new HarmonyMethod(patchType.GetMethod(nameof(Patches.SendHintPatch2))));

        if (sendHintMethod3 != null)
            Harmony.Patch(sendHintMethod3, new HarmonyMethod(patchType.GetMethod(nameof(Patches.SendHintPatch3))));
    }

    /// <summary>
    ///     Removes all Harmony patches applied by this patcher.
    /// </summary>
    public static void Unpatch()
    {
        Harmony?.UnpatchAll();
    }
}