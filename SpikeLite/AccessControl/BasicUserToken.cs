/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Security;

namespace SpikeLite.AccessControl
{
    public class BasicUserToken : UserToken
    {
        private string _username;
        private string _password;
        private string _displayName;

        public BasicUserToken( string username, string password ) : this(username, password, username) { }

        public BasicUserToken( string username, string password, string displayName )
        {
            _username = username;
            _password = password;
            _displayName = displayName;
        }

        public string Username
        {
            get { return _username; }
        }

        public string Password
        {
            get { return _password; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }
    }
}
