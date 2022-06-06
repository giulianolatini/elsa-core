using System.Text.Json.Serialization;
using Elsa.Expressions.Models;
using Elsa.Workflows.Core.Attributes;
using Elsa.Workflows.Core.Behaviors;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Core.Services;

namespace Elsa.Workflows.Core.Activities;

[Activity("Elsa", "Control Flow", "Iterate over a set of values.")]
public class ForEach : Activity
{
    private const string CurrentIndexProperty = "CurrentIndex";

    public ForEach()
    {
        Behaviors.Add<BreakBehavior>(this);
        Behaviors.Remove<AutoCompleteBehavior>();
    }

    public ForEach(ICollection<object> items) : this()
    {
        Items = new Input<ICollection<object>>(items);
    }

    /// <summary>
    /// The set of values to iterate.
    /// </summary>
    [Input]
    public Input<ICollection<object>> Items { get; set; } = new(Array.Empty<object>());

    /// <summary>
    /// The activity to execute for each iteration.
    /// </summary>
    [Outbound]
    public IActivity? Body { get; set; }

    /// <summary>
    /// The current value being iterated will be assigned to the specified <see cref="MemoryReference"/>.
    /// </summary>
    [JsonIgnore]
    [Output(Description = "Captures the current value of the list of items being iterated.")]
    public Output<MemoryReference?> CurrentValue { get; set; } = new();

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Execute first iteration.
        await HandleIteration(context);
    }

    private async Task HandleIteration(ActivityExecutionContext context)
    {
        var currentIndex = context.GetProperty<int>(CurrentIndexProperty);
        var items = context.Get(Items)!.ToList();

        if (currentIndex >= items.Count)
        {
            await context.CompleteActivityAsync();
            return;
        }

        var currentItem = items[currentIndex];
        context.Set(CurrentValue, currentItem);
        //CurrentValue.Set(context, currentItem);

        if (Body != null)
            context.ScheduleActivity(Body, OnChildCompleted);

        // Increment index.
        context.UpdateProperty<int>(CurrentIndexProperty, x => x + 1);
    }

    private async ValueTask OnChildCompleted(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
        await HandleIteration(context);
    }
}

public class ForEach<T> : ForEach
{
    [JsonConstructor]
    public ForEach()
    {
    }

    public ForEach(Input<ICollection<T>> items) : this()
    {
        Items = items;
    }

    public ForEach(ICollection<T> items) : this(new Input<ICollection<T>>(items))
    {
    }

    [Input]
    public new Input<ICollection<T>> Items
    {
        get => new(base.Items.Expression, base.Items.MemoryReference);
        set => base.Items = new Input<ICollection<object>>(value.Expression, value.MemoryReference);
    }

    public new Output<Variable<T>?> CurrentValue
    {
        get => new(base.CurrentValue.MemoryReference);
        set => base.CurrentValue = new Output<MemoryReference?>(value.MemoryReference);
    }
}