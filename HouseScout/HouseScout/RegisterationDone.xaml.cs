using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Facebook;
using System.IO.IsolatedStorage;

namespace HouseScout
{
    public partial class RegisterationDone : PhoneApplicationPage
    {
        private const string FBApi = "534336869990822";
        private FacebookClient client;

        public RegisterationDone()
        {
            InitializeComponent();
            SystemTray.SetIsVisible(this, false);
            client = new FacebookClient();
            client.PostCompleted += (o, args) =>
            {
                //Checking for errors
                if (args.Error != null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(args.Error.Message));
                }
                else
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Message posted successfully"));
                }
            };

            //Checking for saved token
            if (GetToken() != null)
                client.AccessToken = GetToken();
        }

        private void PostClicked(object sender, RoutedEventArgs e)
        {
            //Client Parameters
            var parameters = new Dictionary<string, object>();
            parameters["client_id"] = FBApi;
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";
            //The scope is what give us the access to the users data, in this case
            //we just want to publish on his wall
            parameters["scope"] = "publish_stream";
            Browser.Visibility = System.Windows.Visibility.Visible;
            Browser.Navigate(client.GetLoginUrl(parameters));

            SubtitleBar.Text = "LOG IN";
            TitleBar.Text = "FACEBOOK";
        }

        private void BrowserNavitaged(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            //Making sure that the url actually has the access token
            if (!client.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
            {
                return;
            }
            //Checking that the user successfully accepted our app, otherwise just show the error
            if (oauthResult.IsSuccess)
            {
                //Process result
                client.AccessToken = oauthResult.AccessToken;
                //Hide the browser
                Browser.Visibility = System.Windows.Visibility.Collapsed;
                PostToWall();
            }
            else
            {
                //Process Error
                MessageBox.Show(oauthResult.ErrorDescription);
                Browser.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void PostToWall()
        {
            var parameters = new Dictionary<string, object>();
            parameters["message"] = WallPost.Text;
            client.PostAsync("me/feed", parameters);
        }

        private void SaveToken(String token)
        {
            //If it is the first save, create the key on ApplicationSettings and save the token, otherwise just modify the key
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("token"))
                IsolatedStorageSettings.ApplicationSettings.Add("token", token);
            else
                IsolatedStorageSettings.ApplicationSettings["token"] = token;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private string GetToken()
        {
            //If there's no Token on memory, just return null, otherwise return the token as string
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("token"))
                return null;
            else
                return IsolatedStorageSettings.ApplicationSettings["token"] as string;
        }

        private void SkipPost_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml",UriKind.Relative));
        }

    }
}