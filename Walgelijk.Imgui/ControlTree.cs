using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public class ControlTree
    {
        public static readonly Identity[] ChildBuffer = new Identity[1024];

        private readonly Context guiContext;
        private bool walkingChildren = false;

        public ControlTree(Context guiContext)
        {
            this.guiContext = guiContext;
        }

        public Identity? PreviousParent;
        public Identity? CurrentParent;

        public void Push(Identity id)
        {
            id.Exists = true;

            if (CurrentParent != null)
            {
                id.SiblingIndex = CurrentParent.ChildCount;
                CurrentParent.ChildCount++;
            }

            PreviousParent = CurrentParent;
            id.InnerContentBounds = new Rect(id.TopLeft.X, id.TopLeft.Y, id.TopLeft.X + id.Size.X, id.TopLeft.Y + id.Size.Y);
            id.Parent = CurrentParent?.Raw ?? null;
            id.PreviousChildCount = id.ChildCount;
            id.ChildCount = 0;

            if (id.Parent != null && CurrentParent != null)
                id.Order = CurrentParent.Order + 1;

            CurrentParent = id;
            id.AbsoluteTranslation = guiContext.AbsoluteTranslation;
            guiContext.AbsoluteTranslation = Vector2.Zero;

            if (guiContext.AbsoluteLayoutCounter > 0)
            {
                id.AbsoluteLayout = true;
                guiContext.AbsoluteLayoutCounter--;
            }
            else
                id.AbsoluteLayout = false;

            if (guiContext.AbsoluteWidthCounter > 0)
            {
                id.AbsoluteWidth = true;
                guiContext.AbsoluteWidthCounter--;
            }
            else
                id.AbsoluteWidth = false;

            if (guiContext.AbsoluteHeightCounter > 0)
            {
                id.AbsoluteHeight = true;
                guiContext.AbsoluteHeightCounter--;
            }
            else
                id.AbsoluteHeight = false;
        }

        public void Pop()
        {
            if (CurrentParent != null && CurrentParent.Parent.HasValue && guiContext.Identities.TryGetValue(CurrentParent.Parent.Value, out var previousParent))
            {
                CurrentParent = previousParent;

                if (previousParent != null && previousParent.Parent.HasValue && guiContext.Identities.TryGetValue(previousParent.Parent.Value, out var evenPreviouserParent))
                    PreviousParent = evenPreviouserParent;
                else
                    PreviousParent = null;
            }
            else
                CurrentParent = null;
        }

        public IEnumerable<Identity> EnumerateChildren(Identity id)
        {
            foreach (var item in guiContext.Identities)
                if (item.Value.ExistedLastFrame && item.Value.Parent.HasValue && item.Value.Parent.Value == id.Raw)
                    yield return item.Value;
        }

        public ReadOnlySpan<Identity> GetDirectChildrenOf(Identity id, Identity[]? buffer = null)//TODO als dit wordt geroepen terwijl iemand langsde child buffer gaat dan gaat ALLES KAPOT :)
        {
            if (walkingChildren)
                throw new Exception("Walking through nested child array is not allowed using this method. Call EnumerateChildren instead");
            walkingChildren = true;
            if (buffer == null)
                buffer = ChildBuffer;

            int i = 0;
            foreach (var item in guiContext.Identities)
            {
                if (item.Value.ExistedLastFrame && item.Value.Parent.HasValue && item.Value.Parent.Value == id.Raw)
                {
                    ChildBuffer[i] = item.Value;
                    i++;
                    if (i >= ChildBuffer.Length)
                        throw new Exception("Er zijn veel te veel controls jij vieze gek");
                }
            }

            walkingChildren = false;
            return ChildBuffer.AsSpan()[..i];
        }
    }
}