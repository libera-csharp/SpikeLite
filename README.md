SpikeLite - FreeNode ##csharp's IRC bot
=============

This readme provides a basic set of information about SpikeLite. More can be found at http://wiki.freenode-csharp.net/wiki/.

Getting Started:
------------

Obviously you'll need to get the source from GitHub. There should be plenty of help on how to do that, if needed.

### Building The Bot

SpikeLite targets .NET4, so the project file is VS2010 compatible. It has been verified to work on Mono 2.10, and against
MonoDevelop 2.6. You'll want to set the FrontEnd_Console as your start project, if you'd like to run the bot via an IDE. All
build artifacts will be present in output\Debug or output\Release, depending on how you built it. Right now you'll want x86 only.

There's a NAnt build file that seems to work well enough with XBuild, but still seems to have problems on Windows against MSBuild.

### Artifact Structure

* FrontEnd_Console.exe - This is the console runner, and runs the bot. Running it (when properly configured) will also cause the bot to log to spikelite.log.
* DLL files - These are the misc dependencies the bot uses. 
* Config files - SpikeLite does not actually use the standard app.config file, and instead uses Spring.NET. More on that later.
* log4net.xml - This configures Log4NET for the bot. We've tried to provide sensible defaults, when possible.
* Spring\Config\beans-overrides.xml - This is how the bot is actually configured. This is a bunch of Spring.NET objects.

### Getting Started

Out of the box SpikeLite will use an SQLite3 database file, and will create the schema if none exists. During startup the bot will also attempt to seed
a bunch of known hosts (cloaks) of administrative users. Right now you'll have to manually add yourself, more to come on that later.

If you take a gander at beans-overrides.xml, you'll see some points that need setting up:

* The bot's nickname and password - If you don't use identification, set the "server.supports.identification" to false. Otherwise it will block on waiting for a response from NickServ.
* You must have a proper Bing API key in "apikeys.bing" - these can be obtained at (insert here).
* You must have a proper referrer URL in "referrers.googleajax" - this can be obtained at (insert here).
* If you want another DB provider, you'll need to set up the DB section. It's recommended that you skip this for now.
* You'll want to populate the Channel list.  There's a sample in there, just make sure subsequent channels have unique ID XML attributes.

For more information please see http://wiki.freenode-csharp.net/wiki/.
