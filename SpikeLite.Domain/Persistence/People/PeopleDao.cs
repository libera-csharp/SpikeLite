/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using SpikeLite.Domain.Model.People;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.People
{
    // TODO [Kog 06/19/2011] : Probably need to add a way to add offset-based reads. Or, perhaps we should just return the last N (see maximumCount) and
    // TODO [Kog 06/19/2011] : make extended viewing via an IPC service? Though... that means auth needs to be fully baked.

    // TODO [Kog 06/19/2011] : This is pretty rough... CreateOrFind always sucks. Polish this.
    public class PeopleDao : HibernateDaoSupport, IPeopleDao
    {
        public IList<Person> FindAllPeople()
        {
            return Session.QueryOver<Person>()
                          .List();
        }

        public Person CreateOrFindPerson(string name)
        {
            var entity = Session.QueryOver<Person>()
                                .Where(person => person.Name == name)
                                .SingleOrDefault();

            if (entity == null)
            {
                entity = new Person {Name = name};
                HibernateTemplate.Save(entity);
            }

            return entity;
        }

        public void SaveFactoid(PersonFactoid factoid)
        {
            HibernateTemplate.Save(factoid);
        }
    }
}