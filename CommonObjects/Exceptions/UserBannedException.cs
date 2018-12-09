using System;

namespace CommonObjects.Exceptions
{
    public class UserBannedException : Exception
    {
        public UserBannedException(String message = "User is banned") : base(message) { }
    }
}
