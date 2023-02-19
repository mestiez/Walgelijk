using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion;

public static class Onion
{
    public static LayoutState Layout = new();
    public static ControlTree Tree = new();

    /*TODO 
     * control state (hot, active, capture, scroll, etc.)
     * raycast
     * scissorbox (drawarea? GetDrawBox() of iets)
     * ClearEverything();
     * style
     * Stack<Style> en dan bouw je voor elke control een final style misschien?
     * heel veel basic functies hier (label, button. etc.)
     * Animation system (IAnimation) deel van style? nee toch??? weet ik het 
     * fix die node deletion shit ControlTree.cs:93
     * navigation (arrows)
    */
}
