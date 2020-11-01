﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Services;
using Elsa.Services.Models;
using Newtonsoft.Json;

namespace Elsa
{
    public static class WorkflowBlueprintExtensions
    {
        public static ValueTask<T> GetActivityPropertyValue<TActivity, T>(
            this IWorkflowBlueprint workflowBlueprint, 
            string activityId, 
            Expression<Func<TActivity, T>> propertyExpression, 
            ActivityExecutionContext activityExecutionContext, 
            CancellationToken cancellationToken) where TActivity : IActivity
        {
            var expression = (MemberExpression)propertyExpression.Body;
            string propertyName = expression.Member.Name;
            return GetActivityPropertyValue<T>(workflowBlueprint, activityId, propertyName, activityExecutionContext, cancellationToken);
        }
        
        public static async ValueTask<T> GetActivityPropertyValue<T>(
            this IWorkflowBlueprint workflowBlueprint, 
            string activityId, 
            string propertyName, 
            ActivityExecutionContext activityExecutionContext, 
            CancellationToken cancellationToken)
        {
            var provider = workflowBlueprint.ActivityPropertyProviders.GetProvider(activityId, propertyName);

            if (provider == null)
                return default!;

            var value = await provider.GetValueAsync(activityExecutionContext, cancellationToken);
            return (T)value!;
        }
        
        public static T GetActivityState<TActivity, T>(
            this IWorkflowBlueprint workflowBlueprint, 
            string activityId, 
            Expression<Func<TActivity, T>> propertyExpression, 
            ActivityExecutionContext activityExecutionContext) where TActivity : IActivity
        {
            var expression = (MemberExpression)propertyExpression.Body;
            string propertyName = expression.Member.Name;
            return GetActivityState<T>(workflowBlueprint, activityId, propertyName, activityExecutionContext);
        }
        
        public static T GetActivityState<T>(
            this IWorkflowBlueprint workflowBlueprint, 
            string activityId, 
            string propertyName, 
            ActivityExecutionContext activityExecutionContext)
        {
            var activity = activityExecutionContext.ActivityInstance;
            var serializer = activityExecutionContext.GetService<JsonSerializer>();
            return activity.Data[propertyName]!.ToObject<T>(serializer)!;
        }
    }
}