namespace AIEnv.Embeddings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class BasicTokenizer
    {
        private bool _doLowerCase;
        private HashSet<string> _neverSplit;
        private bool _tokenizeChineseChars;
        private bool _stripAccents;

        public BasicTokenizer(bool doLowerCase = true, IEnumerable<string> neverSplit = null, bool tokenizeChineseChars = true, bool stripAccents = false)
        {
            if (neverSplit is null)
            {
                neverSplit = new List<string>();
            }
            _doLowerCase = doLowerCase;
            _neverSplit = new HashSet<string>(neverSplit);
            _tokenizeChineseChars = tokenizeChineseChars;
            _stripAccents = stripAccents;
        }

        public string[] Tokenize(string text, IEnumerable<string> never_split)
        {
            never_split = never_split == null ? _neverSplit : _neverSplit.Union(new HashSet<string>(never_split));
            text = CleanText(text);
            if (_tokenizeChineseChars)
            {
                text = TokenizeChineseChars(text);
            }
            var orig_tokens = WhitespaceTokenize(text);
            var split_tokens = new List<string>();
            foreach (var token in orig_tokens)
            {
                if (!never_split.Contains(token))
                {
                    var lower_token = _doLowerCase ? token.ToLower() : token;
                    if (_stripAccents is not false)
                    {
                        lower_token = RunStripAccents(lower_token);
                    }
                    else if (_stripAccents)
                    {
                        lower_token = RunStripAccents(lower_token);
                    }
                    split_tokens.AddRange(RunSplitOnPunct(lower_token, never_split));
                }
                else
                {
                    split_tokens.Add(token);
                }
            }
            var output_tokens = WhitespaceTokenize(string.Join(" ", split_tokens));
            return output_tokens;
        }

        private string RunStripAccents(string text)
        {
            text = text.Normalize(NormalizationForm.FormD);
            var output = new StringBuilder();
            foreach (var ch in text)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }
                output.Append(ch);
            }
            return output.ToString();
        }

        private List<string> RunSplitOnPunct(string text, IEnumerable<string> never_split = null)
        {
            if (never_split is not null && never_split.Contains(text))
            {
                return new List<string>() { text };
            }
            var chars = text.ToCharArray();
            var i = 0;
            var start_new_word = true;
            var output = new List<string>();
            var cur_output = new StringBuilder();
            while (i < chars.Length)
            {
                var ch = chars[i];
                if (IsPunctuation(ch))
                {
                    if (cur_output.Length > 0)
                    {
                        output.Add(cur_output.ToString());
                        cur_output.Clear();
                    }
                    output.Add(ch.ToString());
                    start_new_word = true;
                }
                else
                {
                    if (start_new_word)
                    {
                        cur_output.Clear();
                        output.Add(cur_output.ToString());
                    }
                    cur_output.Append(ch);
                    start_new_word = false;
                }
                i += 1;
            }
            if (cur_output.Length > 0)
            {
                output.Add(cur_output.ToString());
            }
            return output;
        }

        private string TokenizeChineseChars(string text)
        {
            var output = new StringBuilder();
            foreach (var ch in text)
            {
                var cp = char.ConvertToUtf32(ch.ToString(), 0);
                if (IsChineseChar(cp))
                {
                    output.Append(" ");
                    output.Append(ch);
                    output.Append(" ");
                }
                else
                {
                    output.Append(ch);
                }
            }
            return output.ToString();
        }

        private bool IsChineseChar(int cp)
        {
            return cp >= 0x4E00 && cp <= 0x9FFF
                || cp >= 0x3400 && cp <= 0x4DBF
                || cp >= 0x20000 && cp <= 0x2A6DF
                || cp >= 0x2A700 && cp <= 0x2B73F
                || cp >= 0x2B740 && cp <= 0x2B81F
                || cp >= 0x2B820 && cp <= 0x2CEAF
                || cp >= 0xF900 && cp <= 0xFAFF
                || cp >= 0x2F800 && cp <= 0x2FA1F;
        }

        private string CleanText(string text)
        {
            var output = new StringBuilder();
            foreach (var ch in text)
            {
                var cp = char.ConvertToUtf32(ch.ToString(), 0);
                if (cp == 0 || cp == 0xFFFD || IsControl(ch))
                {
                    continue;
                }
                if (IsWhitespace(ch))
                {
                    output.Append(" ");
                }
                else
                {
                    output.Append(ch);
                }
            }
            return output.ToString();
        }

        private string[] WhitespaceTokenize(string text)
        {
            return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsControl(char ch)
        {
            return char.IsControl(ch);
        }

        private bool IsWhitespace(char ch)
        {
            return char.IsWhiteSpace(ch);
        }

        private bool IsPunctuation(char ch)
        {
            return char.IsPunctuation(ch);
        }
    }
}
