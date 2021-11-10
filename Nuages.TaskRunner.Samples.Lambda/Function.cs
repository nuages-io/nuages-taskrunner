using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Nuages.TaskRunner.Samples.Lambda
{
    
// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class Function
{
    private ServiceProvider _serviceProvider;

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    // ReSharper disable once EmptyConstructor
    public Function()
    {
    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        // create service provider
        _serviceProvider = serviceCollection.BuildServiceProvider();
            
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private static void ConfigureServices(ServiceCollection serviceCollection)
    {
        //Configure additional Services here
    }

    // ReSharper disable once UnusedParameter.Local
    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var taskRunner = new TaskRunnerService(_serviceProvider, new List<ITaskAuthorizationService>());

        var t = JsonSerializer.Deserialize<RunnableTaskDefinition>(message.Body);

        LambdaLogger.Log("MESSAGE: " + message.Body);
        
        // ReSharper disable once AssignNullToNotNullAttribute
        await taskRunner.ExecuteAsync(t);
    }
}
}
