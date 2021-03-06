﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine.Invocation
{
    public class SystemConsole : ITerminal
    {
        private VirtualTerminalMode _virtualTerminalMode;
        private readonly ConsoleColor _initialForegroundColor;
        private readonly ConsoleColor _initialBackgroundColor;

        internal SystemConsole()
        {
            _initialForegroundColor = Console.ForegroundColor;
            _initialBackgroundColor = Console.BackgroundColor;
            Error = StandardStreamWriter.Create(Console.Error);
            Out = StandardStreamWriter.Create(Console.Out);
        }

        public void SetOut(TextWriter writer) => Console.SetOut(writer);

        public IStandardStreamWriter Error { get; }

        public IStandardStreamWriter Out { get; }

        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();

        public Region GetRegion() =>
            IsOutputRedirected
                ? new Region(0, 0, int.MaxValue, int.MaxValue, false)
                : EntireConsoleRegion.Instance;

        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public bool IsOutputRedirected => Console.IsOutputRedirected;
        
        public bool IsVirtualTerminal
        {
            get
            {
                if (_virtualTerminalMode != null)
                {
                    return _virtualTerminalMode.IsEnabled;
                }

                var terminalName = Environment.GetEnvironmentVariable("TERM");

                return !string.IsNullOrEmpty(terminalName)
                       && terminalName.StartsWith("xterm", StringComparison.OrdinalIgnoreCase);
            }
        }

        public void TryEnableVirtualTerminal()
        {
            if (IsOutputRedirected)
            {
                return;
            }

            _virtualTerminalMode = VirtualTerminalMode.TryEnable();
        }

        private void ResetConsole()
        {
            _virtualTerminalMode?.Dispose();

            Console.ForegroundColor = _initialForegroundColor;
            Console.BackgroundColor = _initialBackgroundColor;
        }

        public void Dispose()
        {
            ResetConsole();
            GC.SuppressFinalize(this);
        }

        ~SystemConsole()
        {
            ResetConsole();
        }

        public static ITerminal Create() => new SystemConsole();
    }
}
