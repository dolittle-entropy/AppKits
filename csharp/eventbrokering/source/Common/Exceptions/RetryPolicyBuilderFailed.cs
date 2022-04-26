using System;
using System.Runtime.Serialization;

namespace Common.Exceptions
{
    [Serializable]
    public class RetryPolicyBuilderFailed : Exception
    {
        public RetryPolicyBuilderFailed()
        {
        }

        public RetryPolicyBuilderFailed(string? message) : base(message)
        {
        }

        public RetryPolicyBuilderFailed(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RetryPolicyBuilderFailed(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}