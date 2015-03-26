using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace IsoBaseMediaFileFormat.File
{
    public class CopyrightBox:FullBox
    {
        public CopyrightBox()
            : base("cprt", 0, 0)
        {

        }

        const bool pad = false;

        private BitArray[] language = null;
        public string Language
        {
            get
            {
                return GetLanguageAsString();
            }
            set
            {
                SetLanguage(value);
            }
        }

        public string Notice
        {
            get;
            set;
        }
        
        public BitArray[] GetLanguage()
        {
            return language;
        }

        public string GetLanguageAsString()
        {
            if (language == null)
                return null;

            string languageAsString = string.Empty;
            foreach (BitArray ba in language)
            {
                byte[] byteArray = new byte[1];
                ba.CopyTo(byteArray, 0);
                languageAsString += (char)(byteArray[0] + 0x60);
            };

            return languageAsString;
        }

        public void SetLanguage(BitArray[] language)
        {
            if(language.Length != 3 || language.Any(ba => ba.Count != 5))
                throw new ArgumentOutOfRangeException();

            this.language = language;
        }

        public void SetLanguage(string language)
        {
            Regex.IsMatch(language, "[a-z]{3}");
            this.language = language.Select(c => new BitArray(new byte[] { (byte)(c - 0x60) }) { Length = 5 }).ToArray();
        }
    }
}
