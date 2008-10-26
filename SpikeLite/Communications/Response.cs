/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.Communications
{
    public enum ResponseType
    { 
        Public,
        Private
    }

    public struct Response
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public ResponseType ResponseType { get; set; }
        public string Message { get; set; }
    }
}