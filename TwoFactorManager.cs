using CollaborativeSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaborativeSoftware
{
    public static class TwoFactorManager
    {
        private static string _currentCode;
        private static DateTime _expiry;

        public static string GenerateCode()
        {
            Random rnd = new Random();
            _currentCode = rnd.Next(100000, 999999).ToString();
            _expiry = DateTime.Now.AddMinutes(5);
            return _currentCode;
        }

        public static bool ValidateCode(string code)
        {
            if (DateTime.Now > _expiry)
                return false;

            return code == _currentCode;
        }
    }
}
