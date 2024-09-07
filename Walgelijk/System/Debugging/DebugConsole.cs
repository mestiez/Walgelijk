using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TextCopy;

namespace Walgelijk;

/// <summary>
/// Class that renders and controls the debug console
/// </summary>
public class DebugConsole : IDisposable
{
    /// <summary>
    /// The key that will toggle the console
    /// </summary>
    /// 
    public Key ToggleKey = Key.F12;
    /// <summary>
    /// Whether the console is shown and active or not
    /// </summary>
    public bool IsActive;

    /// <summary>
    /// The <see cref="Game"/> this console is associated with
    /// </summary>
    public Game Game { get; init; } = null!;

    /// <summary>
    /// Whether the console is currently eating user input
    /// </summary>
    public bool IsEatingInput => IsActive && Game.Window.HasFocus;

    /// <summary>
    /// Briefly draw the most recent console message to the screen even if the console is not open.
    /// </summary>
    public bool DrawConsoleNotification { get; set; }

    /// <summary>
    /// Scroll offset of the console
    /// </summary>
    public int ScrollOffset;

    /// <summary>
    /// Filters what kind of messages will be displayed
    /// </summary>
    public ConsoleMessageType Filter = ConsoleMessageType.All;

    private InputState Input => Game.State.Input;

    private readonly MemoryStream stream = new();
    private readonly StreamWriter writer;
    private readonly ConsoleSessionHistory sessionHistory;

    /// <summary>
    /// User interface controller
    /// </summary>
    public DebugConsoleUi UI { get; }

    private readonly List<string> history = new();
    private int historyIndex = 0;
    private string? historyInputBackup;

    private readonly string[] suggestionBuffer = new string[8];
    private int suggestionIndex;
    private bool disposed = false;

    public string DebugPrefix = "[DBG]";
    public string InfoPrefix = "[INF]";
    public string WarningPrefix = "[WRN]";
    public string ErrorPrefix = "[ERR]";

    internal const int MaxShownBuffer = 8192;
    internal ReadOnlySpan<byte> GetBuffer()
        => stream.GetBuffer().AsSpan(Math.Max(0, (int)stream.Length - MaxShownBuffer), Math.Min(MaxShownBuffer, (int)stream.Length));

    /// <summary>
    /// Input cursor index
    /// </summary>
    public int CursorPosition;

    /// <summary>
    /// Current user input
    /// </summary>
    public string? CurrentInput;

    public DebugConsole(Game game)
    {
        Game = game;
        writer = new(stream, Encoding.ASCII);
        writer.AutoFlush = true;
        UI = new DebugConsoleUi(this);
        sessionHistory = new();

        // Push the session history into our current history.
        if (sessionHistory.LastSessionCommands != null)
        {
            foreach (var input in sessionHistory.LastSessionCommands)
            {
                if (string.IsNullOrEmpty(input))
                    continue;

                history.Add(input);
            }

            historyIndex = sessionHistory.LastSessionCommands.Length;
        }
    }

    public void Update()
    {
        if (Input.IsKeyReleased(ToggleKey))
            IsActive = !IsActive;

        if (IsActive)
            Game.Window.IsCursorLocked = false;

        if (IsEatingInput)
        {
            UI.CaretBlinkTime += Game.State.Time.DeltaTimeUnscaled;

            int scrollSpeed = Input.IsKeyHeld(Key.LeftControl) ? 16 : 1;

            // TODO fix scrolling with filters enabled
            // the issue with this is that we'd have to recalculate the amount of lines that match the filter, which is
            // a potentially expensive operation

            if (Input.MouseScrollDelta > float.Epsilon)
                ScrollOffset -= scrollSpeed;
            else if (Input.MouseScrollDelta < -float.Epsilon)
                ScrollOffset += scrollSpeed;
            ScrollOffset = Utilities.Clamp(ScrollOffset, 0, UI.VisibleLineCount - UI.MaxLineCount);

            if (Input.IsKeyPressed(Key.Escape))
                IsActive = false;

            int oldCursorPos = CursorPosition;
            ProcessShortcuts();
            if (oldCursorPos != CursorPosition)
                UI.CaretBlinkTime = 0;

            CursorPosition = Utilities.Clamp(CursorPosition, 0, CurrentInput?.Length ?? 0);

            foreach (var c in Input.TextEntered)
            {
                bool isEmpty = string.IsNullOrWhiteSpace(CurrentInput);
                switch (c)
                {
                    case '\n':
                    case '\r':
                        if (!isEmpty)
                        {
                            sessionHistory.Add(CurrentInput!);
                            history.Add(CurrentInput!);
                            historyIndex = history.Count;
                            historyInputBackup = null;
                            CommandProcessor.Execute(CurrentInput!, this);
                            CurrentInput = string.Empty;
                            UI.Flash(Colors.White.WithAlpha(0.1f));
                        }
                        return;
                    case '\u007F': // delete
                        if (!isEmpty && CursorPosition < CurrentInput!.Length)
                            CurrentInput = CurrentInput![0..(CursorPosition)] + CurrentInput![(CursorPosition + 1)..];
                        break;
                    case '\b':
                        if (!isEmpty && CursorPosition > 0)
                        {
                            CurrentInput = CurrentInput![0..(CursorPosition - 1)] + CurrentInput![CursorPosition..];
                            CursorPosition--;
                        }
                        break;
                    default:
                        if (!char.IsControl(c))
                        {
                            CurrentInput ??= string.Empty;
                            CurrentInput = CurrentInput.Insert(CursorPosition, c.ToString());
                            CursorPosition++;
                            suggestionIndex = 0;
                        }
                        break;
                }
            }

            CursorPosition = Utilities.Clamp(CursorPosition, 0, CurrentInput?.Length ?? 0);
        }

        UI.Update(Game);
    }

    private void ProcessShortcuts()
    {
        if (Input.IsKeyHeld(Key.LeftControl))
        {
            if (Input.IsKeyPressed(Key.V))
            {
                var pasted = ClipboardService.GetText();
                if (pasted != null)
                {
                    pasted = pasted.ReplaceLineEndings(string.Empty);
                    CurrentInput ??= string.Empty;
                    CurrentInput = CurrentInput.Insert(CursorPosition, pasted);
                    CursorPosition += pasted.Length;
                }
            }

            if (Input.IsKeyPressed(Key.C) && CurrentInput != null)
            {
                if (!string.IsNullOrWhiteSpace(CurrentInput))
                    UI.Flash(Color.White);
                CurrentInput = null;
            }

            if (Input.IsKeyPressed(Key.L))
                Clear();

            if (Input.IsKeyPressed(Key.Left) && CursorPosition > 0 && !string.IsNullOrWhiteSpace(CurrentInput))
                CursorPosition = GetNextWordIndex(CurrentInput, CursorPosition, -1);

            if (Input.IsKeyPressed(Key.Right) && CursorPosition < CurrentInput!.Length && !string.IsNullOrWhiteSpace(CurrentInput))
                CursorPosition = GetNextWordIndex(CurrentInput, CursorPosition, 1);
            return;
        }

        if (Input.IsKeyPressed(Key.Left))
            CursorPosition--;

        if (Input.IsKeyPressed(Key.Right))
            CursorPosition++;

        if (Input.IsKeyPressed(Key.End))
            CursorPosition = CurrentInput?.Length ?? 0;

        if (Input.IsKeyPressed(Key.Home))
            CursorPosition = 0;

        if (Input.IsKeyPressed(Key.Tab) && !string.IsNullOrWhiteSpace(CurrentInput))
        {
            var suggestionCount = CommandProcessor.GetSuggestions(CurrentInput.AsSpan(0, CursorPosition), suggestionBuffer);

            if (suggestionCount > 0)
            {
                CurrentInput = suggestionBuffer[suggestionIndex % suggestionCount];
                suggestionIndex++;
            }
        }

        if (history.Count != 0)
        {
            if (Input.IsKeyPressed(Key.Up))
            {
                if (!string.IsNullOrWhiteSpace(CurrentInput) && historyIndex == history.Count)
                    historyInputBackup = CurrentInput;
                historyIndex = Utilities.Clamp(historyIndex - 1, 0, history.Count - 1);
                CurrentInput = history[historyIndex];
                CursorPosition = CurrentInput.Length;
            }

            if (Input.IsKeyPressed(Key.Down))
            {
                historyIndex = Utilities.Clamp(historyIndex + 1, 0, history.Count);
                if (historyIndex == history.Count)
                    CurrentInput = historyInputBackup;
                else
                    CurrentInput = history[historyIndex];

                CursorPosition = CurrentInput?.Length ?? 0;
            }
        }
    }

    public void Render()
    {
        UI.RenderTask.Execute(Game.Window.Graphics);
    }

    public bool PassesFilter(ConsoleMessageType t)
    {
        if (t is ConsoleMessageType.Plain)
            t = ConsoleMessageType.Info;

        return Filter.HasFlag(t);
    }

    private static int GetNextWordIndex(ReadOnlySpan<char> str, int startIndex, int direction)
    {
        direction = Math.Sign(direction);
        if (direction == 0)
            return -1;

        for (int i = startIndex + direction; (i < str.Length && i > 0); i += direction)
        {
            var cc = str[i];
            if (char.IsPunctuation(cc) || char.IsWhiteSpace(cc))
                return i;
        }

        return direction > 0 ? str.Length : 0;
    }

    /// <summary>
    /// Print text to the console
    /// </summary>
    public void WriteLine(ReadOnlySpan<char> v, ConsoleMessageType level = ConsoleMessageType.Info)
    {
        if (disposed)
            return;

        switch (level)
        {
            case ConsoleMessageType.Debug:
                writer.Write(DebugPrefix);
                writer.Write(' ');
                break;
            case ConsoleMessageType.Info:
                writer.Write(InfoPrefix);
                writer.Write(' ');
                break;
            case ConsoleMessageType.Warning:
                writer.Write(WarningPrefix);
                writer.Write(' ');
                break;
            case ConsoleMessageType.Error:
                writer.Write(ErrorPrefix);
                writer.Write(' ');
                break;
        }

        if (v.Contains('\a'))
        {
            UI.Flash(Colors.Yellow);
            Game.AudioRenderer.Play(Sound.Beep);
        }

        writer.WriteLine(v);
        UI.ParseLines();
        ScrollOffset = int.Max(UI.VisibleLineCount - UI.MaxLineCount, 0);
    }

    /// <summary>
    /// Return the <see cref="ConsoleMessageType"/> flags for the given text based on the prefix
    /// </summary>
    public ConsoleMessageType DetectMessageType(ReadOnlySpan<char> message)
    {
        var t = ConsoleMessageType.Plain;
        if (message.StartsWith(DebugPrefix))
            t = ConsoleMessageType.Debug;
        else if (message.StartsWith(InfoPrefix))
            t = ConsoleMessageType.Info;
        else if (message.StartsWith(WarningPrefix))
            t = ConsoleMessageType.Warning;
        else if (message.StartsWith(ErrorPrefix))
            t = ConsoleMessageType.Error;
        return t;
    }

    [Obsolete("use Write and WriteLine instead")]
    public void Print(ReadOnlySpan<char> text, Color color, ConsoleMessageType level = ConsoleMessageType.Plain)
        => WriteLine(text, level);

    [Obsolete("use Write and WriteLine instead")]
    public void Print(ReadOnlySpan<char> text)
        => Print(text, Color.White);

    /// <summary>
    /// Clear the console
    /// </summary>
    public void Clear()
    {
        stream.SetLength(0);
        suggestionIndex = 0;
    }

    public void Dispose()
    {
        disposed = true;
        writer.Dispose();
        stream.Dispose();
        UI.Dispose();
    }
}