namespace FarsiTypeNet
{
    /// <summary>
    /// Represents a Farsi character and its contextual glyph forms (isolated, initial, medial, final).
    /// </summary>
    public struct FarsiGlyphChar
    {
        /// <summary>
        /// The base Unicode character for this Farsi letter.
        /// </summary>
        private char _letter;

        /// <summary>
        /// Gets the base Unicode character for this Farsi letter.
        /// </summary>
        public char Letter => _letter;

        /// <summary>
        /// The isolated form of the Farsi character (when not connected to any other character).
        /// </summary>
        private char _isolated;

        /// <summary>
        /// Gets the isolated form of the Farsi character.
        /// </summary>
        public char Isolated => _isolated;

        /// <summary>
        /// The initial form of the Farsi character (when connected only to the next character).
        /// </summary>
        private char _initial;

        /// <summary>
        /// Gets the initial form of the Farsi character.
        /// </summary>
        public char Initial => _initial;

        /// <summary>
        /// The medial form of the Farsi character (when connected to both previous and next characters).
        /// </summary>
        private char _medial;

        /// <summary>
        /// Gets the medial form of the Farsi character.
        /// </summary>
        public char Medial => _medial;

        /// <summary>
        /// The final form of the Farsi character (when connected only to the previous character).
        /// </summary>
        private char _final;

        /// <summary>
        /// Gets the final form of the Farsi character.
        /// </summary>
        public char Final => _final;

        /// <summary>
        /// Initializes a new instance of the <see cref="FarsiGlyphChar"/> struct with all contextual forms.
        /// </summary>
        /// <param name="letter">The base Unicode character.</param>
        /// <param name="isolated">The isolated form of the character.</param>
        /// <param name="initial">The initial form of the character.</param>
        /// <param name="medial">The medial form of the character.</param>
        /// <param name="final">The final form of the character.</param>
        public FarsiGlyphChar(char letter, char isolated, char initial, char medial, char final)
        {
            _letter = letter;
            _isolated = isolated;
            _initial = initial;
            _medial = medial;
            _final = final;
        }

        /// <summary>
        /// Determines whether the specified character matches any of the contextual forms of this Farsi character.
        /// </summary>
        /// <param name="c">The character to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="c"/> matches the base, isolated, initial, medial, or final form; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCharacter(char c)
        {
            return c == _letter || c == _isolated || c == _initial || c == _medial || c == _final;
        }
    }
}