namespace TestWorld;

public interface IOscillator
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Evaluate(double time, Grid<Cell> field);
}
