using System.Reflection;
using Elsa.Workflows.Core.Attributes;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Core.Services;
using Elsa.Workflows.Core.Signals;

namespace Elsa.Workflows.Core.Behaviors;

/// <summary>
/// Implements a behavior that invokes "child completed" callbacks on parent activities.
/// </summary>
public class ScheduledChildCallbackBehavior : Behavior
{
    public ScheduledChildCallbackBehavior(IActivity owner) : base(owner)
    {
        OnSignalReceived<ActivityCompleted>(OnActivityCompletedAsync);
    }

    private async ValueTask OnActivityCompletedAsync(ActivityCompleted signal, SignalContext context)
    {
        var activityExecutionContext = context.ReceiverActivityExecutionContext;
        var childActivityExecutionContext = context.SenderActivityExecutionContext;
        var childActivity = childActivityExecutionContext.Activity;
        var callbackEntry = activityExecutionContext.WorkflowExecutionContext.PopCompletionCallback(activityExecutionContext, childActivity);

        if (callbackEntry == null)
            return;

        // Before invoking the parent activity, make sure its properties are evaluated.
        if (!activityExecutionContext.GetHasEvaluatedProperties())
            await activityExecutionContext.EvaluateInputPropertiesAsync();

        // If no callback was specified, check if there's a [Port] attribute present on the completed child and use its name to complete the activity with outcomes.
        if (callbackEntry.CompletionCallback != null)
        {
            await callbackEntry.CompletionCallback(activityExecutionContext, childActivityExecutionContext);
        }
        else
        {
            var ports = Owner.GetType().GetProperties().Where(x => typeof(IActivity).IsAssignableFrom(x.PropertyType)).ToList();

            var portQuery =
                from p in ports
                let i = (IActivity)p.GetValue(Owner)
                where i == childActivity
                select new { PortProperty = p, PortActivity = i };

            var port = portQuery.FirstOrDefault();

            if (port == null)
                return;

            var portName = port.PortProperty.GetCustomAttribute<PortAttribute>()?.Name ?? port.PortProperty.Name;
            await activityExecutionContext.CompleteActivityWithOutcomesAsync(portName);
        }
    }
}