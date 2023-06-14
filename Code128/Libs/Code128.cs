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
    /// ref.
    /// https://en.wikipedia.org/wiki/Code_128
    /// https://github.com/graphicore/librebarcode/blob/master/app/lib/code128Encoder/encoder.mjs
    /// https://www.barcodefaq.com/1d/code-128/#Code-128CharacterSet
    /// </summary>
    public class Code128
    {
        #region common

        public enum eCodeSet
        {
            CodeA,
            CodeB,
            CodeC
        }

        private const int CODE_SHIFT = 98;
        private const int CODE_SHIFT_C = 99;
        private const int CODE_SHIFT_B = 100;
        private const int CODE_SHIFT_A = 101;
        private const int CODE_START_A = 103;
        private const int CODE_START_B = 104;
        private const int CODE_START_C = 105;
        private const int CODE_STOP = 106;

       
        public string GetAllCharacter()
        {
            var charList = DictCharacter.Values.ToList();
            return String.Join("", charList);
        }

        private Dictionary<int, string> DictCharacter = new Dictionary<int, string>()
        {
            {0,"Ï"},        // space
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
            {100,"È"},      // CODE B
            {101,"É"},      // CODE A
            {102,"Ê"},      // FNC 1
            {103,"Ë"},      // Start Code A
            {104,"Ì"},      // Start Code B
            {105,"Í"},      // Start Code C	
            {106,"Î"},      // Stop
        };

        private string GetCharacter(int code)
        {
            return DictCharacter[code];
        }

        private int GetCode(string character)
        {
            return DictCode[character];
        }

        Dictionary<string, int> _DictCode;
        private Dictionary<string, int> DictCode
        {
            get
            {
                if (_DictCode == null)
                {
                    _DictCode = new Dictionary<string, int>();
                    foreach (var item in DictCharacter)
                    {
                        _DictCode.Add(item.Value, item.Key);
                    }
                }
                return _DictCode;
            }
        }
        
        #endregion

        //public string StartChar { get; private set; }
        //public string Data { get; private set; } = "";
        //public string Checksum { get; private set; }
        //public string StopChar { get; private set; }

        string _text;
        public Code128(string text = null)
        {
            _text = text;
        }

        public void SetText(string text)
        {
            _text = text;
        }

        public string Encode(string text)
        {
            _text = text;

            if (String.IsNullOrEmpty(text))
                return null;

            string result = "";

            // turn the string into ascii byte data
            var asciiBytes = Encoding.ASCII.GetBytes(text);

            //// decide which codeset to start with
            //var csa1 = asciiBytes.Length > 0
            //               ? CodesetAllowedForChar(asciiBytes[0])
            //               : CodeSetAllowed.CodeAorB;
            //var csa2 = asciiBytes.Length > 1
            //               ? CodesetAllowedForChar(asciiBytes[1])
            //               : CodeSetAllowed.CodeAorB;
            //_currentCodeSet = GetBestStartSet(csa1, csa2);


            var currentCodeSet = GetCodeSet(text);

            // get start char
            string charStart;
            switch (currentCodeSet)
            {
                case eCodeSet.CodeA:
                    charStart = GetCharacter(CODE_START_A);
                    break;
                case eCodeSet.CodeB:
                    charStart = GetCharacter(CODE_START_B);
                    break;
                default:
                    charStart = GetCharacter(CODE_START_C);
                    break;
            }
            result = charStart;

            // loop all input character
            for (var i = 0; i < text.Length; i++)
            {
                if (currentCodeSet == eCodeSet.CodeC)
	            {
                    var nextTwoCharacter = text.Substring(i,2);
                    var code = Convert.ToInt32(nextTwoCharacter);                    
                    var character = GetCharacter(code);
                    result += character;
                    i++; //skip next character

                }
	            else
	            {
                    result += text[i];
                }

                var remainCharacter = text.Substring(i + 1);
                var nextCodeSet = GetCodeSet(remainCharacter);
                if (nextCodeSet != currentCodeSet)
                {
                    result += GetSetShiftCharacter(nextCodeSet);
                    currentCodeSet = nextCodeSet;
                }
            }

            // check sum
            string charSum = GetSumCharacter(result);
            result += charSum;

            // stop
            string charStop = GetString(CODE_STOP);
            result += charStop;

            return result;

        }

        private string GetSetShiftCharacter(eCodeSet nextCodeSet)
        {
            switch (nextCodeSet)
            {
                case eCodeSet.CodeA:
                    return GetCharacter(CODE_SHIFT_A);
                case eCodeSet.CodeB:
                    return GetCharacter(CODE_SHIFT_B);
                default:
                    return GetCharacter(CODE_SHIFT_C);
            }
        }

        private eCodeSet GetCodeSet(string text)
        {
            if (text.Length > 2 && IsFirstTwoCharacterIsNumber(text))
                return eCodeSet.CodeC;

            // text contain lower case
            else if (text.Any(char.IsLower))
                return eCodeSet.CodeB;

            else
                return eCodeSet.CodeA;
        }

        private bool IsContainLowerCase(string text)
        {
            throw new NotImplementedException();
        }

        private bool IsFirstTwoCharacterIsNumber(string text)
        {
            try
            {
                if (!IsNumber(text[0]))
                    return false;

                if (!IsNumber(text[1]))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                // text is less than 2 character
                return false;
            }
        }

        private bool IsNumber(char c)
        {
            int val;
            return int.TryParse(c.ToString(), out val);
        }
        
        private int CalculateCheckSum(ArrayList codes)
        {
            // calculate the check digit
            var checksum = (int)codes[0];
            for (var i = 1; i < codes.Count; i++)
            {
                checksum += i * (int)codes[i];
            }

            return checksum % 103;
        }

        public override string ToString()
        {
            return Encode(_text);
        }

        public string GetSumCharacter(string barcode)
        {
            var codes = new ArrayList();
            foreach (var c in barcode)
            {
                var val = DictCode[c.ToString()];
                codes.Add(val);
            }

            var checksum = CalculateCheckSum(codes);
            var sumChar = GetString(checksum);
            return sumChar;
        }        

        public string GetString(int ascii)
        {
            return DictCharacter[ascii];
        }

    }
}
