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
