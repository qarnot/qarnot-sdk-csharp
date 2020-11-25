using System;

public interface IEnvironmentWrapper
{
    string GetEnvironmentVariable(string environmentName);
}

public class EnvironmentWrapper:IEnvironmentWrapper
{
    public string GetEnvironmentVariable(string environmentName)
    {
        return Environment.GetEnvironmentVariable(environmentName);
    }
}
