using System;
using System.Collections.ObjectModel;

using CommonObjects.Models;
using CommonObjects.Helpers;

namespace Server.Objects
{
    public class UserManager
    {
        public ObservableCollection<User> Users { get; private set; }
        public ObservableCollection<User> BannedUsers { get; set; }

        public UserManager()
        {
            Users = new ObservableCollection<User>();
            BannedUsers = new ObservableCollection<User>();
        }

        public Boolean IsConnectedUser(String user_id)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (user_id == Users[i].ID)
                {
                    return true;
                }
            }
            return false;
        }
        public void RemoveUserByID(String user_id)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (user_id == Users[i].ID)
                {
                    InvokeHelper.ApplicationInvoke(() => Users.RemoveAt(i));
                    break;
                }
            }
        }
        public User GetUserByID(String user_id)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (user_id == Users[i].ID)
                {
                    return Users[i];
                }
            }
            return null;
        }
        public void AddUser(User user)
        {
            InvokeHelper.ApplicationInvoke(() => Users.Add(user));
        }

        public Boolean IsBannedUser(String user_id)
        {
            for (int i = 0; i < BannedUsers.Count; i++)
            {
                if (user_id == BannedUsers[i].ID)
                {
                    return true;
                }
            }
            return false;
        }
        public void RemoveBannedUserByID(String user_id)
        {
            for (int i = 0; i < BannedUsers.Count; i++)
            {
                if (user_id == BannedUsers[i].ID)
                {
                    InvokeHelper.ApplicationInvoke(() => BannedUsers.RemoveAt(i));
                    break;
                }
            }
        }
        public User GetBannedUserByID(String user_id)
        {
            for (int i = 0; i < BannedUsers.Count; i++)
            {
                if (user_id == BannedUsers[i].ID)
                {
                    return BannedUsers[i];
                }
            }
            return null;
        }
        public void AddBannedUser(User user)
        {
            InvokeHelper.ApplicationInvoke(() => BannedUsers.Add(user));
        }
    }
}