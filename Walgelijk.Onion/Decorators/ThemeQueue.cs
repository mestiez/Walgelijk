namespace Walgelijk.Onion.Decorators; // TODO move namespace

public class ThemeQueue
{
    /// <summary>
    /// The base theme
    /// </summary>
    public Theme Default = new();

    // TODO I am not really sure what to do here. I want to be able to override individual theme properties in a way that preferrably does not cause GC to go insane.
    // Lots of methods here for each theme property would be ideal for ease-of-use at the cost of how nice the code looks, which I think is fine.
    // I don't know how to implement this properly. A solution is to add to a list of delegates, but that would be horrible for GC, so that's absolutely ruled out.
    // -- some times has passed --
    // Wait nevermind I think I know how to do this. Every control needs to have a Theme? member where each property is nullable too wait no
    // I want to store less stuff inside a control so maybe an array of property overrides per control is better.
    // I will implement this when I get home.
}
