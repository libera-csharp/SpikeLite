/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using SpikeLite.Domain.Model.People;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.People
{
    // TODO [Kog 04/09/2012] : Need a paginated finder... Tends to kill WCF clients with the default 65536 message size.

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
                             .SingleOrDefault() ?? new Person {Name = name};

            return entity;
        }

        public void SaveFactoid(PersonFactoid factoid)
        {
            HibernateTemplate.Save(factoid);
        }

        public void DeleteFactoidById(int factoidId)
        {
            var target = Session.QueryOver<PersonFactoid>()
                                .Where(factoid => factoid.Id == factoidId)
                                .SingleOrDefault();

            if (target != null)
            {
                Session.Delete(target);
            }
        }
    }
}