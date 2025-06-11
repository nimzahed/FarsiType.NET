using System;
using System.Buffers;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FarsiTypeNet
{
    /// <summary>
    /// Represents the connection state of a Farsi character within a word.
    /// </summary>
    public enum WordConnection
    {
        /// <summary>Default connection (no special connection).</summary>
        Default = 0,
        /// <summary>Isolated character (not connected to any other character).</summary>
        Isolated,
        /// <summary>Connected to the previous character only.</summary>
        Previous,
        /// <summary>Connected to the next character only.</summary>
        Next,
        /// <summary>Connected to both previous and next characters.</summary>
        Both,
    }

    /// <summary>
    /// Specifies the text direction/order for Farsi rendering.
    /// </summary>
    public enum FarsiOrder
    {
        /// <summary>Automatic order based on content.</summary>
        Default = 0,
        /// <summary>Left-to-right order.</summary>
        LTR,
        /// <summary>Right-to-left order.</summary>
        RTL
    }

    /// <summary>
    /// Enumerates Farsi letters and ligatures, mapped to their Unicode code points.
    /// </summary>
    public enum FarsiWord
    {
        FA_ALEF_HAMZEH_ABOVE = 0, // أ
        FA_ALEF, // ا
        FA_ALEF_MAD_ABOVE, // آ
        FA_HAMZEH, // ء
        FA_VAAV_HAMZEH_ABOVE, // ؤ
        FA_ALEF_HAMZEH_BELOW, // إ
        FA_YEH_HAMZEH_ABOVE, // ئ
        FA_BEH, // ب
        FA_PEH, // پ
        FA_TEH, // ت
        FA_SEH, // ث
        FA_JEEM, // ج
        FA_CHEH, // چ
        FA_HEH_JEEMY, // ح
        FA_KHEH, // خ
        FA_DAAL, // د
        FA_ZAAL, // ذ
        FA_REH, // ر
        FA_ZEH, // ز
        FA_JEH, // ژ
        FA_SEEN, // س
        FA_SHEEN, // ش
        FA_SAAD, // ص
        FA_ZAAD, // ض
        FA_TAAH, // ط
        FA_ZAAH, // ظ
        FA_AIN, // ع
        FA_GHAIN, // غ
        FA_FEH, // ف
        FA_QAAF, // ق
        FA_KAAF, // ک
        FA_GAAF, // گ
        FA_LAAM, // ل
        FA_MEEM, // م
        FA_NOON, // ن
        FA_VAAV, // و
        FA_HEH, // ه
        FA_YEH, // ی
        FA_ARABIC_YEH, // ي
        FA_LAAM_ALEF, // لا
        FA_LAAM_ALEF_HAMZEH_ABOVE, // لأ
        FA_LAAM_ALEF_MAD_ABOVE, // لآ
        Count
    }

    /// <summary>
    /// Provides static methods for Farsi text shaping, glyph selection, and string manipulation.
    /// </summary>
    public static class FarsiType
    {
        private static bool useIsolated = true;
        public static bool UseIsolated => useIsolated;
        public static void SetUseIsolated(bool value)
        {
            useIsolated = value;
        }

        /// <summary>
        /// Lookup table for Farsi glyphs and their contextual forms.
        /// </summary>
        static readonly FarsiGlyphChar[] farsiGlyphChars = new FarsiGlyphChar[]
        {
            // Each entry: (base, isolated, initial, medial, final)
            new FarsiGlyphChar('\u0623','\uFE83','\u0623','\uFE84','\uFE84'), // FA_ALEF_HAMZEH_ABOVE, // أ
            new FarsiGlyphChar('\u0627','\uFE8D','\u0627','\uFE8E','\uFE8E'), // FA_ALEF, // ا
            new FarsiGlyphChar('\u0622','\uFE81','\u0622','\uFE82','\uFE82'), // FA_ALEF_MAD_ABOVE, // آ
            new FarsiGlyphChar('\u0621','\uFE80','\u0621','\u0621','\u0621'), // FA_HAMZEH, // ء
            new FarsiGlyphChar('\u0624','\uFE85','\u0624','\uFE86','\uFE86'), // FA_VAAV_HAMZEH_ABOVE, // ؤ
            new FarsiGlyphChar('\u0625','\uFE87','\u0625','\uFE88','\uFE88'), // FA_ALEF_HAMZEH_BELOW, // إ
            new FarsiGlyphChar('\u0626','\uFE89','\uFE8B','\uFE8C','\uFE8A'), // FA_YEH_HAMZEH_ABOVE, // ئ
            new FarsiGlyphChar('\u0628','\uFE8F','\uFE91','\uFE92','\uFE90'), // FA_BEH, // ب
            new FarsiGlyphChar('\u067E','\uFB56','\uFB58','\uFB59','\uFB57'), // FA_PEH, // پ
            new FarsiGlyphChar('\u062A','\uFE95','\uFE97','\uFE98','\uFE96'), // FA_TEH, // ت
            new FarsiGlyphChar('\u062B','\uFE99','\uFE9B','\uFE9C','\uFE9A'), // FA_SEH, // ث
            new FarsiGlyphChar('\u062C','\uFE9D','\uFE9F','\uFEA0','\uFE9E'), // FA_JEEM, // ج
            new FarsiGlyphChar('\u0686','\uFB7A','\uFB7C','\uFB7D','\uFB7B'), // FA_CHEH, // چ
            new FarsiGlyphChar('\u062D','\uFEA1','\uFEA3','\uFEA4','\uFEA2'), // FA_HEH_JEEMY, // ح
            new FarsiGlyphChar('\u062E','\uFEA5','\uFEA7','\uFEA8','\uFEA6'), // FA_KHEH, // خ
            new FarsiGlyphChar('\u062F','\uFEA9','\u062F','\uFEAA','\uFEAA'), // FA_DAAL, // د
            new FarsiGlyphChar('\u0630','\uFEAB','\u0630','\uFEAC','\uFEAC'), // FA_ZAAL, // ذ
            new FarsiGlyphChar('\u0631','\uFEAD','\u0631','\uFEAE','\uFEAE'), // FA_REH, // ر
            new FarsiGlyphChar('\u0632','\uFEAF','\u0632','\uFEB0','\uFEB0'), // FA_ZEH, // ز
            new FarsiGlyphChar('\u0698','\uFB8A','\u0698','\uFB8B','\uFB8B'), // FA_JEH, // ژ
            new FarsiGlyphChar('\u0633','\uFEB1','\uFEB3','\uFEB4','\uFEB2'), // FA_SEEN, // س
            new FarsiGlyphChar('\u0634','\uFEB5','\uFEB7','\uFEB8','\uFEB6'), // FA_SHEEN, // ش
            new FarsiGlyphChar('\u0635','\uFEB9','\uFEBB','\uFEBC','\uFEBA'), // FA_SAAD, // ص
            new FarsiGlyphChar('\u0636','\uFEBD','\uFEBF','\uFEC0','\uFEBE'), // FA_ZAAD, // ض
            new FarsiGlyphChar('\u0637','\uFEC1','\uFEC3','\uFEC4','\uFEC2'), // FA_TAAH, // ط
            new FarsiGlyphChar('\u0638','\uFEC5','\uFEC7','\uFEC8','\uFEC6'), // FA_ZAAH, // ظ
            new FarsiGlyphChar('\u0639','\uFEC9','\uFECB','\uFECC','\uFECA'), // FA_AIN, // ع
            new FarsiGlyphChar('\u063A','\uFECD','\uFECF','\uFED0','\uFECE'), // FA_GHAIN, // غ
            new FarsiGlyphChar('\u0641','\uFED1','\uFED3','\uFED4','\uFED2'), // FA_FEH, // ف
            new FarsiGlyphChar('\u0642','\uFED5','\uFED7','\uFED8','\uFED6'), // FA_QAAF, // ق
            new FarsiGlyphChar('\u06A9','\uFED9','\uFEDB','\uFEDC','\uFEDA'), // FA_KAAF, // ک
            new FarsiGlyphChar('\u06AF','\uFB92','\uFB94','\uFB95','\uFB93'), // FA_GAAF, // گ
            new FarsiGlyphChar('\u0644','\uFEDD','\uFEDF','\uFEE0','\uFEDE'), // FA_LAAM, // ل
            new FarsiGlyphChar('\u0645','\uFEE1','\uFEE3','\uFEE4','\uFEE2'), // FA_MEEM, // م
            new FarsiGlyphChar('\u0646','\uFEE5','\uFEE7','\uFEE8','\uFEE6'), // FA_NOON, // ن
            new FarsiGlyphChar('\u0648','\uFEED','\uFEED','\uFEEE','\uFEEE'), // FA_VAAV, // و
            new FarsiGlyphChar('\u0647','\uFEE9','\uFEEB','\uFEEC','\uFEEA'), // FA_HEH, // ه
            new FarsiGlyphChar('\u06CC','\uFBFC','\uFBFE','\uFBFF','\uFBFD'), // FA_YEH, // ی
            new FarsiGlyphChar('\u064A','\uFEF1','\uFEF3','\uFEF4','\uFEF2'), // FA_ARABIC_YEH, // ي
            new FarsiGlyphChar('\uFEFB','\uFEFB','\uFEFB','\uFEFC','\uFEFC'), // FA_LAAM_ALEF, // لا
            new FarsiGlyphChar('\uFEF7','\uFEF7','\uFEF7','\uFEF8','\uFEF8'), // FA_LAAM_ALEF_HAMZEH_ABOVE, // لأ
            new FarsiGlyphChar('\uFEF5','\uFEF5','\uFEF5','\uFEF6','\uFEF6'), // FA_LAAM_ALEF_MAD_ABOVE, // لآ
        };

        /// <summary>
        /// Determines if a character is compatible with Farsi script (in Unicode Farsi/Arabic blocks).
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is Farsi-compatible; otherwise, false.</returns>
        public static bool IsFarsiCompatible(char c, bool farsiSpecials = true)
        {
            
            return (farsiSpecials && (c == '\r' || c=='\n')) || (farsiSpecials && c == '.')
                || (c >= '\u0600' && c <= '\u06FF')  // Farsi block
                || (c >= '\u0750' && c <= '\u077F')  // Arabic Supplement
                || (c >= '\u08A0' && c <= '\u08FF')  // Arabic Extended-A
                || (c >= '\uFB50' && c <= '\uFDFF')  // Arabic Presentation Forms-A
                || (c >= '\uFE70' && c <= '\uFEFF')  // Arabic Presentation Forms-B
                || (c == '\u0627')  // Specific Farsi characters, like Alef
                || (c == '\u06CC'); // Persian Yeh
        }

        /// <summary>
        /// Checks if the first character of a string is Farsi-compatible.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns>True if the first character is Farsi-compatible; otherwise, false.</returns>
        public static bool IsFarsiBeginner(string str)
        {
            return IsFarsiCompatible(str[0]);
        }

        /// <summary>
        /// Finds the index of the first Farsi-compatible character in a string.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <returns>The index of the first Farsi character, or -1 if not found.</returns>
        public static int GetFarsiIndex(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (IsFarsiCompatible(str[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Automatically shapes and reorders a string for Farsi display, using contextual glyphs and RTL logic.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The shaped and reordered string for Farsi display.</returns>
        public static string GetAutoGlyph(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = GetFarsiCharsGlyph(text);
            bool useRTL = IsFarsiCompatible(text[0]);
            text = ReverseFarsi(text, useRTL);
            if (!useRTL)
            {
                return text;
            }
            return ReverseWordsPlacement(text);
        }

        /// <summary>
        /// Reverses the placement of words in a string, used for RTL Farsi rendering.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The string with word order reversed for RTL display.</returns>
        public static string ReverseWordsPlacement(string text)
        {
            if (text.Length <= 0)
            {
                return "";
            }
            bool isFarsi = IsFarsiCompatible(text[text.Length - 1]);
            StringBuilder result = new StringBuilder(text.Length + 10);
            int segmentStart = text.Length - 1;

            for (int i = text.Length - 1; i >= 0; i--)
            {
                bool currentIsFarsi = IsFarsiCompatible(text[i]);
                if (isFarsi != currentIsFarsi)
                {
                    AppendSegment(result, text.AsSpan(i + 1, segmentStart - i), !isFarsi);
                    segmentStart = i;
                    isFarsi = currentIsFarsi;
                }
            }
            AppendSegment(result, text.AsSpan(0, segmentStart + 1), !isFarsi);
            return result.ToString();
        }

        /// <summary>
        /// Determines if a character is considered "nothing" (null or space).
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is null or a space; otherwise, false.</returns>
        public static bool IsNothing(char c)
        {
            return c == '\0' || c == ' ';
        }

        /// <summary>
        /// Determines if a Farsi character can connect to the next character in a word.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character can connect to the next; otherwise, false.</returns>
        public static bool IsConnectedToNext(char c)
        {
            if (IsNothing(c) || !IsFarsiCompatible(c, false))
            {
                return false;
            }
            else if (c == GetChar(FarsiWord.FA_ALEF) || c == GetChar(FarsiWord.FA_ALEF_HAMZEH_ABOVE) ||
                c == GetChar(FarsiWord.FA_ALEF_HAMZEH_BELOW) || c == GetChar(FarsiWord.FA_ALEF_MAD_ABOVE) ||
                c == GetChar(FarsiWord.FA_HAMZEH) || c == GetChar(FarsiWord.FA_VAAV_HAMZEH_ABOVE) ||
                (c >= GetChar(FarsiWord.FA_DAAL) && c <= GetChar(FarsiWord.FA_ZEH)) ||
                c == GetChar(FarsiWord.FA_JEH) || c == GetChar(FarsiWord.FA_VAAV)
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a character can connect to the previous character (used for punctuation).
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character can connect to the previous; otherwise, false.</returns>
        public static bool IsConnectedToBack(char c)
        {
            switch (c)
            {
                case '\r':
                case '\n':
                case '.':
                case '\u061F': // Arabic question mark
                case '\u060C': // Arabic comma
                case '\u061B': // Arabic semicolon
                case '\u066B': // Arabic decimal separator
                case '\u066C': // Arabic thousands separator
                    break;
                default:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the correct contextual glyph for a Farsi character based on its position in a word.
        /// </summary>
        /// <param name="character">The Farsi character.</param>
        /// <param name="previousCharacter">The previous character in the word (default: '\0').</param>
        /// <param name="nextCharacter">The next character in the word (default: '\0').</param>
        /// <returns>The contextual glyph character.</returns>
        public static char GetFarsiCharGlyph(char character, char previousCharacter = '\0', char nextCharacter = '\0')
        {
            if (!IsFarsiCompatible(character))
            {
                return character;
            }

            bool isEndCompatible = IsFarsiCompatible(nextCharacter) && IsConnectedToBack(nextCharacter);
            bool connectedBack = IsConnectedToNext(previousCharacter);
            bool connectedFront = IsConnectedToNext(character) && isEndCompatible;
            bool isEnd = IsNothing(nextCharacter);
            WordConnection conn = WordConnection.Default;
            if (isEnd && IsNothing(previousCharacter)) // Single character
            {
                conn = WordConnection.Default;
            }
            else if (connectedBack && !connectedFront) // Connected only to previous
            {
                conn = WordConnection.Previous;
            }
            else if (connectedFront && !connectedBack) // Connected only to next
            {
                conn = WordConnection.Next;
            }
            else if (!connectedBack && !connectedFront) // Isolated in word
            {
                if (useIsolated)
                {
                    conn = WordConnection.Isolated;
                }
                else
                {
                    conn = WordConnection.Default;
                }
            }
            else if (connectedBack && connectedFront) // Connected both sides
            {
                conn = WordConnection.Both;
            }

            return GetExactGlyph(character, conn);
        }

        /// <summary>
        /// Converts a string to its contextual Farsi glyph representation.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The string with contextual Farsi glyphs.</returns>
        public static string GetFarsiCharsGlyph(string text)
        {
            StringBuilder newstring = new StringBuilder(text.Length);
            char lastChar = '\0';
            char nextChar = '\0';

            for (int i = 0; i < text.Length; i++)
            {
                nextChar = i + 1 < text.Length ? text[i + 1] : '\0';
                newstring.Append(GetFarsiCharGlyph(text[i], lastChar, nextChar));
                lastChar = text[i];
            }

            return newstring.ToString();
        }

        /// <summary>
        /// Converts a string to its contextual Farsi glyph representation and applies the specified text order.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="order">The desired Farsi text order.</param>
        /// <returns>The processed string with contextual glyphs and order.</returns>
        public static string GetFarsiGlyph(string text, FarsiOrder order = FarsiOrder.Default)
        {
            switch (order)
            {
                case FarsiOrder.Default:
                    return GetAutoGlyph(text);
                case FarsiOrder.LTR:
                    return ReverseFarsi(GetFarsiCharsGlyph(text), false);
                case FarsiOrder.RTL:
                    return ReverseWordsPlacement(ReverseFarsi(GetFarsiCharsGlyph(text), true));
                default:
                    return GetAutoGlyph(text);
            }
        }

        /// <summary>
        /// Gets the exact glyph for a Farsi character based on its connection state.
        /// </summary>
        /// <param name="c">The Farsi character.</param>
        /// <param name="conn">The connection state.</param>
        /// <returns>The glyph character for the specified connection.</returns>
        public static char GetExactGlyph(char c, WordConnection conn)
        {
            if (conn == WordConnection.Default)
            {
                return c;
            }
            for (int i = 0; i < farsiGlyphChars.Length; i++)
            {
                if (!farsiGlyphChars[i].IsCharacter(c))
                {
                    continue;
                }
                switch (conn)
                {
                    case WordConnection.Default:
                        return c;
                    case WordConnection.Previous:
                        return farsiGlyphChars[i].Final;
                    case WordConnection.Next:
                        return farsiGlyphChars[i].Initial;
                    case WordConnection.Both:
                        return farsiGlyphChars[i].Medial;
                    case WordConnection.Isolated:
                        return farsiGlyphChars[i].Isolated;
                    default:
                        continue;
                }
            }
            return c;
        }

        /// <summary>
        /// Reverses a string efficiently, using stack or pooled memory depending on length.
        /// </summary>
        /// <param name="text">The string to reverse.</param>
        /// <returns>The reversed string.</returns>
        public static string ReverseString(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            int length = text.Length;

            if (length < 1024)
            {
                Span<char> span = stackalloc char[length];
                text.AsSpan().CopyTo(span);
                span.Reverse();
                return new string(span);
            }
            else
            {
                char[] buffer = ArrayPool<char>.Shared.Rent(length);
                text.CopyTo(0, buffer, 0, length);
                ReverseInPlace(buffer, length);
                string reversed = new string(buffer, 0, length);
                ArrayPool<char>.Shared.Return(buffer, clearArray: false);
                return reversed;
            }
        }

        /// <summary>
        /// Reverses only the Farsi segments of a string, preserving the order of non-Farsi segments.
        /// If <paramref name="rtl"/> is true, all segments (including spaces) are reversed to enforce right-to-left order.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="rtl">
        /// If true, all segments (including spaces and non-Farsi) are reversed to enforce RTL order.
        /// If false, only Farsi segments are reversed, and non-Farsi segments (including spaces) remain in LTR order.
        /// </param>
        /// <returns>The processed string with Farsi segments reversed, and optionally all segments reversed for RTL.</returns>
        public static string ReverseFarsi(string text, bool rtl = false)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder result = new StringBuilder(text.Length + 10);
            int segmentStart = 0;
            bool isFarsi = IsFarsiCompatible(text[0]);

            bool doAppend = false;
            for (int i = 0; i < text.Length; i++)
            {
                bool currentIsFarsi = IsFarsiCompatible(text[i]);
                bool isNothing = IsNothing(text[i]);

                if (currentIsFarsi != isFarsi)
                {
                    doAppend = false;
                    if (rtl)
                    {
                        doAppend = true;
                    }
                    else if (!isNothing)
                    {
                        if (isFarsi) i--;
                        doAppend = true;
                    }

                    if (doAppend)
                    {
                        AppendSegment(result, text.AsSpan(segmentStart, i - segmentStart), isFarsi || rtl);
                        isFarsi = currentIsFarsi;
                        segmentStart = i;
                    }
                }
            }

            AppendSegment(result, text.AsSpan(segmentStart), isFarsi || rtl);
            return result.ToString();
        }

        /// <summary>
        /// Appends a segment to the result, reversing it if specified.
        /// </summary>
        /// <param name="result">The StringBuilder to append to.</param>
        /// <param name="span">The segment to append.</param>
        /// <param name="reverse">Whether to reverse the segment before appending.</param>
        private static void AppendSegment(StringBuilder result, ReadOnlySpan<char> span, bool reverse)
        {
            Span<char> tempSpan = span.Length <= 1024 ? stackalloc char[span.Length] : new char[span.Length];
            span.CopyTo(tempSpan);
            if (reverse)
            {
                ReverseInPlace(tempSpan);
            }
            result.Append(tempSpan);
        }

        /// <summary>
        /// Reverses a span of characters in place.
        /// </summary>
        /// <param name="span">The span to reverse.</param>
        private static void ReverseInPlace(Span<char> span)
        {
            int left = 0, right = span.Length - 1;
            while (left < right)
            {
                (span[left], span[right]) = (span[right], span[left]);
                left++;
                right--;
            }
        }

        /// <summary>
        /// Reverses a character array in place up to a specified length.
        /// </summary>
        /// <param name="buffer">The character array.</param>
        /// <param name="length">The number of characters to reverse.</param>
        private static void ReverseInPlace(char[] buffer, int length)
        {
            int left = 0, right = length - 1;
            while (left < right)
            {
                (buffer[left], buffer[right]) = (buffer[right], buffer[left]);
                left++;
                right--;
            }
        }

        /// <summary>
        /// Gets the base Unicode character for a FarsiWord enum value.
        /// </summary>
        /// <param name="word">The FarsiWord enum value.</param>
        /// <returns>The base character.</returns>
        public static char GetChar(FarsiWord word)
        {
            return farsiGlyphChars[(int)word].Letter;
        }

        /// <summary>
        /// Gets the base Unicode character for a FarsiWord index.
        /// </summary>
        /// <param name="word">The index of the FarsiWord.</param>
        /// <returns>The base character.</returns>
        public static char GetChar(int word)
        {
            return farsiGlyphChars[word].Letter;
        }
    }
}