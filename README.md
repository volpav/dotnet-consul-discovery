# Service Discovery with Consul and .NET
This project contains service discovery client/wrapper for Consul targeting .NET Core applications.

## Building and Running

From the directory where this `README.md` file resides, execute:

    dotnet build

to build the project. To run examples, go to `dotnet-consul-discovery-example` and take a look at the note in there.

## Running Consul Locally
You can download Consul here: https://www.consul.io/downloads.html. Here's the Windows command for running it locally with Web UI attached (notice that I'm simply putting `consul.exe` into the Web UI folder and executing the shell command from there):

    ./consul agent -data-dir=C:\Temp\Consul -bind=127.0.0.1 -ui-dir=. -server -dev

Visit http://localhost:8500/ to view the UI and monitor services.




