using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks
{
    static partial class StringEx
    {
        #region  basic string methods;
 


            

        #endregion


        #region BeginWith

        /**
        // 
        */
        public static bool BeginWith(this string s, char c)
        {
            return s[0] == c;
        }

        public static bool BeginWithAny(this string s, IEnumerable<char> chars)
        {
            return chars.Contains(s[0]);
        }

        public static bool BeginWithAny(this string s, params char[] chars)
            => s.BeginWithAny(chars.AsEnumerable());



        #endregion
    }
}
