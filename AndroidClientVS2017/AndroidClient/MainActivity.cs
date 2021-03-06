﻿using Android.App;
using Android.Widget;
using Android.OS;
using System;
using IdentityModel.OidcClient;
using System.Net.Http;

namespace AndroidClient
{
    [Activity(Label = "AndroidClient", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private TextView _output;
        private LoginResult _result;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var loginButton = FindViewById<Button>(Resource.Id.LoginButton);
            loginButton.Click += _loginButton_Click;

            var apiButton = FindViewById<Button>(Resource.Id.ApiButton);
            apiButton.Click += _apiButton_Click;

            _output = FindViewById<TextView>(Resource.Id.Output);
        }

        private async void _loginButton_Click(object sender, System.EventArgs e)
        {
            var authority = "https://demo.identityserver.io";

            try
            {
                var options = new OidcClientOptions
                {
                    Authority = authority,
                    ClientId = "native.hybrid",
                    Scope = "openid profile api",
                    RedirectUri = "io.identitymodel.native://callback",
                    ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                    Browser = new ChromeCustomTabsBrowser(this)
                };

                var oidcClient = new OidcClient(options);
                _result = await oidcClient.LoginAsync();

                if (_result.IsError)
                {
                    Log("Error:" + _result.Error, true);
                }
                else
                {
                    Log("Claims:", true);
                    foreach (var claim in _result.User.Claims)
                    {
                        Log($"   {claim.Type}:{claim.Value}");
                    }
                    Log("Access Token: " + _result.AccessToken);
                }
            }
            catch (Exception ex)
            {
                Log("Exception: " + ex.Message, true);
                Log(ex.ToString());
            }
        }

        private async void _apiButton_Click(object sender, EventArgs e)
        {
            if (_result?.IsError == false)
            {
                var apiUrl = "https://demo.identityserver.io/api/test";

                var client = new HttpClient();
                client.SetBearerToken(_result.AccessToken);

                try
                {
                    var result = await client.GetAsync(apiUrl);
                    if (result.IsSuccessStatusCode)
                    {
                        Log("API Results:", true);

                        var json = await result.Content.ReadAsStringAsync();
                        Log(json);
                    }
                    else
                    {
                        Log("API Error: " + (int)result.StatusCode, true);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception: " + ex.Message, true);
                    Log(ex.ToString());
                }
            }
            else
            {
                Log("Login to call API");
            }
        }

        public void Log(string msg, bool clear = false)
        {
            if (clear)
            {
                _output.Text = "";
            }
            else
            {
                _output.Text += "\r\n";
            }

            _output.Text += msg;
        }
    }
}

