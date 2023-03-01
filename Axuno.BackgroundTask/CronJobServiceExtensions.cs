using System;
using System.Collections.Generic;
using System.Text;
using Axuno.BackgroundTask;
using Microsoft.Extensions.DependencyInjection;

namespace Axuno.BackgroundTask;

public static class CronJobServiceExtensions
{
    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
        }
        var config = new ScheduleConfig<T>();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
        }

        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }
}

public interface IScheduleConfig<T>
{
    string CronExpression { get; set; }
    TimeZoneInfo TimeZoneInfo { get; set; }
}

public class ScheduleConfig<T> : IScheduleConfig<T>
{
    public string CronExpression { get; set; } = string.Empty;
    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Utc;
}