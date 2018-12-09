using System;

namespace CommonObjects.Exceptions
{
    public class InvalidUserNameException : Exception
    {
        public InvalidUserNameException(String message = "Invalid user name") : base(message) { }
    }
}
