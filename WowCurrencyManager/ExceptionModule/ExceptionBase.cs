using System;

namespace WowCurrencyManager.ExceptionModule
{
    [Serializable]
    public class ExceptionBase : Exception
    {
        public ExceptionType Range { get; private set; }

        public ExceptionBase() { }

        public ExceptionBase(string message)
        : base(message)
        {
            Range = ExceptionType.Default;
        }
        public ExceptionBase(string message, ExceptionType range)
        : base(message)
        {
            Range = range;
        }

        public ExceptionBase(System.Exception ex, ExceptionType range) : base(ex.Message)
        {
            Range = range;
        }
    }
}
