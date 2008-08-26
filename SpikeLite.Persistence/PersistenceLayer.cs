/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using NHibernate;

namespace SpikeLite.Persistence
{
    // TODO: Kog 07/25/2008 - inject our persistence into our DAOs and not into this.
    // TODO: also, make the DAOs conform to an interface...

    /// <summary>
    /// A really cheap and temporary wrapper around our persistence layer.
    /// This is going out the door as soon as the PoC of nhib goes through.
    /// </summary>
    public class PersistenceLayer
    {
        private ISession session;

        /// <summary>
        /// Construct our layer around NHibernate's session. This will need
        /// to be fixed to be more precise, but it allows for temporary injection.
        /// </summary>
        /// <param name="underlyingSession">The <see cref="ISession"/> object passed to us by the factory</param>
        public PersistenceLayer(ISession underlyingSession)
        {
            session = underlyingSession;
        }

        /// <summary>
        /// Provides access to an NHibernate <see cref="ISession"/> to query against
        /// </summary>
        public ISession Session
        {
            get { return session; }
        }
    }
}
