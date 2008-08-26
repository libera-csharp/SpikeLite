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
using Sharkbite.Irc;
using System.Threading;

namespace SpikeLite.AccessControl
{
    //TODO: Handle bot parting / quitting
    public class UserTokenCache
    {
        private Connection _client;
        private Dictionary<string, UserToken> _tokens; //nick => user token
        private ReadWriteLock _mutex;

        public UserTokenCache(Connection client)
        {
            _client = client;
            _tokens = new Dictionary<string,UserToken>();
            _mutex = new ReadWriteLock();
        }

        private void AttachHandlers()
        {
            _client.Listener.OnNick += new NickEventHandler(Listener_OnNick);
            _client.Listener.OnPart += new PartEventHandler(Listener_OnPart);
            _client.Listener.OnQuit += new QuitEventHandler(Listener_OnQuit);
        }

        #region Event Handling

        private void Listener_OnNick(UserInfo user, string newNick)
        {
            string oldNick = user.Nick;
            UserToken token;

            using ( _mutex.AquireWriteLock())
            {
                _tokens.Remove(newNick);

                if ( _tokens.TryGetValue(oldNick, out token) )
                {
                    _tokens.Add(newNick, token);
                }
                _tokens.Remove(oldNick);
            }
        }

        private void Listener_OnPart(UserInfo user, string channel, string reason)
        {
            using (_mutex.AquireReadLock())
            {
                if (!IsStillVisible(user))
                {
                    InvalidateToken(user);
                }
            }
        }

        private void Listener_OnQuit( UserInfo user, string reason )
        {
            InvalidateToken(user);
        }

        #endregion 

        /// <summary>
        /// Checks if the user is still visible in any of the channels on this server that the bot is on.
        /// </summary>
        /// 
        /// <param name="user">The user to check</param>
        /// <returns>True if the user is visible to the bot in another channel, otherwise false</returns>
        private bool IsStillVisible(UserInfo user)
        {
            return IsStillVisible(user.Nick);
        }

        /// <summary>
        /// Checks if a nick is still visible in any of the channels on this server that the bot is on.
        /// </summary>
        /// 
        /// <param name="nick">The nick to check</param>
        /// <returns>True if the nick is visible to the bot in another channel, otherwise false</returns>
        private bool IsStillVisible(string nick)
        {
            //TODO: Find the user
            //for now err on the side of safety and assume the user is no longer visible
            return false;
        }

        #region Token Handling

        public UserToken RetrieveToken(UserInfo user)
        {
            string nick = user.Nick;
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

        private UserToken CreateUserToken(UserInfo user)
        {
            return new IrcUserToken(user.Nick, user.Hostname);
        }

        #endregion 

        #region Write Locking

        private class ReadWriteLock
        {
            private ReaderWriterLock _mutex;
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

            public IDisposable ReleaseLock()
            {
                return new LatchRelease(_mutex);
            }

            private class ReadLockCookie : IDisposable
            {
                private ReaderWriterLock _mutex;

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
                private ReaderWriterLock _mutex;
                LockCookie _cookie;
                private bool _upgraded;

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

            #region Latch Releasing

            private class LatchRelease : IDisposable
            {
                private ReaderWriterLock _mutex;
                LockCookie _cookie;
                private bool _released;

                public LatchRelease(ReaderWriterLock lck)
                {
                    _mutex = lck;

                    if (_mutex.IsReaderLockHeld || _mutex.IsWriterLockHeld)
                    {
                        _cookie = lck.ReleaseLock();
                        _released = true;
                    }
                    else
                    {
                        _released = false;
                    }
                }

                public void Dispose()
                {
                    if (_released)
                    {
                        _mutex.RestoreLock(ref _cookie);
                    }
                }
            }

            #endregion 
        }

        #endregion 
    }
}
