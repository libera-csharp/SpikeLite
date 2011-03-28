/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
namespace SpikeLite.AccessControl
{
    public class IrcUserToken : IUserToken
    {
        private readonly string _nick;
        private readonly string _hostMask;

        public IrcUserToken(string nick, string hostMast)
        {
            _nick = nick;
            _hostMask = hostMast;
        }

        public string DisplayName
        {
            get { return _nick; }
        }

        public string Nick
        {
            get { return _nick; }
        }

        public string HostMask
        {
            get { return _hostMask; }
        }
    }
}
