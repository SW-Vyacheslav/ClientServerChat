using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonObjects.Helpers;
using CommonObjects.Models;

namespace Client.ViewModel
{
    public class ChatViewModel : ObservableObject
    {
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public String MessageText { get; set; }

        public DelegateCommand SendMessageCommand { get; set; }

        public ChatViewModel()
        {
            Messages = new ObservableCollection<Message>();
            Users = new ObservableCollection<User>();
        }

        private void SendMessage()
        {

        }
    }
}
