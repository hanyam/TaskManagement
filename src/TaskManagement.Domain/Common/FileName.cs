using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Text;

namespace TaskManagement.Domain.Common;
public sealed class TaskManagementMetrics
{
    public const string MeterName = "TaskManagement.Metrics";

    private readonly Counter<long> _habitRequestCounter;
    private readonly Counter<long> _newUserRegistrationsCounter;

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "All Meter objects are managed by IMeterFactory")]
    public TaskManagementMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(MeterName);
        _habitRequestCounter = meter.CreateCounter<long>("taskManagement.api.habit_requests.count");
        _newUserRegistrationsCounter = meter.CreateCounter<long>("taskManagement.api.user_registrations.count");
    }

    public void IncreaseHabitsRequestCount(TagList? tags = null)
    {
        _habitRequestCounter.Add(1, tags ?? []);
    }

    public void IncreaseUserRegistrationsCount(TagList? tags = null)
    {
        _newUserRegistrationsCounter.Add(1, tags ?? []);
    }
}