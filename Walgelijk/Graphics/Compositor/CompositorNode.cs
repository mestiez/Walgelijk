using System;

namespace Walgelijk;

public abstract class CompositorNode : IDisposable
{
    public readonly CompositorSocket[] Inputs;
    public readonly CompositorSocket Output;

    protected CompositorNode(int inputCount)
    {
        Inputs = new CompositorSocket[inputCount];
        Output = new(CompositorSocketType.Output, this);

        for (int i = 0; i < inputCount; i++)
            Inputs[i] = new CompositorSocket(CompositorSocketType.Input, this);
    }

    public abstract void Dispose();
    public abstract RenderTexture? Read(Game game, in int width, in int height);
}
