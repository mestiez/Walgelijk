using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Walgelijk.Imgui;

public class Context
{
    public readonly Dictionary<int, Identity> Identities = new();
    public readonly ControlTree ControlTree;

    public OffsetLayout CurrentOffsetLayout => ControlTree.PreviousParent?.OffsetLayout ?? UnchangedOffsetLayout;
    public WidthLayout CurrentWidthLayout => ControlTree.PreviousParent?.WidthLayout ?? UnchangedWidthLayout;
    public HeightLayout CurrentHeightLayout => ControlTree.PreviousParent?.HeightLayout ?? UnchangedHeightLayout;

    public HorizontalAnchor? CurrentHorizontalAnchor;
    public VerticalAnchor? CurrentVerticalAnchor;
    public WidthAnchor? CurrentWidthAnchor;
    public HeightAnchor? CurrentHeightAnchor;

    private static Vector2 UnchangedOffsetLayout(Identity p, Identity i, Vector2 x, Style? style = null) => x;
    private static float UnchangedWidthLayout(Identity p, Identity i, float x, Style? style = null) => x;
    private static float UnchangedHeightLayout(Identity p, Identity i, float y, Style? style = null) => y;

    private int[] identityBuffer = Array.Empty<int>();
    private int identityBufferLength = 0;

    public ReadOnlySpan<int> GetIdentityBuffer() => identityBuffer.AsSpan()[0..identityBufferLength];

    public static bool IsWindow(Identity? id) => id != null && id.Raw == Gui.WindowIdentity;

    public Identity? Hot;
    public Identity? Active;

    public Identity? PreviouslyHot;
    public Identity? PreviouslyActive;

    public int TextInputCaretPos;

    public RenderOrder Order = RenderOrder.UI;

    public int AbsoluteLayoutCounter;
    public int AbsoluteWidthCounter;
    public int AbsoluteHeightCounter;
    public Vector2 AbsoluteTranslation;

    public bool IsActive(Identity i) => Active == i;
    public bool IsHot(Identity i) => Hot == i;

    public bool IsAnythingHot() => Hot != null || PreviouslyHot != null;
    public bool IsAnythingActive() => Active != null || PreviouslyActive != null;

    /// <summary>
    /// Is anything hot, active, or hovered over? 
    /// </summary>
    public bool IsUiBeingUsed() =>
        IsAnythingActive() ||
        IsAnythingHot() ||
        ControlInputStateResolver.Raycast(Gui.Input.WindowMousePos).HasValue;

    public Scene? Scene;
    public Scene GetSceneOrCurrent() => Scene ?? Game.Main.Scene;
    public Identity? LastCreatedIdentity { get; private set; } = null;
    public float UnscaledTime;

    public int CurrentControlIndex = 0;

    public Context()
    {
        ControlTree = new(this);
    }

    public Identity StartControl(int hash)
    {
        if (Game.Main.DevelopmentMode && !GetSceneOrCurrent().HasSystem<GuiSystem>())
            Logger.Error("Your scene has no GuiSystem");

        CurrentControlIndex++;

        if (Identities.TryGetValue(hash, out var existing))
        {
            LastCreatedIdentity = existing;
            ControlTree.Push(existing);
            return existing;
        }

        var created = new Identity
        {
            Raw = hash,
        };

        created.LocalInputState.ButtonsHeld = new (bool, bool)[Gui.Input.ButtonsHeld.Length];
        created.LocalInputState.ButtonsUp   = new bool[Gui.Input.ButtonsUp.Length];
        created.LocalInputState.ButtonsDown = new bool[Gui.Input.ButtonsDown.Length];

        created.LocalInputState.KeysHeld = new (bool,bool)[Gui.Input.KeysHeld.Length];
        created.LocalInputState.KeysUp   = new bool[Gui.Input.KeysUp.Length];
        created.LocalInputState.KeysDown = new bool[Gui.Input.KeysDown.Length];

        LastCreatedIdentity = created;
        Identities.Add(hash, created);
        ControlTree.Push(created);
        return created;
    }

    public void EndControl()
    {
        ControlTree.Pop();
    }

    public void RequestScrollInput(Identity id)
    {
        id.WantsToEatScrollInput = true;
    }

    public void RebuildBuffer()
    {
        if (identityBuffer.Length < Identities.Count)
            Array.Resize(ref identityBuffer, Identities.Count);

        int i = 0;
        foreach (var item in Identities)
        {
            identityBuffer[i] = item.Key;
            i++;
        }

        identityBufferLength = i;
    }

    public void CleanUpUnusedIdentities()
    {
        ControlInputStateResolver.ClearCache();
        identityBuffer = Array.Empty<int>();

        while (true)
        {
            if (!Gui.Context.Identities.Any())
                break;
            var a = Gui.Context.Identities.FirstOrDefault(static e => !e.Value.Exists);
            if (a.Value == null)
                break;
            Gui.Context.Identities.Remove(a.Key);
            Logger.Debug(string.Format("Removed ID {0}", a.Value));
        }
    }
}
