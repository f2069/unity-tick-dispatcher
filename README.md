# Unity Tick Dispatcher

A small extension that allows you to use a single Update/FixedUpdate/etc.

- Zero-allocations when used as a structure (recommended);
- Minimum allocations with IDisposable;
- A local object pool for memory optimization;
- Removing and adding subscriptions without constantly resizing the list and reusing allocated memory;
- Ability to clear internal queues and tighten memory when necessary;
- Not thread-safe.

## Installation

*Requires Unity 2021.1+*

### Install via UPM (using Git URL)

1. Navigate to your project's Packages folder and open the manifest.json file.
2. Add this line to "dependencies"

```json itle="Packages/manifest.json"
{
    "dependencies": {
        "com.ilyakvant.unitytickdispatcher": "https://github.com/IlyaKvant/unity-tick-dispatcher.git#1.1.0"
        // other dependencies
    }
}
```

## Usage

### VContainer

Register TickDispatcher as EntryPoint.<br/>Other API doesn't change.

```csharp
using UnityTickDispatcher;

public sealed class BootstrapScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<TickDispatcher>()
                .As<ITickDispatcher>()
                .WithParameter(LoopTiming.All);

        // ...
    }
}
```

Inject TickDispatcher as implemented interface `ITickDispatcher`.

```csharp
public class MyMonoBehaviour : MonoBehaviour
{
    private TickHandle _tickHandle;

    [Inject]
    public void Construct(ITickDispatcher tickDispatcher)
    {
        _tickDispatcher = tickDispatcher;
    }

    private void OnEnable()
    {
        _tickDispatcher.SubscribeAsHandle(OnLateUpdate, ref _tickHandle, LoopTiming.LateUpdate);
    }

    private void OnLateUpdate()
    {
        // ...
    }

    private void OnDisable()
    {
        _tickHandle.Dispose();
        _tickHandle = default;
    }
}
```

### As MonoBehaviour component

The TickManager component is unavailable if you used VContainer!

- add `TickManager` component to your `Bootstrap` scene in `DontDestroyOnLoad` block;
- setup the needed `TickManager.loopTiming`;
- subscribe on event in your code (don't forget unsubscribe!).

### Usage as TickHandle structure (non-alloc):

```csharp
public class MyMonoBehaviour : MonoBehaviour
{
    private TickHandle _tickHandle;

    private void OnEnable()
    {
        _tickHandle = TickManager.SubscribeAsHandle(OnLateUpdate, LoopTiming.LateUpdate)
    }

    private void OnLateUpdate()
    {
        // ...
    }

    private void OnDisable()
    {
        _tickHandle.Dispose();
        _tickHandle = default;
    }
}
```

or

```csharp
public class MyMonoBehaviour : MonoBehaviour
{
    private TickHandle _tickHandle;

    private void OnEnable()
    {
        // automatically calls _tickHandle.Dispose() before subscribe
        TickManager.SubscribeAsHandle(OnLateUpdate, ref _tickHandle, LoopTiming.LateUpdate);
    }

    private void OnLateUpdate()
    {
        // ...
    }

    private void OnDisable()
    {
        this.DisposeTickHandle(ref _tickHandle);
    }
}
```

### Usage as IDisposable interface (alloc):

```csharp
private readonly CompositeDisposable _disposable = new CompositeDisposable();

public class MyMonoBehaviour : MonoBehaviour
{
    private void OnEnable()
    {
        TickManager
            .Subscribe(OnFixedUpdate, LoopTiming.FixedUpdate)
            .AddTo(_disposable);
    }

    private void OnFixedUpdate()
    {
        // ...
    }

    private void OnDisable()
    {
        _disposable.Clear();
    }
}
```

### Available processings

```csharp
public enum LoopTiming
{
    FixedUpdate,

    Update,
    LastUpdate,

    LateUpdate,
    LastLateUpdate,
}
```

## Garbage collect

If your project has a separate script that periodically unloads unused resources or monitors memory leaks, you can also
call this method.

This operation in next tick will remove all empty subscriptions and trim active lists to their original size.

```csharp
private void InProjectGarbageCollect()
{
    TickManager.Optimize();
}
```