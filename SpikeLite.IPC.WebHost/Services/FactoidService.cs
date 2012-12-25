/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

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

        // TODO [Kog 04/09/2012] : Consider marking deleted instead of doing an actual delete.

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

        public void DeleteFactoidById(int id)
        {
            var peopleDao = GetBean<IPeopleDao>("PersonDao");  
            peopleDao.DeleteFactoidById(id);
        }
    }
}
