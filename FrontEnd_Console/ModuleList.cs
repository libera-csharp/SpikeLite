﻿/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using SpikeLite.Modules;

namespace FrontEnd_Console
{
    // TODO: Kog 12/27/2008 - I'm considering contributing something like util:map to Spring.NET.

    /// <summary>
    /// Unfortunately Spring.NET doesn't implement something like the Java version's util:map, so
    /// for now we'll be using this. We strip off our generic parameters and allow ourselves to be passed
    /// around like a first-class Spring bean.
    /// </summary>
    public class ModuleList : List<IModule>
    {
        /// <summary>
        /// Pretty much a no-op here. We just get rid of the generic type parameter.
        /// </summary>
        /// 
        /// <param name="collection">An enumeration of modules to throw into our list.</param>
        public ModuleList(IEnumerable<IModule> collection) : base(collection) {}
    }
}