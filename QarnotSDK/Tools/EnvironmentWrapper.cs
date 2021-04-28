#if (DEBUG)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QarnotSDK.UnitTests")]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QarnotSDK.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d90399a0cf9d9d"
                                                                                            + "50a5999f3da1a6d5a5a9777d0e5faf95a10381da641b3fbf15f9ea13096eda48f1ab8498d348d3"
                                                                                            + "d711185ed238ec17a097c0c7a39754643f9eebecb6c12ccdc767c0655845fc03c33f1ee90a7cce"
                                                                                            + "23b6d8f40bde0bffff0d8a4cf2b0d0ea365b2f58c1293854c2f23ed7e8196a06e5e33a1e25f5fd"
                                                                                            + "3ab01d88")]
#endif
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace QarnotSDK
{
    using System;

    internal interface IEnvironmentWrapper
    {
        string GetEnvironmentVariable(string environmentName);
    }

    internal class EnvironmentWrapper : IEnvironmentWrapper
    {
        public string GetEnvironmentVariable(string environmentName)
        {
            return Environment.GetEnvironmentVariable(environmentName);
        }
    }
}
