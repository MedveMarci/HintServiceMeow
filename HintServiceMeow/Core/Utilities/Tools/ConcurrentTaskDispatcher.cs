using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.Tools;

internal class ConcurrentTaskDispatcher : IConcurrentTaskDispatcher
{
    private readonly BlockingCollection<ITaskPatch> taskQueue = new();
    private readonly List<Task> workers = [];

    public static IConcurrentTaskDispatcher Instance { get; private set; } = new ConcurrentTaskDispatcher(Environment.ProcessorCount - 1);

    public ConcurrentTaskDispatcher(int workerCount)
    {
        for (; workerCount > 0; workerCount--) workers.Add(Task.Run(WorkerMethod));
    }

    public void Enqueue(Func<Task> task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        TaskPatch wrapper = new(task);
        taskQueue.Add(wrapper);
    }

    public Task<T> Enqueue<T>(Func<Task<T>> task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        TaskPatch<T> wrapper = new(task);
        taskQueue.Add(wrapper);
        return wrapper.Completion.Task;
    }

    private async Task WorkerMethod()
    {
        foreach (ITaskPatch? task in taskQueue.GetConsumingEnumerable())
            try
            {
                await task.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
    }

    private interface ITaskPatch
    {
        Task ExecuteAsync();
    }

    private class TaskPatch<T>(Func<Task<T>> task) : ITaskPatch
    {
        public Func<Task<T>> Task { get; } = task ?? throw new ArgumentNullException(nameof(task));

        public TaskCompletionSource<T> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task ExecuteAsync()
        {
            try
            {
                T result = await Task();
                Completion.SetResult(result);
            }
            catch (Exception ex)
            {
                Completion.SetException(ex);
            }
        }
    }

    private class TaskPatch(Func<Task> task) : ITaskPatch
    {
        public Func<Task> Task { get; } = task ?? throw new ArgumentNullException(nameof(task));

        public async Task ExecuteAsync()
        {
            try
            {
                await Task();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }
    }
}