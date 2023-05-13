namespace Walgelijk.Onion.Controls;

/// <summary>
/// Indicates that the control function requires a manual call to End() after invocation
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequiresManualEndAttribute : Attribute { }
