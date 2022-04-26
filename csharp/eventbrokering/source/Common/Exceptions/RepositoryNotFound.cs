using System;
using System.Runtime.Serialization;

namespace Common.Exceptions
{
    [Serializable]
    internal class RepositoryNotFound : Exception
    {
        public RepositoryNotFound()
        {
        }

        public RepositoryNotFound(string? message) : base(message)
        {
        }

        public RepositoryNotFound(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RepositoryNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}