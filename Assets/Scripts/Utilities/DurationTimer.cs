//Inspired by Marnielle Lloyd Estrada: https://coffeebraingames.wordpress.com/2014/10/20/a-generic-duration-timer-class/

using UnityEngine;

public class DurationTimer
{
    private float _currentTime;
    private float _duration;

    public DurationTimer(float newDuration)
    {
        Reset(newDuration);
    }

    /// <summary>
    /// Adds deltaTime to the currently elapsed time.
    /// </summary>
    public void Tick()
    {
        _currentTime += Time.deltaTime;
    }

    public void Reset()
    {
        _currentTime = 0f;
    }

    public void Reset(float newDuration)
    {
        Reset();
        _duration = newDuration;
    }

    /// <summary>
    /// Returns whether the timer has ended.
    /// </summary>
    public bool HasElapsed()
    {
        return Comparison.TolerantGreaterThanOrEquals(_currentTime, _duration);
    }

    /// <summary>
    /// Returns the ratio between the full duration and the currently elapsed time. 0 means timer hasn't started,
    /// 1 would mean that it has elapsed (ended).
    /// </summary>
    public float GetRatio()
    {
        if (Comparison.TolerantLesserThanOrEquals(_duration, 0f))
            // bad duration time value
            // if countdownTime is zero, ratio will be infinity (divide by zero)
            // we just return 1.0 here for safety
            return 1.0f;

        float ratio = _currentTime / _duration;
        return Mathf.Clamp01(ratio);
    }

    /// <summary>
    /// Returns the current elapsed time.
    /// </summary>
    public float GetPolledTime()
    {
        return _currentTime;
    }

    /// <summary>
    /// Artificially send the timer to its end time.
    /// </summary>
    public void EndTimer()
    {
        _currentTime = _duration;
    }

    /// <summary>
    /// Sends the timer to a specific time, which has the be within the timer's duration. 
    /// </summary>
    public void ForceCurrentTime(float newTime)
    {
        if (Comparison.TolerantLesserThanOrEquals(newTime, _duration))
        {
            _currentTime = newTime;
        }
        else
        {
            Debug.LogError("A script tried to force a timer to go past its duration, clamping to the duration.");
            EndTimer();
        }
    }

    public float GetDurationTime()
    {
        return _duration;
    }
}