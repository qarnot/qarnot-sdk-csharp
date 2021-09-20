# Qarnot computing C# SDK [![Build status](https://ci.appveyor.com/api/projects/status/pl5j147jmkc6fbor?svg=true)](https://ci.appveyor.com/project/qarnot/qarnot-sdk-csharp) [![NuGet Version](http://img.shields.io/nuget/v/QarnotSDK.svg?style=flat)](https://www.nuget.org/packages/QarnotSDK/)

> QarnotSDK is a C# assembly designed to interact with Qarnot cloud computing service.
> It allows users to launch, manage and monitor payloads running on distributed computing nodes deployed in Qarnot’s digital heaters.
You can find samples and detailed information on http://computing.qarnot.com.

## Getting Started

### Get your Qarnot account

Create your Qarnot account at http://account.qarnot.com and get your qarnot connection token.

### Add the SDK to your project

You can find the sdk package in nuget : https://www.nuget.org/packages/QarnotSDK

And install it with the `dotnet add package QarnotSDK` command.

### Run a new project

Create a new Connection object using your Qarnot token: `new QarnotSDK.Connection("QARNOT_TOKEN_HERE")`
Create a new QTask object and submit it.

## Projects Examples

### Hello example

All the Qarnot tasks run inside docker containers (linux or windows envs).

A basic task will need a name, a profile (it define the task specificities), and a number of instance to be launch.
The generic profile is `docker-batch`. This profile will launch a `ubuntu` docker image that need a shell command to execute.

```C#

using System;
using QarnotSDK;
using System.Threading;
using System.Threading.Tasks;

namespace HelloWorldQarnot
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var myQarnotToken = "secret_token"; // Retrieve your token from https://account.qarnot.com
            var connection = new QarnotSDK.Connection(myQarnotToken);
            var task = connection.CreateTask("HelloWorldTask", "docker-batch", 4);
            task.Constants.Add("DOCKER_CMD", "echo hello world from node ${INSTANCE_ID}!"); // The docker command to be launch
            await task.RunAsync();
            Console.WriteLine(await task.StdoutAsync());

            Console.WriteLine("End of the run!");
        }
    }
}
```

Output :

```bash
" 0> Hello : 0"
" 2> Hello : 2"
" 3> Hello : 3"
" 1> Hello : 1"
End of the run!
```

## Documentation

The SDK C# documentation:
https://computing.qarnot.com/documentation/sdk-csharp/

The global Qarnot documentation and advance command examples:
https://computing.qarnot.com/en/developers/overview/qarnot-computing-home

Full payload examples:
https://blog.qarnot.com/

## The other Qarnot Tools

The API Documentation : https://computing.qarnot.com/documentation/api/
The Python SDK Documentation : https://computing.qarnot.com/documentation/sdk-python/
The Nodejs SDK Documentation : https://computing.qarnot.com/documentation/sdk-nodejs/
The CLI Documentation : https://computing.qarnot.com/documentation/cli/
The Web Console : https://console.qarnot.com
The Web 3D-Render Console : https://render.qarnot.com

## Licence

Apache License
