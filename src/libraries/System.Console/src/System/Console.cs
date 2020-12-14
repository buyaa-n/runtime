// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace System
{
    public static partial class Console
    {
        // Unlike many other buffer sizes throughout .NET, which often only affect performance, this buffer size has a
        // functional impact on interactive console apps, where the size of the buffer passed to ReadFile/Console impacts
        // how many characters the cmd window will allow to be typed as part of a single line. It also does affect perf,
        // in particular when input is redirected and data may be consumed from a larger source. This 4K default size is the
        // same as is currently used by most other environments/languages tried.
        internal const int ReadBufferSize = 4096;
        // There's no visible functional impact to the write buffer size, and as we auto flush on every write,
        // there's little benefit to having a large buffer.  So we use a smaller buffer size to reduce working set.
        private const int WriteBufferSize = 256;

        private static readonly object s_syncObject = new object();
        private static TextWriter? s_out, s_error;
        private static Encoding? s_outputEncoding;
        private static bool s_isOutTextWriterRedirected;
        private static bool s_isErrorTextWriterRedirected;

        public static Encoding OutputEncoding
        {
            get
            {
                Encoding? encoding = Volatile.Read(ref s_outputEncoding);
                if (encoding == null)
                {
                    lock (s_syncObject)
                    {
                        if (s_outputEncoding == null)
                        {
                            Volatile.Write(ref s_outputEncoding, ConsolePal.OutputEncoding);
                        }
                        encoding = s_outputEncoding;
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
                    ConsolePal.SetConsoleOutputEncoding(value);

                    // Before changing the code page we need to flush the data
                    // if Out hasn't been redirected. Also, have the next call to
                    // s_out reinitialize the console code page.
                    if (s_out != null && !s_isOutTextWriterRedirected)
                    {
                        s_out.Flush();
                        Volatile.Write(ref s_out, null!);
                    }
                    if (s_error != null && !s_isErrorTextWriterRedirected)
                    {
                        s_error.Flush();
                        Volatile.Write(ref s_error, null!);
                    }

                    Volatile.Write(ref s_outputEncoding, (Encoding)value.Clone());
                }
            }
        }

        public static bool KeyAvailable
        {
            get
            {
                if (IsInputRedirected)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_ConsoleKeyAvailableOnFile);
                }

                return ConsolePal.KeyAvailable;
            }
        }

        public static TextWriter Out
        {
            get
            {
                // Console.Out shouldn't be locked while holding a lock on s_syncObject.
                // Otherwise there can be a deadlock when another thread locks these
                // objects in opposite order.
                //
                // Some functionality requires the console to be initialized.
                // On Linux, this initialization requires a lock on Console.Out.
                // The EnsureConsoleInitialized call must be placed outside the s_syncObject lock.
                Debug.Assert(!Monitor.IsEntered(s_syncObject));

                return Volatile.Read(ref s_out) ?? EnsureInitialized();

                static TextWriter EnsureInitialized()
                {
                    lock (s_syncObject) // Ensures Out and OutputEncoding are synchronized.
                    {
                        if (s_out == null)
                        {
                            Volatile.Write(ref s_out, CreateOutputWriter(OpenStandardOutput()));
                        }
                        return s_out;
                    }
                }
            }
        }

        public static TextWriter Error
        {
            get
            {
                return Volatile.Read(ref s_error) ?? EnsureInitialized();

                static TextWriter EnsureInitialized()
                {
                    lock (s_syncObject) // Ensures Error and OutputEncoding are synchronized.
                    {
                        if (s_error == null)
                        {
                            Volatile.Write(ref s_error, CreateOutputWriter(OpenStandardError()));
                        }
                        return s_error;
                    }
                }
            }
        }

        private static TextWriter CreateOutputWriter(Stream outputStream)
        {
            return TextWriter.Synchronized(outputStream == Stream.Null ?
                StreamWriter.Null :
                new StreamWriter(
                    stream: outputStream,
                    encoding: OutputEncoding.RemovePreamble(), // This ensures no prefix is written to the stream.
                    bufferSize: WriteBufferSize,
                    leaveOpen: true)
                {
                    AutoFlush = true
                });
        }

        private static StrongBox<bool>? _isStdInRedirected;
        private static StrongBox<bool>? _isStdOutRedirected;
        private static StrongBox<bool>? _isStdErrRedirected;

        public static bool IsInputRedirected
        {
            get
            {
                StrongBox<bool> redirected = Volatile.Read(ref _isStdInRedirected) ?? EnsureInitialized();
                return redirected.Value;

                static StrongBox<bool> EnsureInitialized()
                {
                    Volatile.Write(ref _isStdInRedirected, new StrongBox<bool>(ConsolePal.IsInputRedirectedCore()));
                    return _isStdInRedirected;
                }
            }
        }

        public static bool IsOutputRedirected
        {
            get
            {
                StrongBox<bool> redirected = Volatile.Read(ref _isStdOutRedirected) ?? EnsureInitialized();
                return redirected.Value;

                static StrongBox<bool> EnsureInitialized()
                {
                    Volatile.Write(ref _isStdOutRedirected, new StrongBox<bool>(ConsolePal.IsOutputRedirectedCore()));
                    return _isStdOutRedirected;
                }
            }
        }

        public static bool IsErrorRedirected
        {
            get
            {
                StrongBox<bool> redirected = Volatile.Read(ref _isStdErrRedirected) ?? EnsureInitialized();
                return redirected.Value;

                static StrongBox<bool> EnsureInitialized()
                {
                    Volatile.Write(ref _isStdErrRedirected, new StrongBox<bool>(ConsolePal.IsErrorRedirectedCore()));
                    return _isStdErrRedirected;
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public static bool NumberLock
        {
            get { return ConsolePal.NumberLock; }
        }

        [SupportedOSPlatform("windows")]
        public static bool CapsLock
        {
            get { return ConsolePal.CapsLock; }
        }

        internal const ConsoleColor UnknownColor = (ConsoleColor)(-1);

        [SupportedOSPlatform("windows")]
        public static void SetBufferSize(int width, int height)
        {
            ConsolePal.SetBufferSize(width, height);
        }

        public static int WindowLeft
        {
            get { return ConsolePal.WindowLeft; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.WindowLeft = value; }
        }

        public static int WindowTop
        {
            get { return ConsolePal.WindowTop; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.WindowTop = value; }
        }

        [SupportedOSPlatform("windows")]
        public static void SetWindowPosition(int left, int top)
        {
            ConsolePal.SetWindowPosition(left, top);
        }

        [SupportedOSPlatform("windows")]
        public static void SetWindowSize(int width, int height)
        {
            ConsolePal.SetWindowSize(width, height);
        }

        [SupportedOSPlatform("windows")]
        public static void Beep(int frequency, int duration)
        {
            ConsolePal.Beep(frequency, duration);
        }

        [SupportedOSPlatform("windows")]
        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            ConsolePal.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
        }

        [SupportedOSPlatform("windows")]
        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            ConsolePal.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);
        }

        public static void Clear()
        {
            ConsolePal.Clear();
        }

        public static Stream OpenStandardOutput()
        {
            return ConsolePal.OpenStandardOutput();
        }

        public static Stream OpenStandardOutput(int bufferSize)
        {
            // bufferSize is ignored, other than in argument validation, even in the .NET Framework
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            return OpenStandardOutput();
        }

        public static Stream OpenStandardError()
        {
            return ConsolePal.OpenStandardError();
        }

        public static Stream OpenStandardError(int bufferSize)
        {
            // bufferSize is ignored, other than in argument validation, even in the .NET Framework
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            return OpenStandardError();
        }

        public static void SetOut(TextWriter newOut)
        {
            CheckNonNull(newOut, nameof(newOut));
            newOut = TextWriter.Synchronized(newOut);
            lock (s_syncObject)
            {
                s_isOutTextWriterRedirected = true;
                Volatile.Write(ref s_out, newOut);
            }
        }

        public static void SetError(TextWriter newError)
        {
            CheckNonNull(newError, nameof(newError));
            newError = TextWriter.Synchronized(newError);
            lock (s_syncObject)
            {
                s_isErrorTextWriterRedirected = true;
                Volatile.Write(ref s_error, newError);
            }
        }

        private static void CheckNonNull(object obj, string paramName)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine()
        {
            Out.WriteLine();
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(bool value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(char value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(char[]? buffer)
        {
            Out.WriteLine(buffer);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(char[] buffer, int index, int count)
        {
            Out.WriteLine(buffer, index, count);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(decimal value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(double value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(float value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(int value)
        {
            Out.WriteLine(value);
        }

        [CLSCompliant(false)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(uint value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(long value)
        {
            Out.WriteLine(value);
        }

        [CLSCompliant(false)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(ulong value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(object? value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(string? value)
        {
            Out.WriteLine(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object? arg0)
        {
            Out.WriteLine(format, arg0);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object? arg0, object? arg1)
        {
            Out.WriteLine(format, arg0, arg1);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object? arg0, object? arg1, object? arg2)
        {
            Out.WriteLine(format, arg0, arg1, arg2);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, params object?[]? arg)
        {
            if (arg == null)                       // avoid ArgumentNullException from String.Format
                Out.WriteLine(format, null, null); // faster than Out.WriteLine(format, (Object)arg);
            else
                Out.WriteLine(format, arg);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(string format, object? arg0)
        {
            Out.Write(format, arg0);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(string format, object? arg0, object? arg1)
        {
            Out.Write(format, arg0, arg1);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(string format, object? arg0, object? arg1, object? arg2)
        {
            Out.Write(format, arg0, arg1, arg2);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(string format, params object?[]? arg)
        {
            if (arg == null)                   // avoid ArgumentNullException from String.Format
                Out.Write(format, null, null); // faster than Out.Write(format, (Object)arg);
            else
                Out.Write(format, arg);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(bool value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(char value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(char[]? buffer)
        {
            Out.Write(buffer);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(char[] buffer, int index, int count)
        {
            Out.Write(buffer, index, count);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(double value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(decimal value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(float value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(int value)
        {
            Out.Write(value);
        }

        [CLSCompliant(false)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(uint value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(long value)
        {
            Out.Write(value);
        }

        [CLSCompliant(false)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(ulong value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(object? value)
        {
            Out.Write(value);
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static void Write(string? value)
        {
            Out.Write(value);
        }
    }
}
