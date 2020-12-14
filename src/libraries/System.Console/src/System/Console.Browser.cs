// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace System
{
    public static partial class Console
    {
        [UnsupportedOSPlatform("browser")]
        public static TextReader In => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public static Encoding InputEncoding
        {
            get => throw new PlatformNotSupportedException();
            set
            {
                throw new PlatformNotSupportedException();
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleKeyInfo ReadKey()
        {
            throw new PlatformNotSupportedException();
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            throw new PlatformNotSupportedException();
        }

        public static int CursorSize
        {
            [UnsupportedOSPlatform("browser")]
            get => throw new PlatformNotSupportedException();
            [SupportedOSPlatform("windows")]
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleColor BackgroundColor
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleColor ForegroundColor
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static void ResetColor()
        {
            throw new PlatformNotSupportedException();
        }

        public static int BufferWidth
        {
            [UnsupportedOSPlatform("browser")]
            get => throw new PlatformNotSupportedException();
            [SupportedOSPlatform("windows")]
            set { throw new PlatformNotSupportedException(); }
        }

        public static int BufferHeight
        {
            [UnsupportedOSPlatform("browser")]
            get => throw new PlatformNotSupportedException();
            [SupportedOSPlatform("windows")]
            set { throw new PlatformNotSupportedException(); }
        }

        public static int WindowWidth
        {
            [UnsupportedOSPlatform("browser")]
            get => throw new PlatformNotSupportedException();
            [SupportedOSPlatform("windows")]
            set { throw new PlatformNotSupportedException(); }
        }

        public static int WindowHeight
        {
            [UnsupportedOSPlatform("browser")]
            get => throw new PlatformNotSupportedException();
            [SupportedOSPlatform("windows")]
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static int LargestWindowWidth
        {
            get => throw new PlatformNotSupportedException();
        }

        [UnsupportedOSPlatform("browser")]
        public static int LargestWindowHeight
        {
            get => throw new PlatformNotSupportedException();
        }

        public static bool CursorVisible
        {
            [SupportedOSPlatform("windows")]
            get => throw new PlatformNotSupportedException();
            [UnsupportedOSPlatform("browser")]
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static int CursorLeft
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static int CursorTop
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static (int Left, int Top) GetCursorPosition() => throw new PlatformNotSupportedException();

        public static string Title
        {
            [SupportedOSPlatform("windows")]
            get => throw new PlatformNotSupportedException();
            [UnsupportedOSPlatform("browser")]
            set
            {
                throw new PlatformNotSupportedException();
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static void Beep()
        {
            throw new PlatformNotSupportedException();
        }

        [UnsupportedOSPlatform("browser")]
        public static void SetCursorPosition(int left, int top)
        {
            throw new PlatformNotSupportedException();
        }

        [UnsupportedOSPlatform("browser")]
        public static event ConsoleCancelEventHandler? CancelKeyPress
        {
            add
            {
                throw new PlatformNotSupportedException();
            }
            remove
            {
                throw new PlatformNotSupportedException();
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static bool TreatControlCAsInput
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public static Stream OpenStandardInput() => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public static Stream OpenStandardInput(int bufferSize) => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public static void SetIn(TextReader newIn)
        {
            throw new PlatformNotSupportedException();
        }

        [UnsupportedOSPlatform("browser")]
        public static int Read() => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public static string? ReadLine() => throw new PlatformNotSupportedException();
    }
}
