# Fibs.Net
.NET Core library, proxy and tests as well as web client for [First Internet Backgammon Server (FIBS)](http://fibs.com); all very much under development

## Usage
* Build Fibs.Net.sln: 

  `C:\Fibs.Net\> msbuild Fibs.Net.sln`

* Execute the FibsProxy project, which will set up a WebSocket proxy for fibs.com:4321 on port 5000:

  `C:\Fibs.Net\src\FibsProxy> dotnet run`

  The index.html in FibsProxy\wwwroot is just a raw proof-of-concept HTML client, not a real FIBS client.

* The real FIBS client is under development in FibsWeb. It's written in [Polymer](http://polymer-project.org). To run the FibsWeb client, install the Polymer tools and execute the following from the FibsWeb folder:

  `C:\Fibs.Net\src\FibsWeb> polymer serve -o`

* To do anything (and you can't do much yet), you need to login and to login, you must first have a FIBS account, which you can get by logging in as guest via telnet:

  `telnet fibs.com:4321`

  If you don't have a telnet client (and Windows doesn't seem to ship with one anymore), I recommend [puttytel](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html). Once you have it downloaded, you can log into FIBS with the following:

  `puttytel telnet://fibs.com:4321`

Enjoy!