using System;

namespace Nuages.TaskRunner
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException(string? message) : base(message)
        {
            
        }
    }
}