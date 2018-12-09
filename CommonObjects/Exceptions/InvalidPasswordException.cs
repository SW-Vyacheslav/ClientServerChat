using System;

namespace CommonObjects.Exceptions
{
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(String message = "Invalid password.") : base(message) { }
    }
}
