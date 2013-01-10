SpikeLite - FreeNode ##csharp's IRC bot
=============

This readme provides a basic set of information about SpikeLite. SpikeLite is MIT licensed.

Getting Started:
------------

Obviously you'll need to get the source from GitHub. There should be plenty of help on how to do that, if needed.

### Building The Bot

SpikeLite targets .NET4.5, so the project file is VS2012 compatible. It has been verified to work on Mono 3.03. You'll want to set 
the SpikeLite.UI.Cli as your start project, if you'd like to run the bot via an IDE. All build artifacts will be present in 
output\Debug or output\Release, depending on how you built it. Right now you'll want x86 only.

There's a NAnt build file that seems to work well enough with XBuild, but still seems to have problems on Windows against MSBuild.

### Artifact Structure

* SpikeLite.UI.Cli.exe - This is the console runner, and runs the bot. Running it (when properly configured) will also cause the bot to log to spikelite.log.
* DLL files - These are the misc dependencies the bot uses. 
* Config files - SpikeLite does not actually use the standard app.config file, and instead uses Spring.NET. More on that later.
* log4net.xml - This configures Log4NET for the bot. We've tried to provide sensible defaults, when possible.
* Spring\Config\beans-overrides.xml - This is how the bot is actually configured. This is a bunch of Spring.NET objects.

### Getting Started

Out of the box SpikeLite will use an SQLite3 database file, and will create the schema if none exists. During startup the bot will also attempt to seed
a bunch of known hosts (cloaks) of administrative users. Right now you'll have to manually add yourself, more to come on that later.

If you take a gander at beans-overrides.xml, you'll see some points that need setting up:

* The bot's nickname and password - If you don't use identification, set the "server.supports.identification" to false. Otherwise it will block on waiting for a response from NickServ.
* You should have a proper Bing API key in "apikeys.bing" - these can be obtained at (insert here).
* You should have a proper referrer URL in "apikeys.google" - this can be obtained at (insert here).
* If you want another DB provider, you'll need to set up the DB section. It's recommended that you skip this for now.
* You'll want to populate the Channel list.  There's a sample in there, just make sure subsequent channels have unique ID XML attributes.

### Modifying the Seeded Users and Adding users

When the bot starts up with no SQLite database (or if the cloaks table is empty), it will seed the ACLs with a set of hosts that can be found in beans.xml.
If you'd like to override this, however, you can add the following XML snippet to your beans-overrides.xml:

  <object id="seedCloaks" type="SpikeLite.UI.Cli.Spring.CloakList, SpikeLite.UI.Cli">
    <constructor-arg index="0">
      <list element-type="SpikeLite.Domain.Model.Authentication.KnownHost, SpikeLite.Domain">
        <object id ="bob" type="SpikeLite.Domain.Model.Authentication.KnownHost, SpikeLite.Domain">
          <property name="AccessLevel" value="Root"/>
          <property name="HostMatchType" value="Start"/>
          <property name="HostMask" value="bob@127.0.0.1"/>
        </object>
      </list>
    </constructor-arg>
  </object>	
  
There are a couple of moving pieces here:

* AccessLevel will either be Root or Public. The difference being (at the moment) that Root can do admin things like shutting down the bot, and managing users (see below).
* HostMatchType can be one of: Full, Start, End or Contains. These can be found in KnownHost.cs, but the gist is as follows:
    * Full does a literal string match.
    * Start checks to see String.StartsWith.
    * End checks to see String.EndsWith.
    * Contains checks to see if String.Contains.
* The hostmask bit should be self-explanitory: it'll be the hostmask that the bot sees for the user, however your network represents them.

Once you're listed as level Root, and the bot is running, you can use the Users module, which comes as part of SpikeLite.Modules.Admin.
You can ask the Bot for help (~help users), and hopefully the directions should be easily enough followed. Here's a quick primer:

* ~users list: this will show the users list.
* ~users add hostmask admin: this will add a user at the Root level.
* ~users add hostmask user: this will add a user at the Public level.
* ~users del hostmask: this will delete a user.

### NuGet

As of July, 2012 we've started to rely on NuGet rather than having all our dependencies as binaries in VCS. Some of our dependencies don't exist in the standard repositories and are kept around. We've tested both the standard MS CLR on Windows as well as against Mono 2.11. If you find yourself having issues with the latter you might want to look at issue #7 (https://github.com/Freenode-Csharp/SpikeLite/issues/7). The short of it is that you'll want to do the following:

    "mozroots --import --sync"

 This will grab the standard set of SSL certs that Mozilla ships, and put them into the "trusted" category (use certmgr to check if you're curious). The --sync option means that you're not going to have to say yes to the approximately 140 certificates. If you have trouble parsing the file and get "No certificates found" you should fetch the file via wget/curl/whatever and use the -file parameter.