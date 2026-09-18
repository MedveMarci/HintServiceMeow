using System;
using Hints;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.UniryAdaptors;
using HintServiceMeow.Core.Utilities.Tools;
using Mirror;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors;

internal class ScpslDisplayOutput(ReferenceHub referenceHub) : IDisplayOutput
{
    private readonly NetworkConnection? connectionToPlayer = referenceHub.connectionToClient;
    private string? lastSentContent;

    public IScreenResolution ScreenResolution { get; } = new ScpslScreenResolution(referenceHub);

    public void ShowHint(DisplayOutputArg arg)
    {
        try
        {
            if (Logger.Instance.IsDebugEnabled)
                Logger.Instance.Debug($"[ScpslDisplayOutput] Trying to show hint to player {referenceHub.PlayerId} (X/Y: {ScreenResolution.XyRatio}) with content: {arg.Content}");

            if (connectionToPlayer is not { isReady: true })
                return;

            if (Logger.Instance.IsDebugEnabled)
                Logger.Instance.Debug($"[ScpslDisplayOutput] Player {referenceHub.PlayerId} is ready to receive messages. Proceeding to send hint.");

            bool hasParameters = arg.Parameters.Length > 0;


            if (!hasParameters && string.Equals(arg.Content, lastSentContent, StringComparison.Ordinal))
            {
                if (Logger.Instance.IsDebugEnabled)
                    Logger.Instance.Debug($"[ScpslDisplayOutput] Skipping unchanged hint for player {referenceHub.PlayerId}.");
                return;
            }

            HintParameter[] hintParameters;
            if (hasParameters)
            {
                hintParameters = new HintParameter[arg.Parameters.Length];
                for (int i = 0; i < arg.Parameters.Length; i++) hintParameters[i] = arg.Parameters[i].GetScpslHintParameter();
            }
            else
            {
                hintParameters = [new StringHintParameter(string.Empty)];
            }

            HintEffect[] hintEffects = new HintEffect[arg.Effects.Length];
            for (int i = 0; i < arg.Effects.Length; i++) hintEffects[i] = arg.Effects[i].GetHintEffect();

            HintMessage hintMessage = new(new TextHint(arg.Content, hintParameters, hintEffects, arg.Duration));
            connectionToPlayer.Send(hintMessage);
            
            lastSentContent = hasParameters ? null : arg.Content;

            if (Logger.Instance.IsDebugEnabled)
                Logger.Instance.Debug($"[ScpslDisplayOutput] Hint sent to player {referenceHub.PlayerId} successfully.");
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
        }
    }
}