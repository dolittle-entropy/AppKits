using Common.Extensions;
using Polly;
using Polly.Retry;
using Serilog;
using System.Runtime.CompilerServices;

namespace Common.Helpers
{
    public static class RetryPolicyBuilder
    {
        public static RetryPolicy? ForReceiptHandling(object sender, ILogger log, [CallerMemberName] string callerName = "")
        {
            const int RetryCount = 20;
            const int increaseAmountInMilliseconds = 200;
            var senderName = sender.GetType().Name;

            return Policy.Handle<Exception>()
                .WaitAndRetry(
                    retryCount: RetryCount,
                    sleepDurationProvider: attemptNumber => TimeSpan.FromMilliseconds(attemptNumber * increaseAmountInMilliseconds),
                    onRetry: (exception, sleepDuration, retryIndex, context) =>
                    {
                        log.Fail(sender, $"Retrying {senderName}.{callerName} in {sleepDuration}. Retry {retryIndex} of {RetryCount}: {exception.Message}");
                    });
        }

        public static AsyncRetryPolicy? ForDolittleAggregate(object sender, ILogger log, [CallerMemberName] string callerName = "")
        {
            const int RetryCount = 20;
            const int increaseAmountInMilliseconds = 150;
            var senderName = sender.GetType().Name;

            return Policy.Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: RetryCount,
                    sleepDurationProvider: attemptNo => TimeSpan.FromMilliseconds(attemptNo * increaseAmountInMilliseconds),
                    onRetry: (ex, sleep, index, context) =>
                    {
                        log.Fail(sender, $"Retrying {senderName}.{callerName} in {sleep}. Retry {index} of {RetryCount}: {ex.Message}");
                    });
        }

        public static RetryPolicy? ForDataWriter(object sender, ILogger log, [CallerMemberName] string callerName = "")
        {
            const int RetryCount = 20;
            var senderName = sender.GetType().Name;

            return Policy.Handle<Exception>()
                .WaitAndRetry(
                    retryCount: RetryCount,
                    sleepDurationProvider: attemptNumber => TimeSpan.FromMilliseconds(attemptNumber * 100),
                    onRetry: (exception, sleepDuration, retryIndex, context) =>
                    {
                        log.Fail(sender, $"Retrying {senderName}.{callerName} in {sleepDuration}. Retry {retryIndex} of {RetryCount}: {exception.Message}");
                    });
        }

        public static RetryPolicy? ForErpConnector(object sender, ILogger log, [CallerMemberName] string callerName = "")
        {
            const int RetryCount = 5;
            var senderName = sender.GetType().Name;

            return Policy.Handle<Exception>()
                .WaitAndRetry(
                    retryCount: RetryCount,
                    sleepDurationProvider: attemptNumber => TimeSpan.FromSeconds(attemptNumber * 1),
                    onRetry: (exception, sleepDuration, retryIndex, context) =>
                    {
                        log.Fail(sender, $"Retrying {senderName}.{callerName} in {sleepDuration}. Retry {retryIndex} of {RetryCount}: {exception.Message}");
                    });
        }
    }
}
