using System;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public struct ControlInputStateResolver
    {
        private struct ControlAtDepth { public int Identity; public int Order; }
        private static ControlAtDepth[] sortedByDepth = Array.Empty<ControlAtDepth>();

        // < 0 : x is less than y.
        // = 0 : x equals y.
        // > 0 : x is greater than y.
        private static int Compare(ControlAtDepth x, ControlAtDepth y)
        {
            return y.Order - x.Order;
        }

        public static void SetIdentities(ReadOnlySpan<int> identities)
        {
            if (identities.Length > sortedByDepth.Length)
                Array.Resize(ref sortedByDepth, identities.Length);

            for (int i = 0; i < identities.Length; i++)
            {
                var v = identities[i];
                if (Gui.Context.Identities.TryGetValue(v, out var id))
                    sortedByDepth[i] = new ControlAtDepth { Order = id.Exists ? id.Order : int.MaxValue, Identity = id.Raw };
            }

            Array.Sort(sortedByDepth, Compare);
        }

        public static int? Raycast(Vector2 mousePos)
        {
            for (int i = 0; i < sortedByDepth.Length; i++)
            {
                var item = sortedByDepth[i];
                if (!Gui.Context.Identities.TryGetValue(item.Identity, out var id))
                    continue;

                if ((!id.Exists && !id.ExistedLastFrame) || !id.RaycastRectangle.HasValue)
                    continue;

                if (id.RaycastRectangle.Value.ContainsPoint(mousePos))
                    return id.Raw;
            }

            return null;
        }

        public static int? Raycast(Vector2 mousePos, Func<Identity, bool> predicate)
        {
            for (int i = 0; i < sortedByDepth.Length; i++)
            {
                var item = sortedByDepth[i];
                if (!Gui.Context.Identities.TryGetValue(item.Identity, out var id))
                    continue;

                if ((!id.Exists && !id.ExistedLastFrame) || !id.RaycastRectangle.HasValue || !predicate(id))
                    continue;

                if (id.RaycastRectangle.Value.ContainsPoint(mousePos))
                    return id.Raw;
            }

            return null;
        }


        public static void UpdateInputStates(UiInputState state)
        {
            bool hoverEaten = false;
            bool scrollEaten = false;
            for (int i = 0; i < sortedByDepth.Length; i++)
            {
                var item = sortedByDepth[i];
                if (!Gui.Context.Identities.TryGetValue(item.Identity, out var id))
                    continue;

                var localState = id.LocalInputState;

                if (!id.Exists && !id.ExistedLastFrame)
                {
                    localState.Clear();
                    continue;
                }

                var rectContainsMouse = id.RaycastRectangle?.ContainsPoint(state.WindowMousePos) ?? false;

                localState.IsMouseOver = !hoverEaten && (rectContainsMouse);
                if (localState.IsMouseOver)
                {
                    hoverEaten = true;
                    state.ButtonsDown.CopyTo(localState.ButtonsDown, 0);
                    state.ButtonsHeld.CopyTo(localState.ButtonsHeld, 0);
                    state.ButtonsUp.CopyTo(localState.ButtonsUp, 0);

                    state.KeysDown.CopyTo(localState.KeysDown, 0);
                    state.KeysHeld.CopyTo(localState.KeysHeld, 0);
                    state.KeysUp.CopyTo(localState.KeysUp, 0);
                }
                else
                    localState.Clear();

                if (rectContainsMouse && !scrollEaten && id.WantsToEatScrollInput)
                {
                    scrollEaten = true;
                    localState.HasScrollFocus = true;
                    localState.ScrollDelta = state.ScrollDelta;
                }
                else
                {
                    localState.HasScrollFocus = false;
                    localState.ScrollDelta = 0;
                }
            }
        }

        public static void ClearCache()
        {
            sortedByDepth = Array.Empty<ControlAtDepth>();
        }
    }
}