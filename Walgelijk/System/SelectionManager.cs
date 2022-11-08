using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Walgelijk.Selection;

/// <summary>
/// Guarantees a selectable object
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Order in the selection stack. The lower this number, the further back it is.
    /// </summary>
    public int RaycastOrder { get; set; }

    /// <summary>
    /// Returns true if the object contains the point
    /// </summary>
    public bool ContainsPoint(Vector2 point);

    /// <summary>
    /// If this returns true, the object will be ignored by the selection system
    /// </summary>
    public bool Disabled { get; }
}

/// <summary>
/// Generic ordered selectable object manager
/// </summary>
public class SelectionManager<T> where T : class, ISelectable
{
    /// <summary>
    /// List of selectables
    /// </summary>
    public List<T> Selectables = new();
    private int[] selectionCache = Array.Empty<int>();

    /// <summary>
    /// Currently selected object
    /// </summary>
    public T? SelectedObject;

    /// <summary>
    /// The object that is currently being hovered over
    /// </summary>
    public T? HoveringObject;

    /// <summary>
    /// Update the selection manager given a mouse position and whether it is pressed
    /// </summary>
    public void UpdateState(Vector2 mousePosition, bool isMouseButtonPressed)
    {
        var allUnderMouse = GetAllIndicesAt(mousePosition);
        HoveringObject = null;
        for (int i = 0; i < Selectables.Count; i++)
        {
            var item = Selectables[i];
            if (item.Disabled)
                continue;

            if (item.ContainsPoint(mousePosition))
            {
                var indexInMouseCache = allUnderMouse.IndexOf(i);
                HoveringObject = item;
                if (isMouseButtonPressed)
                {
                    if (IsSelected(item) || (SelectedObject != null && Selectables.IndexOf(SelectedObject) > i))
                    {
                        if (allUnderMouse[^1] == i)
                        {
                            //if this is the last item in the hovering stack, select the topmost selectable
                            Select(Selectables[allUnderMouse[0]]);
                            return;
                        }
                        //this is already selected so let the selection fall through to the next selectable
                        continue;
                    }
                    else
                        Select(item);
                }

                return;
            }
        }

        if (isMouseButtonPressed)
            DeselectAll();
    }

    /// <summary>
    /// Get all the selection indices (indices in the <see cref="Selectables"/> list) that overlap the given point
    /// </summary>
    public ReadOnlySpan<int> GetAllIndicesAt(Vector2 pos)
    {
        int ii = 0;
        for (int i = 0; i < Selectables.Count; i++)
        {
            var item = Selectables[i];
            if (item.Disabled)
                continue;
            if (item.ContainsPoint(pos))
            {
                selectionCache[ii] = i;
                ii++;
            }
        }

        return selectionCache.AsSpan()[0..ii];
    }

    /// <summary>
    /// Should call every time the list changes (something added, something removed)
    /// </summary>
    public void UpdateOrder()
    {
        Selectables.Sort(static (a, b) => b.RaycastOrder - a.RaycastOrder);

        if (selectionCache.Length < Selectables.Count)
            selectionCache = new int[Selectables.Count];
    }

    /// <summary>
    /// Returns true if the given object is selected
    /// </summary>
    public bool IsSelected(T? s) => s != null && SelectedObject == s;

    /// <summary>
    /// Returns true if the mouse position (given in <see cref="UpdateState(Vector2, bool)"/>) is hovering over the given object
    /// </summary>
    public bool IsHovering(T? s) => s != null && HoveringObject == s;

    /// <summary>
    /// Pull the given object to the front
    /// </summary>
    public void PullToFront(T obj)
    {
        obj.RaycastOrder = Selectables.Max(static o => o.RaycastOrder) + 1;
        UpdateOrder();
    }

    /// <summary>
    /// Select the given object
    /// </summary>
    public void Select(T obj)
    {
        SelectedObject = obj;
        // PullToFront(obj);
    }

    /// <summary>
    /// Deselect everything
    /// </summary>
    public void DeselectAll()
    {
        SelectedObject = null;
    }
}


/// <summary>
/// Generic ordered selectable object manager
/// </summary>
public class MultiSelectionManager<T> where T : class, ISelectable
{
    /// <summary>
    /// List of selectables
    /// </summary>
    public List<T> Selectables = new();
    private int[] selectionCache = Array.Empty<int>();

    /// <summary>
    /// Currently selected object
    /// </summary>
    public List<T> SelectedObjects = new();

    /// <summary>
    /// Currently active object
    /// </summary>
    public T? ActiveObject;

    /// <summary>
    /// The object that is currently being hovered over
    /// </summary>
    public T? HoveringObject;

    /// <summary>
    /// Update the selection manager given a mouse position and whether it is pressed
    /// </summary>
    public void UpdateState(Vector2 mousePosition, bool isMouseButtonPressed, bool multiselectHeld)
    {
        var allUnderMouse = GetAllIndicesAt(mousePosition);
        HoveringObject = null;
        for (int i = 0; i < Selectables.Count; i++)
        {
            var item = Selectables[i];
            if (item.Disabled)
                continue;

            if (item.ContainsPoint(mousePosition))
            {
                var indexInMouseCache = allUnderMouse.IndexOf(i);
                HoveringObject = item;
                if (isMouseButtonPressed)
                {
                    if (IsSelected(item) || (ActiveObject != null && Selectables.IndexOf(ActiveObject) > i))
                    {
                        if (allUnderMouse[^1] == i)
                        {
                            //if this is the last item in the hovering stack, select the topmost selectable
                            Select(Selectables[allUnderMouse[0]], multiselectHeld);
                            return;
                        }
                        //this is already selected so let the selection fall through to the next selectable
                        continue;
                    }
                    else
                        Select(item, multiselectHeld);
                }

                return;
            }
        }

        if (isMouseButtonPressed)
            DeselectAll();
    }

    /// <summary>
    /// Get all the selection indices (indices in the <see cref="Selectables"/> list) that overlap the given point
    /// </summary>
    public ReadOnlySpan<int> GetAllIndicesAt(Vector2 pos)
    {
        int ii = 0;
        for (int i = 0; i < Selectables.Count; i++)
        {
            var item = Selectables[i];
            if (item.Disabled)
                continue;
            if (item.ContainsPoint(pos))
            {
                selectionCache[ii] = i;
                ii++;
            }
        }

        return selectionCache.AsSpan()[0..ii];
    }

    /// <summary>
    /// Should call every time the list changes (something added, something removed)
    /// </summary>
    public void UpdateOrder()
    {
        Selectables.Sort(static (a, b) => b.RaycastOrder - a.RaycastOrder);

        if (selectionCache.Length < Selectables.Count)
            selectionCache = new int[Selectables.Count];
    }

    /// <summary>
    /// Returns true if the given object is selected
    /// </summary>
    public bool IsSelected(T? s) => s != null && SelectedObjects.Contains(s);

    /// <summary>
    /// Returns true if the mouse position (given in <see cref="UpdateState(Vector2, bool)"/>) is hovering over the given object
    /// </summary>
    public bool IsHovering(T? s) => s != null && HoveringObject == s;

    /// <summary>
    /// Pull the given object to the front
    /// </summary>
    public void PullToFront(T obj)
    {
        if (!Selectables.Any())
            return;
        obj.RaycastOrder = Selectables.Max(static o => o.RaycastOrder) + 1;
        UpdateOrder();
    }

    /// <summary>
    /// Select the given object
    /// </summary>
    public void Select(T obj, bool additive)
    {
        ActiveObject = obj;
        if (!additive)
            SelectedObjects.Clear();

        SelectedObjects.Add(obj);
    }

    /// <summary>
    /// Deselect everything
    /// </summary>
    public void DeselectAll()
    {
        ActiveObject = null;
        SelectedObjects.Clear();
    }
}
