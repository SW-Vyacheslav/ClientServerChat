using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using CommonObjects.Helpers;
using CommonObjects.Models;

namespace Client.ViewModel
{
    public class StartViewModel : ObservableObject, IDataErrorInfo
    {
        private const String IPAddressRegEx = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        private const String UserNameRegEx = @"^[a-zA-Z0-9_-]{3,16}$";
        private const String PasswordRegEx = @"^[a-zA-Z0-9_-]{6,18}$";

        public String IPAddress { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }

        public DelegateCommand SignInCommand { get; set; }
        public DelegateCommand SignUpCommand { get; set; }

        public Dictionary<String, String> ErrorCollection { get; private set; }
        public string Error { get { return null; } }
        public string this[string propName]
        {
            get
            {
                String result = null;

                switch (propName)
                {
                    case "IPAddress":
                        {
                            if (String.IsNullOrWhiteSpace(IPAddress)) result = "IPAddress can't be empty.";
                            else
                            {
                                Regex ipAddrRegEx = new Regex(IPAddressRegEx);
                                if (!(ipAddrRegEx.Match(IPAddress).Success)) result = "Example: 127.0.0.1";
                            }

                            break;
                        }

                    case "UserName":
                        {
                            if (String.IsNullOrWhiteSpace(UserName)) result = "User name can't be empty";
                            else if (UserName.Length < 3) result = "Min length is 3";
                            else if (UserName.Length > 16) result = "Max length is 16";
                            else
                            {
                                Regex userNameRegEx = new Regex(UserNameRegEx);
                                if (!(userNameRegEx.Match(UserName).Success)) result = "User name can contain only Uppercase, Lowcase english symbols, digits, underscore and dash";
                            }
                            
                            break;
                        }

                    case "Password":
                        {
                            if (String.IsNullOrWhiteSpace(Password)) result = "Password can't be empty";
                            else if (Password.Length < 6) result = "Min length is 6";
                            else if (Password.Length > 18) result = "Max length is 18";
                            else
                            {
                                Regex pswdRegEx = new Regex(Password);
                                if (!(pswdRegEx.Match(Password).Success)) result = "User name can contain only Uppercase, Lowcase english symbols, digits, underscore and dash";
                            }

                            break;
                        }

                    default:
                        break;
                }

                if (ErrorCollection.ContainsKey(propName)) ErrorCollection[propName] = result;
                else if (result != null) ErrorCollection.Add(propName,result);

                RaisePropertyChangedEvent("ErrorCollection");
                return result;
            }
        }
        private Boolean IsValidData()
        {
            for (int i = 0; i < ErrorCollection.Count; i++)
            {
                if (ErrorCollection.Values.ElementAt(i) != null) return false;
            }

            return true;
        }

        public StartViewModel()
        {
            ErrorCollection = new Dictionary<String, String>();
            SignInCommand = new DelegateCommand(o => SignIn(),o => IsValidData());
            SignUpCommand = new DelegateCommand(o => SignUp(),o => IsValidData());
            IPAddress = MainWindowViewModel.SettingsManager.Settings.ServerIPAddress;
        }

        private void SignIn()
        {
            MainWindowViewModel.SettingsManager.Settings.ServerIPAddress = IPAddress;
            MainWindowViewModel.SettingsManager.ApplySettings();
            MainWindowViewModel.Connect();
            if (!MainWindowViewModel.IsConnected) return;
            AuthRequest authRequest = new AuthRequest(UserName,Password);
            DataTransferHelper.SendRequestToServer(MainWindowViewModel.Client, authRequest);
        }

        private void SignUp()
        {
            MainWindowViewModel.SettingsManager.Settings.ServerIPAddress = IPAddress;
            MainWindowViewModel.SettingsManager.ApplySettings();
            MainWindowViewModel.Connect();
            if (!MainWindowViewModel.IsConnected) return;
            RegistrationRequest registrationRequest = new RegistrationRequest(UserName,Password);
            DataTransferHelper.SendRequestToServer(MainWindowViewModel.Client, registrationRequest);
        }
    }
}
