using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Walgelijk;

/// <summary>
/// Controls the cursor with a last-come-first-serve priority
/// </summary>
public class CursorStack
{
    /// <summary>
    /// The cursor to consider as "overwritable"
    /// </summary>
    public DefaultCursor Fallthrough = DefaultCursor.Default;

    /// <summary>
    /// The final computer cursor appearance
    /// </summary>
    public DefaultCursor ComputedCursor { get; private set; }

    public class Requester
    {
        public int Id;
        public DefaultCursor Cursor;

        public Requester(int id, DefaultCursor cursor)
        {
            Id = id;
            Cursor = cursor;
        }
    }

    public readonly Dictionary<int, Requester> Requests = new();
    public readonly List<int> Order = new();

    public void SetCursor(DefaultCursor cursor, int optionalid = 0, [CallerLineNumber] int callId = -1)
    {
        var id = HashCode.Combine(callId, optionalid);

        if (!Order.Contains(id))
            Order.Add(id);

        if (!Requests.TryGetValue(id, out var r))
        {
            r = new Requester(id, cursor);
            r.Id = id;
            Requests.Add(id, r);
        }
        else
            r.Cursor = cursor;
    }

    public DefaultCursor ProcessRequests()
    {
        var c = Fallthrough;
        for (int i = Order.Count - 1; i >= 0; i--)
        {
            var id = Order[i];
            if (Requests.TryGetValue(id, out var requester) && requester.Cursor != Fallthrough)
                c = requester.Cursor;
        }

        ComputedCursor = c;

        foreach (var item in Requests.Values)
            item.Cursor = Fallthrough;

        return c;
    }
}
