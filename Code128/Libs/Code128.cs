using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Code128
{
    /// <summary>
    /// based on https://github.com/mormegil-cz/GenCode128
    /// </summary>
    public class Code128
    {
        #region common

        public enum eCodeSet
        {
            CodeA,
            CodeB
            //// CodeC   // not supported
        }

        private const int CShift = 98;

        private const int CCodeA = 101;

        private const int CCodeB = 100;

        private const int CStartA = 103;

        private const int CStartB = 104;

        private const int CStop = 106;

        /// <summary>
        /// Indicates which code sets can represent a character -- CodeA, CodeB, or either
        /// </summary>
        public enum CodeSetAllowed
        {
            CodeA,

            CodeB,

            CodeAorB
        }

        /// <summary>
        /// from https://github.com/graphicore/librebarcode/blob/master/app/lib/code128Encoder/encoder.mjs
        /// </summary>
        private Dictionary<int, string> dict = new Dictionary<int, string>()
        {
            {0,"Â"},        // space
            {1,"!"},        // 
            {2,"\""},       // 
            {3,"#"},        // 
            {4,"$"},        // 
            {5,"%"},        // 
            {6,"&"},        // 
            {7,"'"},        // 
            {8,"("},        // 
            {9,")"},        // 
            {10,"*"},       // 
            {11,"+"},       // 
            {12,","},       // 
            {13,"-"},       // 
            {14,"."},       // 
            {15,"/"},       // 
            {16,"0"},       // 
            {17,"1"},       // 
            {18,"2"},       // 
            {19,"3"},       // 
            {20,"4"},       // 
            {21,"5"},       // 
            {22,"6"},       // 
            {23,"7"},       // 
            {24,"8"},       // 
            {25,"9"},       // 
            {26,":"},       // 
            {27,";"},       // 
            {28,"<"},       // 
            {29,"="},       // 
            {30,">"},       // 
            {31,"?"},       // 
            {32,"@"},       // 
            {33,"A"},       // 
            {34,"B"},       // 
            {35,"C"},       // 
            {36,"D"},       // 
            {37,"E"},       // 
            {38,"F"},       // 
            {39,"G"},       // 
            {40,"H"},       // 
            {41,"I"},       // 
            {42,"J"},       // 
            {43,"K"},       // 
            {44,"L"},       // 
            {45,"M"},       // 
            {46,"N"},       // 
            {47,"O"},       // 
            {48,"P"},       // 
            {49,"Q"},       // 
            {50,"R"},       // 
            {51,"S"},       // 
            {52,"T"},       // 
            {53,"U"},       // 
            {54,"V"},       // 
            {55,"W"},       // 
            {56,"X"},       // 
            {57,"Y"},       // 
            {58,"Z"},       // 
            {59,"["},       // 
            {60,"\\"},      // 
            {61,"]"},       // 
            {62,"^"},       // 
            {63,"_"},       // 
            {64,"`"},       // 
            {65,"a"},       // 
            {66,"b"},       // 
            {67,"c"},       // 
            {68,"d"},       // 
            {69,"e"},       // 
            {70,"f"},       // 
            {71,"g"},       // 
            {72,"h"},       // 
            {73,"i"},       // 
            {74,"j"},       // 
            {75,"k"},       // 
            {76,"l"},       // 
            {77,"m"},       // 
            {78,"n"},       // 
            {79,"o"},       // 
            {80,"p"},       // 
            {81,"q"},       // 
            {82,"r"},       // 
            {83,"s"},       // 
            {84,"t"},       // 
            {85,"u"},       // 
            {86,"v"},       // 
            {87,"w"},       // 
            {88,"x"},       // 
            {89,"y"},       // 
            {90,"z"},       // 
            {91,"{"},       // 
            {92,"|"},       // 
            {93,"}"},       // 
            {94,"~"},       // 
            {95,"Ã"},       // DEL
            {96,"Ä"},       // FNC3
            {97,"Å"},       // FNC 2
            {98,"Æ"},       // SHIFT A
            {99,"Ç"},       // CODE C
            {100,"È"},      // FNC 4
            {101,"É"},      // CODE A
            {102,"Ê"},      // FNC 1
            {103,"Ë"},      // Start Code A
            {104,"Ì"},      // Start Code B
            {105,"Í"},      // Start Code C	
            {106,"Î"},      // Stop
        };
        private eCodeSet _currentCodeSet;
        public eCodeSet CodeSet
        {
            get { return _currentCodeSet; }
        }
        #endregion

        public string StartChar { get; private set; }
        public string Data { get; private set; } = "";
        public string Checksum { get; private set; }
        public string StopChar { get; private set; }

        public Code128(string text)
        {
            if(String.IsNullOrEmpty(text))
                return;

            // turn the string into ascii byte data
            var asciiBytes = Encoding.ASCII.GetBytes(text);

            // decide which codeset to start with
            var csa1 = asciiBytes.Length > 0
                           ? CodesetAllowedForChar(asciiBytes[0])
                           : CodeSetAllowed.CodeAorB;
            var csa2 = asciiBytes.Length > 1
                           ? CodesetAllowedForChar(asciiBytes[1])
                           : CodeSetAllowed.CodeAorB;
            _currentCodeSet = GetBestStartSet(csa1, csa2);

            // set up the beginning of the barcode
            // assume no codeset changes, account for start, checksum, and stop
            var codes = new ArrayList(asciiBytes.Length + 3);
            var startCode = StartCodeForCodeSet(_currentCodeSet);
            codes.Add(startCode);
            this.StartChar = GetCode(startCode);

            // add the codes for each character in the string
            for (var i = 0; i < asciiBytes.Length; i++)
            {
                int thischar = asciiBytes[i];
                var nextchar = asciiBytes.Length > i + 1 ? asciiBytes[i + 1] : -1;

                var charCodes = CodesForChar(thischar, nextchar, ref _currentCodeSet);
                codes.AddRange(charCodes);     
                foreach(int c in charCodes)
                {
                    Data += GetCode(c);
                }         
            }

            // calculate the check digit
            var checksum = (int)codes[0];
            for (var i = 1; i < codes.Count; i++)
            {
                checksum += i * (int)codes[i];
            }

            codes.Add(checksum % 103);
            this.Checksum = GetCode(checksum % 103);

            codes.Add(CStop);
            this.StopChar = GetCode(CStop);

            var result = codes.ToArray(typeof(int)) as int[];
        }

        public override string ToString()
        {
            return $@"{StartChar}{Data}{Checksum}{StopChar}";
        }

        public eCodeSet GetBestStartSet(CodeSetAllowed csa1, CodeSetAllowed csa2)
        {
            var vote = 0;

            vote += csa1 == CodeSetAllowed.CodeA ? 1 : 0;
            vote += csa1 == CodeSetAllowed.CodeB ? -1 : 0;
            vote += csa2 == CodeSetAllowed.CodeA ? 1 : 0;
            vote += csa2 == CodeSetAllowed.CodeB ? -1 : 0;

            return vote > 0 ? eCodeSet.CodeA : eCodeSet.CodeB; // ties go to codeB due to my own prejudices
        }

        public string GetCode(int ascii)
        {
            return dict[ascii];
        }

        /// <summary>
        /// Get the Code128 code value(s) to represent an ASCII character, with 
        /// optional look-ahead for length optimization
        /// </summary>
        /// <param name="charAscii">The ASCII value of the character to translate</param>
        /// <param name="lookAheadAscii">The next character in sequence (or -1 if none)</param>
        /// <param name="currentCodeSet">The current codeset, that the returned codes need to follow;
        /// if the returned codes change that, then this value will be changed to reflect it</param>
        /// <returns>An array of integers representing the codes that need to be output to produce the 
        /// given character</returns>
        public int[] CodesForChar(int charAscii, int lookAheadAscii, ref eCodeSet currentCodeSet)
        {
            int[] result;
            var shifter = -1;

            if (!CharCompatibleWithCodeset(charAscii, currentCodeSet))
            {
                // if we have a lookahead character AND if the next character is ALSO not compatible
                if ((lookAheadAscii != -1) && !CharCompatibleWithCodeset(lookAheadAscii, currentCodeSet))
                {
                    // we need to switch code sets
                    switch (currentCodeSet)
                    {
                        case eCodeSet.CodeA:
                            shifter = CCodeB;
                            currentCodeSet = eCodeSet.CodeB;
                            break;
                        case eCodeSet.CodeB:
                            shifter = CCodeA;
                            currentCodeSet = eCodeSet.CodeA;
                            break;
                    }
                }
                else
                {
                    // no need to switch code sets, a temporary SHIFT will suffice
                    shifter = CShift;
                }
            }

            if (shifter != -1)
            {
                result = new int[2];
                result[0] = shifter;
                result[1] = CodeValueForChar(charAscii);
            }
            else
            {
                result = new int[1];
                result[0] = CodeValueForChar(charAscii);
            }

            return result;
        }

        /// <summary>
        /// Tells us which codesets a given character value is allowed in
        /// </summary>
        /// <param name="charAscii">ASCII value of character to look at</param>
        /// <returns>Which codeset(s) can be used to represent this character</returns>
        public CodeSetAllowed CodesetAllowedForChar(int charAscii)
        {
            if (charAscii >= 32 && charAscii <= 95)
            {
                return CodeSetAllowed.CodeAorB;
            }
            else
            {
                return charAscii < 32 ? CodeSetAllowed.CodeA : CodeSetAllowed.CodeB;
            }
        }

        /// <summary>
        /// Determine if a character can be represented in a given codeset
        /// </summary>
        /// <param name="charAscii">character to check for</param>
        /// <param name="currentCodeSet">codeset context to test</param>
        /// <returns>true if the codeset contains a representation for the ASCII character</returns>
        public bool CharCompatibleWithCodeset(int charAscii, eCodeSet currentCodeSet)
        {
            var csa = CodesetAllowedForChar(charAscii);
            return csa == CodeSetAllowed.CodeAorB || (csa == CodeSetAllowed.CodeA && currentCodeSet == eCodeSet.CodeA)
                   || (csa == CodeSetAllowed.CodeB && currentCodeSet == eCodeSet.CodeB);
        }

        /// <summary>
        /// Gets the integer code128 code value for a character (assuming the appropriate code set)
        /// </summary>
        /// <param name="charAscii">character to convert</param>
        /// <returns>code128 symbol value for the character</returns>
        public int CodeValueForChar(int charAscii)
        {
            return charAscii >= 32 ? charAscii - 32 : charAscii + 64;
        }

        /// <summary>
        /// Return the appropriate START code depending on the codeset we want to be in
        /// </summary>
        /// <param name="cs">The codeset you want to start in</param>
        /// <returns>The code128 code to start a barcode in that codeset</returns>
        public int StartCodeForCodeSet(eCodeSet cs)
        {
            return cs == eCodeSet.CodeA ? CStartA : CStartB;
        }
    }
}
