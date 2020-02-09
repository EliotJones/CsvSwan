﻿namespace CsvSwan
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal class CsvReader : IDisposable
    {
        private readonly BufferedStream stream;
        private readonly CsvOptions options;
        private readonly StreamReader reader;
        private readonly bool tabIsWhitespace;
        private readonly bool separatorIsEscapable;

        private readonly object mutex = new object();
        private readonly StringBuilder sb = new StringBuilder();

        private State state = State.None;

        private readonly List<string> output = new List<string>();

        public CsvReader(Stream inputStream, CsvOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            stream = new BufferedStream(inputStream ?? throw new ArgumentNullException(nameof(inputStream)));
            reader = options.Encoding == null ? new StreamReader(stream, true) : new StreamReader(stream, options.Encoding);

            tabIsWhitespace = options.Separator != '\t';
            separatorIsEscapable = options.Separator != '\t';
        }

        public long Position { get; set; }

        public bool ReadRow(out IReadOnlyList<string> values)
        {
            values = null;

            lock (mutex)
            {
                if (state == State.Complete)
                {
                    return false;
                }

                output.Clear();

                var prevWasEscaped = false;
                int prev = 0;
                int b;

                var lastQuoteType = LastQuoteType.None;

                while ((b = reader.Read()) >= 0)
                {
                    switch (state)
                    {
                        case State.Endline:
                            if (IsNewline(b))
                            {
                                break;
                            }

                            if (IsWhitespace(b, false, tabIsWhitespace))
                            {
                                state = State.None;
                                break;
                            }

                            if (IsQuote(b))
                            {
                                state = State.InsideQuote;
                                break;
                            }

                            if (IsSeperator(b))
                            {
                                state = State.None;
                                output.Add(sb.ToString());
                                sb.Clear();
                                break;
                            }

                            state = State.ReadingField;
                            sb.Append((char)b);
                            break;
                        case State.None:
                            {
                                if (IsWhitespace(b, false, tabIsWhitespace))
                                {
                                    break;
                                }

                                if (IsNewline(b))
                                {
                                    state = State.EndlineReturn;
                                    output.Add(sb.ToString());
                                    sb.Clear();
                                    break;
                                }

                                var isQuote = IsQuote(b);

                                state = isQuote
                                    ? State.InsideQuote
                                    : State.ReadingField;

                                if (!isQuote)
                                {
                                    // TODO: escape funkiness.
                                    if (IsSeperator(b))
                                    {
                                        state = State.None;
                                        output.Add(sb.ToString());
                                        sb.Clear();
                                        break;
                                    }

                                    sb.Append((char)b);
                                }
                                else
                                {
                                    lastQuoteType = LastQuoteType.FieldStart;
                                }
                            }
                            break;
                        case State.InsideQuote:
                            if (IsQuote(b))
                            {
                                var previousWasEscape = lastQuoteType == LastQuoteType.EscapeQuote;

                                if (previousWasEscape)
                                {
                                    lastQuoteType = LastQuoteType.None;
                                }
                                else if (reader.Peek() != '"')
                                {
                                    state = State.ReadingToSeparator;
                                    output.Add(sb.ToString());
                                    sb.Clear();
                                    break;
                                }
                                else
                                {
                                    lastQuoteType = LastQuoteType.EscapeQuote;
                                    break;
                                }
                            }
                            else
                            {
                                lastQuoteType = LastQuoteType.None;
                            }

                            var isEscape = IsEscape(b);

                            if (isEscape)
                            {
                                if (IsQuote(reader.Peek()))
                                {
                                    if (IsEscape(prev) && !prevWasEscaped)
                                    {

                                    }
                                    else
                                    {
                                        lastQuoteType = LastQuoteType.EscapeQuote;
                                        break;
                                    }
                                }
                                else if (!prevWasEscaped)
                                {
                                    break;
                                }
                            }

                            sb.Append((char)b);
                            break;
                        case State.ReadingField:
                            if (IsSeperator(b) && !IsEscapedSeparator(prevWasEscaped, prev))
                            {
                                state = State.None;
                                output.Add(sb.ToString());
                                sb.Clear();
                                break;
                            }

                            if (IsNewline(b))
                            {
                                state = State.EndlineReturn;
                                output.Add(sb.ToString());
                                sb.Clear();
                                break;
                            }

                            sb.Append((char)b);
                            break;
                        case State.ReadingToSeparator:
                            if (IsSeperator(b))
                            {
                                state = State.None;
                            }
                            else if (IsNewline(b))
                            {
                                state = State.EndlineReturn;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    prevWasEscaped = IsEscape(prev);
                    prev = b;

                    if (state == State.EndlineReturn)
                    {
                        values = output;
                        state = State.Endline;
                        return true;
                    }
                }

                if (state == State.None || state == State.ReadingField)
                {
                    output.Add(sb.ToString());
                    sb.Clear();
                    values = output;
                    state = State.Complete;
                    return true;
                }

                if (state == State.ReadingToSeparator)
                {
                    values = output;
                    state = State.Complete;
                    return true;
                }
            }

            return false;
        }

        private bool IsQuote(int b) => options.AreTextFieldsQuoted && b == options.QuotationCharacter;
        private bool IsSeperator(int b) => b == options.Separator;
        private bool IsEscape(int b) => options.BackslashEscapesQuotes && b == '\\';

        private bool IsEscapedSeparator(bool prevWasEscaped, int prev) => !separatorIsEscapable
                                                                          && !prevWasEscaped
                                                                          && IsEscape(prev);

        public void Dispose()
        {
            stream?.Dispose();
        }

        private static bool IsWhitespace(int value, bool includeNewline, bool includeTab)
        {
            if (value == 32)
            {
                return true;
            }

            if (includeNewline && IsNewline(value))
            {
                return true;
            }

            if (includeTab && value == 9)
            {
                return true;
            }

            return false;
        }

        private static bool IsNewline(int value)
        {
            return value == 13 || value == 10;
        }

        private enum State : byte
        {
            None = 0,
            InsideQuote = 1,
            ReadingField = 2,
            ReadingToSeparator = 3,
            Endline = 4,
            EndlineReturn = 5,
            Complete = 6
        }

        private enum LastQuoteType : byte
        {
            None = 0,
            FieldStart = 1,
            EscapeQuote = 2
        }

        public void SeekStart()
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            // TODO: header row
            if (options.IncludesHeaderRow)
            {
                // read row
            }

            state = State.None;
        }
    }
}

