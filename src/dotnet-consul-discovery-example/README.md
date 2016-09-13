# Service Discovery with Consul - Example
This project contains sample .NET Core Web API project that utilizes Consul service discovery wrapper.

## Running the Project

From the directory where this `README.md` file resides, execute:

  dotnet build

to build the project.

To run, execute:

  dotnet run --discovery-server-origin=http://localhost:8500

Notice that the above assumes you have Consul listening to `localhost:8500` interface.

