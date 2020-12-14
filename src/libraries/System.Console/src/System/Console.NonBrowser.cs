// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace System
{
    public static partial class Console
    {
        private static TextReader? s_in;
        private static Encoding? s_inputEncoding;
        private static ConsolePal.ControlCHandlerRegistrar? s_registrar;
        private static ConsoleCancelEventHandler? s_cancelCallbacks;

        [UnsupportedOSPlatform("browser")]
        public static TextReader In
        {
            get
            {
                return Volatile.Read(ref s_in) ?? EnsureInitialized();

                static TextReader EnsureInitialized()
                {
                    // Must be placed outside s_syncObject lock. See Out getter.
                    ConsolePal.EnsureConsoleInitialized();

                    lock (s_syncObject) // Ensures In and InputEncoding are synchronized.
                    {
                        if (s_in == null)
                        {
                            Volatile.Write(ref s_in, ConsolePal.GetOrCreateReader());
                        }
                        return s_in;
                    }
                }
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static Encoding InputEncoding
        {
            get
            {
                Encoding? encoding = Volatile.Read(ref s_inputEncoding);
                if (encoding == null)
                {
                    lock (s_syncObject)
                    {
                        if (s_inputEncoding == null)
                        {
                            Volatile.Write(ref s_inputEncoding, ConsolePal.InputEncoding);
                        }
                        encoding = s_inputEncoding;
                    }
                }
                return encoding;
            }
            set
            {
                CheckNonNull(value, nameof(value));

                lock (s_syncObject)
                {
                    // Set the terminal console encoding.
                    ConsolePal.SetConsoleInputEncoding(value);

                    Volatile.Write(ref s_inputEncoding, (Encoding)value.Clone());

                    // We need to reinitialize 'Console.In' in the next call to s_in
                    // This will discard the current StreamReader, potentially
                    // losing buffered data.
                    Volatile.Write(ref s_in, null);
                }
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleKeyInfo ReadKey()
        {
            return ConsolePal.ReadKey(false);
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            return ConsolePal.ReadKey(intercept);
        }

        public static int CursorSize
        {
            [UnsupportedOSPlatform("browser")]
            get { return ConsolePal.CursorSize; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.CursorSize = value; }
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleColor BackgroundColor
        {
            get { return ConsolePal.BackgroundColor; }
            set { ConsolePal.BackgroundColor = value; }
        }

        [UnsupportedOSPlatform("browser")]
        public static ConsoleColor ForegroundColor
        {
            get { return ConsolePal.ForegroundColor; }
            set { ConsolePal.ForegroundColor = value; }
        }

        [UnsupportedOSPlatform("browser")]
        public static void ResetColor()
        {
            ConsolePal.ResetColor();
        }

        public static int BufferWidth
        {
            [UnsupportedOSPlatform("browser")]
            get { return ConsolePal.BufferWidth; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.BufferWidth = value; }
        }

        public static int BufferHeight
        {
            [UnsupportedOSPlatform("browser")]
            get { return ConsolePal.BufferHeight; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.BufferHeight = value; }
        }

        public static int WindowWidth
        {
            [UnsupportedOSPlatform("browser")]
            get { return ConsolePal.WindowWidth; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.WindowWidth = value; }
        }

        public static int WindowHeight
        {
            [UnsupportedOSPlatform("browser")]
            get { return ConsolePal.WindowHeight; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.WindowHeight = value; }
        }

        [UnsupportedOSPlatform("browser")]
        public static int LargestWindowWidth
        {
            get { return ConsolePal.LargestWindowWidth; }
        }

        [UnsupportedOSPlatform("browser")]
        public static int LargestWindowHeight
        {
            get { return ConsolePal.LargestWindowHeight; }
        }

        public static bool CursorVisible
        {
            [SupportedOSPlatform("windows")]
            get { return ConsolePal.CursorVisible; }
            [UnsupportedOSPlatform("browser")]
            set { ConsolePal.CursorVisible = value; }
        }

        [UnsupportedOSPlatform("browser")]
        public static int CursorLeft
        {
            get { return ConsolePal.GetCursorPosition().Left; }
            set { SetCursorPosition(value, CursorTop); }
        }

        [UnsupportedOSPlatform("browser")]
        public static int CursorTop
        {
            get { return ConsolePal.GetCursorPosition().Top; }
            set { SetCursorPosition(CursorLeft, value); }
        }

        /// <summary>Gets the position of the cursor.</summary>
        /// <returns>The column and row position of the cursor.</returns>
        /// <remarks>
        /// Columns are numbered from left to right starting at 0. Rows are numbered from top to bottom starting at 0.
        /// </remarks>
        [UnsupportedOSPlatform("browser")]
        public static (int Left, int Top) GetCursorPosition()
        {
            return ConsolePal.GetCursorPosition();
        }

        public static string Title
        {
            [SupportedOSPlatform("windows")]
            get { return ConsolePal.Title; }
            [UnsupportedOSPlatform("browser")]
            set
            {
                ConsolePal.Title = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static void Beep()
        {
            ConsolePal.Beep();
        }

        [UnsupportedOSPlatform("browser")]
        public static void SetCursorPosition(int left, int top)
        {
            // Basic argument validation.  The PAL implementation may provide further validation.
            if (left < 0 || left >= short.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(left), left, SR.ArgumentOutOfRange_ConsoleBufferBoundaries);
            if (top < 0 || top >= short.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(top), top, SR.ArgumentOutOfRange_ConsoleBufferBoundaries);

            ConsolePal.SetCursorPosition(left, top);
        }

        [UnsupportedOSPlatform("browser")]
        public static event ConsoleCancelEventHandler? CancelKeyPress
        {
            add
            {
                // Must be placed outside s_syncObject lock. See Out getter.
                ConsolePal.EnsureConsoleInitialized();

                lock (s_syncObject)
                {
                    s_cancelCallbacks += value;

                    // If we haven't registered our control-C handler, do it.
                    if (s_registrar == null)
                    {
                        s_registrar = new ConsolePal.ControlCHandlerRegistrar();
                        s_registrar.Register();
                    }
                }
            }
            remove
            {
                lock (s_syncObject)
                {
                    s_cancelCallbacks -= value;
                    if (s_registrar != null && s_cancelCallbacks == null)
                    {
                        s_registrar.Unregister();
                        s_registrar = null;
                    }
                }
            }
        }

        [UnsupportedOSPlatform("browser")]
        public static bool TreatControlCAsInput
        {
            get { return ConsolePal.TreatControlCAsInput; }
            set { ConsolePal.TreatControlCAsInput = value; }
        }

        [UnsupportedOSPlatform("browser")]
        public static Stream OpenStandardInput()
        {
            return ConsolePal.OpenStandardInput();
        }

        [UnsupportedOSPlatform("browser")]
        public static Stream OpenStandardInput(int bufferSize)
        {
            // bufferSize is ignored, other than in argument validation, even in the .NET Framework
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            return OpenStandardInput();
        }

        [UnsupportedOSPlatform("browser")]
        public static void SetIn(TextReader newIn)
        {
            CheckNonNull(newIn, nameof(newIn));
            newIn = SyncTextReader.GetSynchronizedTextReader(newIn);
            lock (s_syncObject)
            {
                Volatile.Write(ref s_in, newIn);
            }
        }

        //
        // Give a hint to the code generator to not inline the common console methods. The console methods are
        // not performance critical. It is unnecessary code bloat to have them inlined.
        //
        // Moreover, simple repros for codegen bugs are often console-based. It is tedious to manually filter out
        // the inlined console writelines from them.
        //
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        [UnsupportedOSPlatform("browser")]
        public static int Read()
        {
            return In.Read();
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        [UnsupportedOSPlatform("browser")]
        public static string? ReadLine()
        {
            return In.ReadLine();
        }

        internal static bool HandleBreakEvent(ConsoleSpecialKey controlKey)
        {
            ConsoleCancelEventHandler? handler = s_cancelCallbacks;
            if (handler == null)
            {
                return false;
            }

            var args = new ConsoleCancelEventArgs(controlKey);
            handler(null, args);
            return args.Cancel;
        }
    }
}
