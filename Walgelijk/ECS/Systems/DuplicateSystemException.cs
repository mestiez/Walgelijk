using System;

namespace Walgelijk;

public class DuplicateSystemException : Exception
{
    public DuplicateSystemException(string? msg) : base(msg ?? "There is already a system of that type in the collection") { }
}
