/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 20011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using SpikeLite.Domain.Model.Authentication;

namespace FrontEnd_Console.Spring
{
    /// <summary>
    /// Because we want to have a first class <see cref="IList{T}"/> of <see cref="KnownHost"/>, we have to craft this work around for Spring.NET.
    /// </summary>
    public class CloakList : List<KnownHost>
    {
        /// <summary>
        /// Pretty much a no-op here. We just get rid of the generic type parameter.
        /// </summary>
        /// 
        /// <param name="collection">An enumeration of cloaks to throw into our list.</param>
        public CloakList(IEnumerable<KnownHost> collection) : base(collection) { }
    }
}
