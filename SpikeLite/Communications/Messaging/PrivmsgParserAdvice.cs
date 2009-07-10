using System.Reflection;
using log4net.Ext.Trace;
using Sharkbite.Irc;
using Spring.Aop;

namespace SpikeLite.Communications.Messaging
{
    // TODO: Kog 07/10/2009 - Add in the rest of SPIKE-12 when AJ gets back to me on this.

    /// <summary>
    /// <see cref="IMethodBeforeAdvice"/> advice wrapped around our underlying IRC module's communications. This allows people to set advice
    /// on the bot and intercept raw messages before they've had a chance to be parsed and sent to our modules.
    /// </summary>
    class PrivmsgParserAdvice : IMethodBeforeAdvice
    {
        /// <summary>
        /// Holds a logger that we can use for spamming schtuff.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl) TraceLogManager.GetLogger(typeof (PrivmsgParserAdvice));

        /// <summary>
        /// Intercepts a message being relayed to the bot. Can do all kinds of magical tricks!
        /// </summary>
        /// 
        /// <param name="method">The <see cref="MethodInfo"/> regarding the execution target.</param>
        /// <param name="args">An optional set of arguments being passed.</param>
        /// <param name="target">The execution target.</param>
        public void Before(MethodInfo method, object[] args, object target)
        {
            UserInfo user = (UserInfo) args[0];
            string channelTarget = "<privmsg>";
            string message;

            // We've received a 1:1 PRIVMSG.
            if (args.Length == 2)
            {
                message = (string)args[1];
            }
            else
            {
                channelTarget = (string) args[1];
                message = (string) args[2];
            }

            _logger.TraceFormat("RawMessage received: [User: {0} ({1})] [Target: {2}] [Message: {3}]", user.Nick, user.Hostname, channelTarget, message);
        }
    }
}
