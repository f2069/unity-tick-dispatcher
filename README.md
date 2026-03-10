# Unity Tick Dispatcher

A small extension that allows you to use a single Update/FixedUpdate/etc.

- Zero-allocations when used as a structure (minimum allocations with IDisposable);
- A local object pool for memory optimization;
- Removing and adding subscriptions without constantly resizing the list and reusing allocated memory;
- Ability to clear internal queues and tighten memory when necessary;
- Not thread-safe.

## Installation

*Requires Unity 2022.3+*

### Install via UPM (using Git URL)

1. Navigate to your project's Packages folder and open the manifest.json file.
2. Add this line to "dependencies"
    
```json itle="Packages/manifest.json"
{
    "dependencies": {
        "com.f2069.unitytickdispatcher": "https://github.com/f2069/unity-tick-dispatcher.git#1.0.0",
        // other dependencies
    }
}
```

## Usage

- add `TickManager` component to your `Bootstrap` scene in `DontDestroyOnLoad` block;
- setup the needed `TickManager.loopTiming`;
- subscribe on event in your code (don't forget unsubscribe);

### Usage as TickHandle structure (non-alloc):

```csharp
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
```

or

```csharp
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
```

### Usage as IDisposable interface (alloc):

```csharp
private void OnEnable()
{
    TickManager
        .Subscribe(OnFixedUpdate, LoopTiming.FixedUpdate)
        .AddTo(CompositeDisposable);
}

private void OnFixedUpdate()
{
    // ...
}

private void OnDisable()
{
    CompositeDisposable.Clear();
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

