/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Threading;
using log4net;
using Sharkbite.Irc;
using SpikeLite.AccessControl;
using SpikeLite.Communications;
using SpikeLite.Modules;
using SpikeLite.Persistence;
using SpikeLite.Configuration;

namespace SpikeLite
{
    #region Enumerations

    /// <summary>
    /// Returns the current connection status of the bot.
    /// </summary>
    /// 
    /// <remarks>
    /// This needs to go, especially if we want multiple network support.
    /// </remarks>
    public enum BotStatus
    {
        /// <summary>
        /// The bot has just started up, and is most likely initializing stuff.
        /// </summary>
        Starting,
        
        /// <summary>
        /// The bot has finished initializing and is connected. 
        /// </summary>
        Started,
        
        /// <summary>
        /// The bot is disconnecting. 
        /// </summary>
        Stopping,
        
        /// <summary>
        /// The bot has finished stopping.
        /// </summary>
        Stopped
    }

    #endregion

    /// <summary>
    /// The heart of the channel bot. Wrap it in a UI and you're good to go.
    /// </summary>
    public class SpikeLite
    {
        #region Data Members

        /// <summary>
        /// Provides access to the Bot's configuration 
        /// </summary>
        private readonly SpikeLiteSection _configuration;

        /// <summary>
        /// Stores the bot's connection status. 
        /// </summary>
        private BotStatus _botStatus = BotStatus.Stopped;

        /// <summary>
        /// Holds an instance of a <see cref="Connection"/> object, useful for binding to events.
        /// </summary>
        private Connection _connection;

        /// <summary>
        /// Holds our connection arguments.
        /// </summary>
        /// 
        /// <remarks>
        /// This is partially a hack to get around the behavior described in the remarks for <see cref="Connect"/>.
        /// We'll need to fix this to make the bot multi-network.
        /// </remarks>
        private readonly ConnectionArgs _connectionArgs;

        /// <summary>
        /// This holds the instace of the logger we use for spamming whatever appender we so desire.
        /// </summary>
        private ILog _logger;

        #endregion

        #region Properties

        /// <summary>
        /// Gets our connection status.
        /// </summary>
        /// 
        /// <remarks>
        /// This will need to be refactored for multi-network support.
        /// </remarks>
        public bool IsConnected
        {
            get { return _connection != null && _connection.Connected; }
        }

        /// <summary>
        /// Gets the Log4NET logger that we're using.
        /// </summary>
        public ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger(typeof(SpikeLite));
                }

                return _logger;
            }
        }

        /// <summary>
        /// Gets or sets the authentication module that we're using. This is usually injected via our IoC container..
        /// </summary>
        public AuthenticationModule AuthenticationManager
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the communications manager we're using. This is usually injected via our IoC container.
        /// </summary>
        public CommunicationManager CommunicationManager
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets our module manager we're using. This is usually injected via our IoC container.
        /// </summary>
        public ModuleManager ModuleManager
        {
            get; set;
        }

        #endregion

        #region Construction

        // TODO: Kog 12/25/2008 - swap this configuration stuff to Spring.NET ASAP.

        /// <summary>
        /// Do some default configuration.
        /// </summary>
        public SpikeLite()
        {
            _configuration = SpikeLiteSection.GetSection();

            NetworkElement network = _configuration.Networks["FreeNode"];
            ServerElement server = network.Servers[0];
            
            _connectionArgs = new ConnectionArgs
            {
                Hostname = server.HostnameOrIP,
                Nick = network.BotNickname,
                Port = server.Port,
                RealName = network.BotRealName,
                UserName = network.BotUserName
            };
        }

        #endregion 

        #region Public Behavior

        /// <summary>
        /// Attempt to initialize our subsystems and spin up IO.
        /// </summary>
        public void Start()
        {
            // Make sure we're not trying to double start.
            if (_botStatus != BotStatus.Stopped)
            {
                throw new Exception(string.Format("Current BotStatus is : '{0}'. To start the bot the BotStatus must be 'Stopped'.", _botStatus));
            }

            _botStatus = BotStatus.Starting;

            // Attempt to connect.
            Connect();

            CommunicationManager.Connection = _connection;
            ModuleManager.LoadModules();

            // Alright, we're cooking now.
            _botStatus = BotStatus.Started;
        }

        // TODO: Kog 12/25/2008 - Do we really need this?

        /// <summary>
        /// Kill all our modules, stop all our IPC.
        /// </summary>    
        public void Shutdown()
        {
            _botStatus = BotStatus.Stopped;
        }

        /// <summary>
        /// Disconnect from our networks. 
        /// </summary>
        /// 
        /// <param name="quitMessage">Quit message to send to IRCD. Cannot be <b>null</b> or <b>empty</b>.</param>
        /// 
        /// <remarks>
        /// This will perform a no-op if you are not already connected.
        /// </remarks>
        public void Quit(string quitMessage)
        {
            if (IsConnected)
            {
                _connection.Disconnect(quitMessage);
            }
        }

        /// <summary>
        /// Attempt to connect to our pre-configured network.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This will need to be refactored to allow multi-network support. This will perform a no-op
        /// if you are already connected.
        /// </para>
        ///
        /// <para>
        /// Unfortunately there's a rather annoying bug with Thresher's IRC library: when you re-use the same
        /// connection object, it maintains a cache of capability options, as sent as part of the registration
        /// preamble from the IRCD. There's no way (so far) to flush this cache, and it's not a very intelligent
        /// cache - it doesn't look for hits, only misses - so we wind up trying to cache something twice and
        /// an exception resulting from said behavior.
        /// </para>
        /// 
        /// <para>
        /// So... In order to support a reconnect ability you need to create a NEW connection object. This requires
        /// unsubscribing your old event handlers (to prevent memory leaks), and re-registering them. Painful duplication,
        /// crappy mixing of responsibility... name your reason why this is bad. Unfortunately there's not much choice.
        /// </para>
        /// 
        /// </remarks>
        public void Connect()
        {
            if (!IsConnected)
            {
                _connection = new Connection(_connectionArgs, true, true);

                // Subscribe our events
                _connection.Listener.OnRegistered += OnRegister;
                _connection.Listener.OnPrivateNotice += OnPrivateNotice;
                _connection.OnRawMessageReceived += OnRawMessageReceived;
                _connection.OnRawMessageSent += OnRawMessageSent;

                _connection.Connect();

                // Make sure we don't get any random disconnected events before we're connected
                _connection.Listener.OnDisconnected += Listener_OnDisconnect;
            }
        }
 
        #endregion 

        #region Events

        /// <summary>
        /// We're sending a message. 
        /// </summary>
        /// 
        /// <param name="message">The text of the message sent to the IRCD.</param>
        /// 
        /// <remarks>
        /// The message text will be unformatted, as sent by the IRCD. May be RFC1459 compliant. 
        /// </remarks>
        private void OnRawMessageSent(string message)
        {
            Logger.DebugFormat("Sent: {0}", message);
        }

        /// <summary>
        /// The IRCD has sent us a message that isn't a PRIVMSG or NOTICE.
        /// </summary>
        /// 
        /// <param name="message">The text of the message sent by the IRCD.</param>
        /// 
        /// <remarks>
        /// The message text will be unformatted, as sent by the IRCD. May be RFC1459 compliant. 
        /// </remarks>
        private void OnRawMessageReceived(string message)
        {
            Logger.DebugFormat("Received: {0}", message);
        }

        /// <summary>
        /// The IRCD has notified us that "registration" is complete, and the MOTD has finished being sent.
        /// </summary>
        /// 
        /// <remarks>
        /// This is a good place to send your services registration. Some networks provide cloaks for users that are best
        /// set up upon connect. Consider this an analog to mIRC's onConnect. The bot currently blocks on a response, see
        /// Listener_OnPrivateNotice.
        /// </remarks>
        private void OnRegister()
        {
            _connection.Sender.PrivateMessage("nickserv", String.Format("identify {0}", _configuration.Networks["FreeNode"].NickServPassword));
        }

        /// <summary>
        /// We've recieved a NOTICE from the server.
        /// </summary>
        /// 
        /// <param name="user">A user representing the sender.</param>
        /// <param name="notice">The message being NOTICE'd.</param>
        /// 
        /// <remarks>
        /// We currently block upon connect between registration and receiving an authentication response. This is so that the
        /// real hostmask of the bot will not be revealed inadvertently. 
        /// </remarks>
        private void OnPrivateNotice(UserInfo user, string notice)
        {
            // Make sure the message is coming from NickServ
            if (user.Nick.Equals("nickserv", StringComparison.OrdinalIgnoreCase))
            {
                // If the nick is not registered, or we're properly identified continue.
                if (notice.StartsWith("You are now identified for", StringComparison.OrdinalIgnoreCase) ||
                        (notice.StartsWith("The nickname", StringComparison.OrdinalIgnoreCase) &&
                         notice.EndsWith("is not registered", StringComparison.OrdinalIgnoreCase))
                    )
                {
                    Logger.Info("Handshake completed, joining channels.");

                    foreach (ChannelElement channel in _configuration.Networks["FreeNode"].Channels)
                    {
                        _connection.Sender.Join(channel.Name);
                        Logger.InfoFormat("joining {0}...", channel.Name);
                    }
                }
                // log auth failures
                else
                {
                    // TODO: Kog JUN-05 2008 - do we want to quit here, or what?
                    Logger.Info("AUTHENTICATION FAILURE");
                }
            }
        }

        /// <summary>
        /// Handle disconnection. 
        /// </summary>
        /// 
        /// <remarks>
        /// We currently attempt to reconnect 5 times, given a 60 second stagger between connection attempts.
        /// </remarks>
        private void Listener_OnDisconnect()
        {
            int reconnectCount = 1;

            // Disconnect our event sources... see docs on the method, or on Connect.
            UnwireEvents();

            // Let's not reconnect when we're shutting down...
            if (_botStatus == BotStatus.Started)
            {
                // Try and reconnect.
                do
                {
                    Connect();

                    if (!IsConnected)
                    {
                        Logger.WarnFormat("Failed whilst attempting reconnection attempt #{0}", reconnectCount);

                        // Because of the do-while we end up with n+1 sleeps... not a big deal, but just remember.
                        Thread.Sleep(60000);
                        reconnectCount++;
                    }
                }
                while (!IsConnected && reconnectCount <= 5);

                // If we went through all the reconnect attempts, and we're still not getting anywhere kill the bot
                if (!IsConnected)
                {
                    Shutdown();
                }
            }
        }

        /// <summary>
        /// Remove the wiring for all our event sources. Please see the documentation on <see cref="Connect"/> for details.
        /// </summary>
        private void UnwireEvents()
        {
            _connection.Listener.OnRegistered -= OnRegister;
            _connection.Listener.OnPrivateNotice -= OnPrivateNotice;
            _connection.OnRawMessageReceived -= OnRawMessageReceived;
            _connection.OnRawMessageSent -= OnRawMessageSent;
            _connection.Listener.OnDisconnected -= Listener_OnDisconnect;
        }

        #endregion
    }
}