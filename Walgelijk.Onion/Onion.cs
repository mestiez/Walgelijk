using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly LayoutState Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();

    /*TODO 
     * control state (hot, active, capture, scroll, etc.)
     * raycast
     * scissorbox (drawarea? GetDrawBox() of iets. )
     * ClearEverything();
     * style
     *      style moet textures meer supporten, niet alleen kleuren 
     *      misschien zelfs iets anders dan quads
     *      uber shader voor alle controls
     * Stack<Style> en dan bouw je voor elke control een final style misschien?
     * heel veel basic functies hier (label, button. etc.)
     * Animation system (IAnimation) deel van style? nee toch??? weet ik het 
     * fix die node deletion shit ControlTree.cs:93
     * navigation (arrows, tab)
     *      tab: cycle through all controls chronologically
     *      arrows: move from control to control based on position in space
    */
}
