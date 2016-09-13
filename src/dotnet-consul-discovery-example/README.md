# Example: Service Discovery with Consul and .NET
This project contains sample .NET Core Web API project that utilizes Consul service discovery wrapper.

## Running the Project

From the directory where this `README.md` file resides, execute:

    dotnet build

to build the project.

To run, execute:

    dotnet run --discovery-server-origin=http://localhost:8500

Notice that the above assumes you have Consul listening to `localhost:8500` interface.

Now visit http://127.0.0.1:4321/api/values which will output two constant values. To test out service discovery, visit http://127.0.0.1:4321/api/discovered/values which makes the same call (so, the output should be the same) but is getting the base URL from Consul.
