using Serilog;
using System.Runtime.CompilerServices;

namespace Common.Extensions
{
    public static class SerilogExtensions
    {
        const string MESSAGE_TEMPLATE = "{InstanceTypeName}.{Caller}() --> {Message}";
        const string MESSAGE_TEMPLATE_ENTER = "Starting: {InstanceTypeName}.{Caller}() --> {Message}";
        const string MESSAGE_TEMPLATE_LEAVE = "Completed: {InstanceTypeName}.{Caller}() --> {Message}";

        public static void Enter(this ILogger logger, object instance, string message,
            [CallerMemberName] string caller = "")
        {
            logger.Debug(MESSAGE_TEMPLATE_ENTER, instance.GetType().Name, caller, message);
        }

        public static void Info(this ILogger logger, object instance, string message,
            [CallerMemberName] string caller = "")
        {
            logger.Information(MESSAGE_TEMPLATE, instance.GetType().Name, caller, message);
        }

        public static void Leave(this ILogger logger, object instance, string message,
            [CallerMemberName] string caller = "")
        {
            logger.Debug(MESSAGE_TEMPLATE_LEAVE, instance.GetType().Name, caller, message);
        }

        public static void Warn(this ILogger logger, object instance, string message,
            [CallerMemberName] string caller = "")
        {
            logger.Warning(MESSAGE_TEMPLATE, instance.GetType().Name, caller, message);
        }

        public static void Fail(this ILogger logger, object instance, string message,
            [CallerMemberName] string caller = "")
        {
            logger.Error(MESSAGE_TEMPLATE, instance.GetType().Name, caller, message);
        }
    }
}
