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
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class FactoidService : AbstractUserContextAwareService, IFactoidService
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
    }
}
