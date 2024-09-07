using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Walgelijk;

/// <summary>
/// Writes session commands to disk in a circular buffer.
/// </summary>
public sealed class ConsoleSessionHistory
{
    /// <summary>
    /// Size of the buffer to write to disk.
    /// </summary>
    public readonly int BufferSize = 5;

    public string FilePath = "console_session.txt";

    /// <summary>
    /// The history from last session.
    /// </summary>
    public readonly string[]? LastSessionCommands;

    private readonly string[] buffer;
    private int bufferCursor = 0;

    public ConsoleSessionHistory()
    {
        buffer = new string[BufferSize];
        var fullPath = new StringBuilder(Directory.GetCurrentDirectory()).Append(Path.DirectorySeparatorChar).Append(FilePath).ToString();

        if (File.Exists(fullPath))
        {
            var content = File.ReadAllText(fullPath, Encoding.UTF8);
            if (!string.IsNullOrEmpty(content) && !string.IsNullOrWhiteSpace(content))
                LastSessionCommands = content.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        // Session history only gets written on a relatively clean exit.
        Game.Main.BeforeExit.AddListener(() =>
        {
            var filteredBuffer = buffer.Where(static x => !string.IsNullOrWhiteSpace(x));
            File.WriteAllLines(fullPath, filteredBuffer, Encoding.UTF8);
        });
    }

    /// <summary>
    /// Add input from the console into the buffer.
    /// </summary>
    /// <param name="input"></param>
    public void Add(in string? input)
    {
        if (string.IsNullOrEmpty(input))
            return;

        buffer[bufferCursor % BufferSize] = input;
        bufferCursor++;
    }
}