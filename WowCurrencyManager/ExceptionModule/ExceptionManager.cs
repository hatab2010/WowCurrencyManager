using System;

namespace WowCurrencyManager.ExceptionModule
{
    public static class ExceptionManager
    {
        public static void Throw(ExceptionBase ex)
        {
            Console.WriteLine(ex.Message);
            throw ex;
        }
    }
}
