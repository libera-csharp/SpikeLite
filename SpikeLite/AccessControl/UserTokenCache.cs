/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using Sharkbite.Irc;
using System.Threading;

namespace SpikeLite.AccessControl
{
    //TODO: Handle bot parting / quitting
    public class UserTokenCache
    {
        private readonly Connection _client;
        private readonly Dictionary<string, UserToken> _tokens;
        private readonly ReadWriteLock _mutex;

        public UserTokenCache(Connection client)
        {
            _client = client;
            _tokens = new Dictionary<string,UserToken>();
            _mutex = new ReadWriteLock();
        }

        public Connection client
        {
            get { return _client; }
        }

        #region Token Handling

        public UserToken RetrieveToken(UserInfo user)
        {
            UserToken token;

            using (_mutex.AquireReadLock())
            {
                token = CreateUserToken(user);
            }

            return token;
        }

        public void CacheToken(string nick, UserToken token)
        {
            using ( _mutex.AquireWriteLock() )
            {
                _tokens[nick] = token;
            }
        }

        public void InvalidateToken(UserInfo user)
        {
            InvalidateToken(user.Nick);
        }

        public void InvalidateToken(string nick)
        {
            using ( _mutex.AquireWriteLock() )
            {
                _tokens.Remove(nick);
            }
        }

        private static UserToken CreateUserToken(UserInfo user)
        {
            return new IrcUserToken(user.Nick, user.Hostname);
        }

        #endregion 

        #region Write Locking

        private class ReadWriteLock
        {
            private readonly ReaderWriterLock _mutex;
            private readonly int _readTimeout;
            private readonly int _writeTimeout;

            public ReadWriteLock() : this(5000, 5000) { }

            public ReadWriteLock(int readTimeout, int writeTimeout)
            {
                _mutex = new ReaderWriterLock();
                _readTimeout = readTimeout;
                _writeTimeout = writeTimeout;
            }

            public IDisposable AquireReadLock()
            {
                return new ReadLockCookie(_mutex, _readTimeout);
            }

            public IDisposable AquireWriteLock()
            {
                return new WriteLatch(_mutex, _writeTimeout);
            }

            private class ReadLockCookie : IDisposable
            {
                private readonly ReaderWriterLock _mutex;

                public ReadLockCookie( ReaderWriterLock lck, int timeout )
                {
                    _mutex = lck;
                    _mutex.AcquireReaderLock(timeout);
                }

                public void Dispose()
                {
                    _mutex.ReleaseReaderLock();
                }
            }

            #region Write Latching

            private class WriteLatch : IDisposable
            {
                private readonly ReaderWriterLock _mutex;
                LockCookie _cookie;
                private readonly bool _upgraded;

                public WriteLatch(ReaderWriterLock lck, int timeout)
                {
                    _mutex = lck;

                    if (_mutex.IsReaderLockHeld)
                    {
                        _upgraded = true;
                        _cookie = lck.UpgradeToWriterLock(timeout);
                    }
                    else
                    {
                        _upgraded = false;
                        lck.AcquireWriterLock(timeout);
                    }
                }

                public void Dispose()
                {
                    if ( _upgraded )
                    {
                        _mutex.DowngradeFromWriterLock(ref _cookie);
                    }
                    else
                    {
                        _mutex.ReleaseWriterLock();
                    }
                }
            }

            #endregion 
        }

        #endregion 
    }
}
