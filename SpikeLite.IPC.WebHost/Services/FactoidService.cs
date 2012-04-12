/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;
using AutoMapper;
using SpikeLite.Domain.Model.People;
using SpikeLite.Domain.Persistence.People;
using TransportPerson = SpikeLite.IPC.WebHost.Transport.Person;
using TransportPersonFactoid = SpikeLite.IPC.WebHost.Transport.PersonFactoid;

namespace SpikeLite.IPC.WebHost.Services
{    
    /// <summary>
    /// Provides a service for looking up, and dealing with, factoids.
    /// </summary>
    [ServiceContract(Namespace = "http://tempuri.org")]
    public interface IFactoidService : IConfigurableServiceHost
    {
        /// <summary>
        /// Fetches all factoids within the system.
        /// </summary>
        /// 
        /// <returns>An array of factoids, organized by person.</returns>
        [OperationContract]
        [SecuredOperation]
        TransportPerson[] GetFactoids();

        /// <summary>
        /// Fetches all factoids within the system corresponding to a given target.
        /// </summary>
        /// 
        /// <param name="person">The target person to search for.</param>
        /// 
        /// <returns>A <see cref="Transport.Person"/> corresponding to the name, filled with any relevant factods.</returns>
        [OperationContract]
        [SecuredOperation]
        TransportPerson GetFactoidForPerson(string person);

        /// <summary>
        /// Adds a factoid for the given string identifier, of a given type.
        /// </summary>
        /// 
        /// <param name="person">The string identifier to attach the factoid to. Will be created if does not already exist.</param>
        /// <param name="type">The type of the factoid: warning, idiot, unban etc.</param>
        /// <param name="factoid">The text of the factoid.</param>
        [OperationContract]
        [SecuredOperation("addFactoid")]
        void AddFactoidForPerson(string person, string type, string factoid);

        // TODO [Kog 04/09/2012] : Mark deleted instead of doing an actual delete...

        /// <summary>
        /// Deletes a factoid by ID.
        /// </summary>
        /// 
        /// <param name="id">The ID of the factoid.</param>
        [OperationContract]
        [SecuredOperation("deleteFactoid")]
        void DeleteFactoidById(int id);

    }

    /// <summary>
    /// Provides a concrete implementation of <see cref="IFactoidService"/>.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FactoidService : AbstractUserContextAwareService, IFactoidService
    {
        public override void Configure()
        {
            Mapper.CreateMap<Person, TransportPerson>();
            Mapper.CreateMap<PersonFactoid, TransportPersonFactoid>();

            Mapper.AssertConfigurationIsValid();    
        }

        public TransportPerson[] GetFactoids()
        {
            var peopleDao = GetBean<IPeopleDao>("PersonDao");
            return Mapper.Map<IList<Person>, TransportPerson[]>(peopleDao.FindAllPeople());
        }

        public TransportPerson GetFactoidForPerson(string person)
        {
            var peopleDao = GetBean <IPeopleDao> ("PersonDao");
            return Mapper.Map<Person, TransportPerson>(peopleDao.CreateOrFindPerson(person));
        }

        // TODO [Kog 04/09/2012] : Consider replacing email address with a nickname here. Need to wire a bit more context first though...

        public void AddFactoidForPerson(string person, string type, string factoid)
        {
            var peopleDao = GetBean<IPeopleDao>("PersonDao");
            var target = peopleDao.CreateOrFindPerson(person.ToUpperInvariant());

            peopleDao.SaveFactoid(new PersonFactoid
            {
                Description = factoid,
                CreationDate = DateTime.UtcNow,
                Person = target,
                Type = type,
                CreatedBy = GetPrincipal().EmailAddress
            });
        }

        public void DeleteFactoidById(int id)
        {
            var peopleDao = GetBean<IPeopleDao>("PersonDao");  
            peopleDao.DeleteFactoidById(id);
        }
    }
}
