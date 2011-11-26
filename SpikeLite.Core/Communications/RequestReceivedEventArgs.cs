using System;

namespace SpikeLite.Communications
{
    /// <summary>
    /// Defines a set of event arguments for a message being received.
    /// </summary>
    public class RequestReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Holds the <see cref="Request"/> for the inbound message.
        /// </summary>
        public Request Request { get; private set; }

        /// <summary>
        /// Constructs an instance of our request arguments, which basically wrap a <see cref="Request"/>.
        /// </summary>
        /// 
        /// <param name="request">The <see cref="Request"/> representing our inbound message.</param>
        public RequestReceivedEventArgs(Request request)
        {
            Request = request;
        }
    }
}