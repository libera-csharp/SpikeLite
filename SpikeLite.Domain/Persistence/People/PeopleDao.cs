/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using System.Linq;
using SpikeLite.Domain.Model.People;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.People
{
    // TODO [Kog 04/09/2012] : Need a paginated finder... Tends to kill WCF clients with the default 65536 message size.

    public class PeopleDao : HibernateDaoSupport, IPeopleDao
    {
        //private IPersistanceSession _persistanceSession = new RavenDBSession();
        private IPersistanceSession _persistanceSession;

        public IList<Person> FindAllPeople()
        {
            _persistanceSession = new NHibernateSession(this.Session);

            return _persistanceSession.Query<Person>().ToList();
        }

        public Person CreateOrFindPerson(string name)
        {
            _persistanceSession = new NHibernateSession(this.Session);

            return _persistanceSession.Query<Person>()
                .SingleOrDefault(p => p.Name == name) ?? new Person() { Name = name };
        }

        public void SaveFactoids(Person entity)
        {
            _persistanceSession = new NHibernateSession(this.Session);

            _persistanceSession.Add(entity);
            _persistanceSession.SaveChanges();
        }

        public void DeleteFactoidById(int factoidId) { }
    }
}