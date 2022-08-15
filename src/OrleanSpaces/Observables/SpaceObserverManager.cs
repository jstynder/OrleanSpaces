﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OrleanSpaces.Observables;

internal sealed class SpaceObserverManager : IEnumerable<ISpaceObserver>
{
    private readonly ConcurrentDictionary<ISpaceObserver, DateTime> observers;

    public SpaceObserverManager()
    {
        observers = new ConcurrentDictionary<ISpaceObserver, DateTime>();
        GetDateTime = () => DateTime.UtcNow;
    }

    public Func<DateTime> GetDateTime { get; set; }
    public TimeSpan ExpirationDuration { get; set; }
    public int Count => observers.Count;

    public void Clear()
    {
        observers.Clear();
    }

    public bool IsSubscribed(ISpaceObserver observer)
    {
        return observers.ContainsKey(observer);
    }

    public void Subscribe(ISpaceObserver observer)
    {
        observers[observer] = GetDateTime();
    }

    public void Unsubscribe(ISpaceObserver observer)
    {
        observers.Remove(observer, out _);
    }

    public void Broadcast(Action<ISpaceObserver> notification, Func<ISpaceObserver, bool>? predicate = null)
    {
        var now = GetDateTime();
        var defunct = default(List<ISpaceObserver>);

        foreach (var observer in observers)
        {
            if (ExpirationDuration != TimeSpan.Zero && observer.Value + ExpirationDuration < now)
            {
                defunct ??= new List<ISpaceObserver>();
                defunct.Add(observer.Key);
                continue;
            }

            if (predicate != null && !predicate(observer.Key))
            {
                continue;
            }

            try
            {
                notification(observer.Key);
            }
            catch (Exception)
            {
                defunct ??= new List<ISpaceObserver>();
                defunct.Add(observer.Key);
            }
        }

        if (defunct != default(List<ISpaceObserver>))
        {
            foreach (var observer in defunct)
            {
                observers.Remove(observer, out _);
            }
        }
    }

    public void ClearExpired()
    {
        var now = GetDateTime();
        var defunct = default(List<ISpaceObserver>);
        foreach (var observer in observers)
        {
            if (observer.Value + ExpirationDuration < now)
            {
                defunct ??= new List<ISpaceObserver>();
                defunct.Add(observer.Key);
            }
        }

        if (defunct != default(List<ISpaceObserver>))
        {
            foreach (var observer in defunct)
            {
                observers.Remove(observer, out _);
            }
        }
    }

    public IEnumerator<ISpaceObserver> GetEnumerator()
    {
        return observers.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return observers.Keys.GetEnumerator();
    }
}
