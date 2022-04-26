using System;

namespace Common.Exceptions
{
    [Serializable]
    public class ValidationFailed : Exception
    {
        public ValidationFailed(string message) : base(message)
        {
        }
    }
}
