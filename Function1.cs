using System;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.Diagnostics;

namespace ServerStatus
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task Run([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer)
        {
            
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Disabled: {Environment.GetEnvironmentVariable("AzureWebJobs.Function1.Disabled")}");

            if(DateTime.Now.Minute.ToString() == "0" )
            {
                Environment.SetEnvironmentVariable("AzureWebJobs.Function1.Disabled", "0");
                _logger.LogInformation("Reseting the Disabled bit");
            }

            if (Environment.GetEnvironmentVariable("AzureWebJobs.Function1.Disabled") != "1")
            {
                var client = new SecretClient(vaultUri: new Uri("https://myserverstatuskeyvault12.vault.azure.net/"), credential: new DefaultAzureCredential());
                KeyVaultSecret secret = client.GetSecret("EmailKeyThingy");
                var connectionString = secret.Value;
                var emailClient = new EmailClient(connectionString);

                var sender = "DoNotReply@topeadio.com";
                var recipient = "adio.tope12@gmail.com";

                try
                {
                    var request = new HttpClient();
                    var response = await request.GetAsync("https://www.topeadio.com/");
                    if (!response.IsSuccessStatusCode)
                    {

                        _logger.LogInformation("Sending Bad Status Code Email");
                        var emailSendOperation = await emailClient.SendAsync(
                            wait: WaitUntil.Completed,
                            senderAddress: sender, // The email address of the domain registered with the Communication Services resource
                            recipientAddress: recipient,
                            subject: "topeadio.com Returned a Bad Status Code",
                            htmlContent: $"<html><body><h1>Bad Status Code Received From Website</h1><br/><h4>Status Code: {response.StatusCode}</h4><br><p>https://www.topeadio.com/</p></body></html>");
                        _logger.LogInformation($"Email Sent. Status = {emailSendOperation.Value.Status}");

                        Environment.SetEnvironmentVariable("AzureWebJobs.Function1.Disabled", "1");
                        /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                        string operationId = emailSendOperation.Id;
                        _logger.LogInformation($"Email operation id = {operationId}");

                    }
                }
                catch (Exception ex)
                {
                    /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                    _logger.LogError($"Error Accessing www.topeadio.com or While Sending Email: {ex.Data}, message: {ex.Message}");
                    var emailSendOperation = await emailClient.SendAsync(
                           wait: WaitUntil.Completed,
                           senderAddress: sender, // The email address of the domain registered with the Communication Services resource
                           recipientAddress: recipient,
                           subject: "topeadio.com Returned an Exception",
                           htmlContent: $"<html><body><h1>Exception: {ex.Message}</h1><br/><h4>Stack Trace:\n{ex.StackTrace}</h4><br><p>https://www.topeadio.com/</p></body></html>");
                    _logger.LogInformation($"Email Sent. Status = {emailSendOperation.Value.Status}");

                    Environment.SetEnvironmentVariable("AzureWebJobs.Function1.Disabled", "1");
                    /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                    string operationId = emailSendOperation.Id;
                    _logger.LogInformation($"Email operation id = {operationId}");
                }
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
