using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using CommonObjects.Helpers;
using CommonObjects.Models;

namespace Client.ViewModel
{
    public class ChatViewModel : ObservableObject, IDataErrorInfo
    {
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public String MessageText { get; set; }

        public DelegateCommand SendMessageCommand { get; set; }
        public DelegateCommand SignOutCommand { get; set; }

        public Boolean HasMessageDataError { get; private set; }

        public string Error { get { return null; } }

        public string this[string propName]
        {
            get
            {
                String result = null;

                switch (propName)
                {
                    case "MessageText":
                        {
                            if (String.IsNullOrWhiteSpace(MessageText)) result = "Message can't be empty";
                            break;
                        }

                    default:
                        break;
                }

                if(result == null) HasMessageDataError = false;
                else HasMessageDataError = true;

                return result;
            }
        }

        public ChatViewModel()
        {
            Messages = new ObservableCollection<Message>();
            Users = new ObservableCollection<User>();
            SendMessageCommand = new DelegateCommand(o=>SendMessage(), o=>!HasMessageDataError);
            SignOutCommand = new DelegateCommand(o=>SignOut());
            HasMessageDataError = false;
        }

        private void SendMessage()
        {
            Message message = new Message(MainWindowViewModel.ClientUser,MessageText);
            MessageRequest messageRequest = new MessageRequest(message);
            DataTransferHelper.SendRequestToServer(MainWindowViewModel.Client,messageRequest);
        }

        private void SignOut()
        {
            SignOutRequest signOutRequest = new SignOutRequest();
            DataTransferHelper.SendRequestToServer(MainWindowViewModel.Client,signOutRequest);
        }
    }
}
