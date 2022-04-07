using System;
using System.Collections.Generic;
using System.Text;

namespace Walgelijk
{

    /// <summary>
    /// Class that renders and controls the debug console
    /// </summary>
    public class DebugConsole
    {
        /// <summary>
        /// The key that will toggle the console
        /// </summary>
        public Key ToggleKey = Key.F12;

        /// <summary>
        /// Whether the console is shown and active or not
        /// </summary>
        public bool IsActive
        {
            get => isActive;

            set
            {
                isActive = value;
                renderer.SetDirty();
            }
        }

        /// <summary>
        /// The <see cref="Game"/> this console is associated with
        /// </summary>
        public Game Game { get; }

        /// <summary>
        /// Whether the console is currently eating user input
        /// </summary>
        public bool IsEatingInput => IsActive && Game.Window.HasFocus;

        /// <summary>
        /// Briefly draw the most recent console message to the screen even if the console is not open.
        /// </summary>
        public bool DrawConsoleNotification { get; set; }

        /// <summary>
        /// Filters what kind of messages will be displayed
        /// </summary>
        public ConsoleMessageType Filter
        {
            get => filter; 
            
            set
            {
                filter = value;
                renderer.SetDirty();
            }
        }
        /// <summary>
        /// Scroll offset of the console
        /// </summary>
        public float ScrollOffset;

        private readonly DebugConsoleRenderer renderer;
        internal List<(string message, Color color, ConsoleMessageType type)> Log = new(128);
        internal List<string> InputHistory = new();
        private int historyIndex = 0;
        private string currentInput = "";
        private bool isActive;
        private ConsoleMessageType filter = ConsoleMessageType.All;

        private InputState Input => Game.Window.InputState;

        public DebugConsole(Game game)
        {
            this.Game = game;
            renderer = new DebugConsoleRenderer(this);

            Logger.OnLog.AddListener((e) =>
            {
                Color c;
                string name;
                ConsoleMessageType messageType;
                switch (e.Level)
                {
                    default:
                    case LogLevel.Debug:
                        c = Colors.Purple;
                        messageType = ConsoleMessageType.Debug;
                        name = "DBG";
                        break;
                    case LogLevel.Info:
                        c = DebugConsoleRenderer.DefaultTextColour;
                        messageType = ConsoleMessageType.Info;
                        name = "LOG";
                        break;
                    case LogLevel.Warn:
                        c = Colors.Orange;
                        messageType = ConsoleMessageType.Warning;
                        name = "WRN";
                        break;
                    case LogLevel.Error:
                        c = Colors.Red;
                        messageType = ConsoleMessageType.Error;
                        name = "ERR";
                        break;
                }
                Print($"[{name}] {e.Message}", c, messageType);
            });
        }

        public void Update()
        {
            if (Input.IsKeyReleased(ToggleKey))
            {
                IsActive = !IsActive;
                renderer.SetDirty();
            }

            UpdateConsole();
        }

        /// <summary>
        /// Print text to the console
        /// </summary>
        public void Print(ReadOnlySpan<char> text, Color color, ConsoleMessageType level = ConsoleMessageType.Info)
        {
            if (Log.Count == Log.Capacity - 1)
                Log.RemoveAt(0);
            Log.Add((new string(text), color, level));
            renderer.SetDirty();
            ScrollOffset = 0;
        }

        /// <summary>
        /// Print text to the console
        /// </summary>
        public void Print(ReadOnlySpan<char> text)
        {
            Print(text, DebugConsoleRenderer.DefaultTextColour);
        }

        /// <summary>
        /// Clear the console
        /// </summary>
        public void Clear()
        {
            Log.Clear();
            renderer.SetDirty();
        }

        private void UpdateConsole()
        {
            if (!IsEatingInput) return;

            ScrollOffset += Input.MouseScrollDelta * 25;
            ScrollOffset = Utilities.Clamp(ScrollOffset, 0, Math.Max(0, renderer.TextBounds.Height - DebugConsoleRenderer.LogHeight + 10));

            foreach (var c in Input.TextEntered)
            {
                switch (c)
                {
                    case '\n':
                    case '\r':
                        CommandProcessor.Execute(currentInput, this);
                        InputHistory.Add(currentInput);
                        historyIndex = InputHistory.Count;
                        currentInput = string.Empty;
                        renderer.InputString = currentInput;
                        return;
                    case '\b':
                        if (currentInput.Length > 0)
                            currentInput = currentInput[0..^1];
                        break;
                    default:
                        currentInput += c.ToString();
                        break;
                }
            }

            if (InputHistory.Count != 0)
            {
                if (Input.IsKeyReleased(Key.Up))
                {
                    historyIndex = Utilities.Clamp(historyIndex - 1, 0, InputHistory.Count - 1);
                    currentInput = InputHistory[historyIndex];
                }

                if (Input.IsKeyReleased(Key.Down))
                {
                    historyIndex = Utilities.Clamp(historyIndex + 1, 0, InputHistory.Count);
                    if (historyIndex == InputHistory.Count)
                        currentInput = "";
                    else
                        currentInput = InputHistory[historyIndex];
                }
            }

            if (Input.IsKeyReleased(Key.Escape))
                IsActive = false;

            renderer.InputString = currentInput;
        }

        public void Render()
        {
            renderer.Render();
        }
    }
}
