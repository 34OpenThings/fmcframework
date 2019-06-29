using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if ACTIVATE_ANALYTICS_CALLS
using Firebase;
using Firebase.Analytics;
#endif

/**
 * This file contains all the classes used to perform Analytics.
 * FMCAnalytics provides a list of all the methods used to communicate with the services.
 * FMCAnalytics can be derived to give a specific implementations (i.e. FMCFirebaseAnalytics).
 **/

public abstract class FMCAnalytics
{
    public const string PrecompileDirectiveName = "ACTIVATE_ANALYTICS_CALLS"; // used by game settings to activate third party calls. Has to match the name of the directives used below.

    public bool IsServiceInitialized { get; protected set; }

    public abstract void Event(string eventName);
    public abstract void Event(string eventName, string paramName, int param);
    public abstract void Event(string eventName, string paramName, long param);
    public abstract void Event(string eventName, string paramName, double param);
    public abstract void Event(string eventName, string paramName, string param);
    public abstract void Event(string eventName, params FMCAnalyticsParameter[] parameters);
}

#if ACTIVATE_ANALYTICS_CALLS

/// <summary>
/// This class implements Analytics with Firebase.
/// All the other classes are agnostic to the Analytics service used.
/// </summary>
public class FMCFirebaseAnalytics : FMCAnalytics
{
    public FMCFirebaseAnalytics()
    {
        IsServiceInitialized = false;
        CheckAndInitializeFirebase();
    }

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    void CheckAndInitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
                InitializeFirebase();
            else
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
        });
    }

    // Handle initialization of the necessary Firebase modules:
    void InitializeFirebase()
    {
        Debug.Log("Enabling data collection.");
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        // Set the user's sign up method.
        FirebaseAnalytics.SetUserProperty(FirebaseAnalytics.UserPropertySignUpMethod, "Google");
        // Set the user ID.
        FirebaseAnalytics.SetUserId("uber_user_510");
        // Set default session duration values.
        FirebaseAnalytics.SetMinimumSessionDuration(new TimeSpan(0, 0, 10));
        FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

        IsServiceInitialized = true;
        Debug.Log("Firebase initialized!");
    }

    /// <summary>
    /// Given an array of fmc AnaliticsParams, returns a Firebase params array
    /// </summary>
    Parameter[] FMCAnalyticsParametersToFirebaseParameters(FMCAnalyticsParameter[] parameters)
    {
        Parameter[] firebaseParams = new Parameter[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            FMCAnalyticsParameter fmcParameter = parameters[i];

            if (fmcParameter.IsLong)
                firebaseParams[i] = new Parameter(fmcParameter.Key, fmcParameter.LongValue);
            else if (fmcParameter.IsDouble)
                firebaseParams[i] = new Parameter(fmcParameter.Key, fmcParameter.DoubleValue);
            else if (fmcParameter.IsString)
                firebaseParams[i] = new Parameter(fmcParameter.Key, fmcParameter.StringValue);
        }

        return firebaseParams;
    }

#region Firebase implementation of events

    public override void Event(string eventName) { FirebaseAnalytics.LogEvent(eventName); }
    public override void Event(string eventName, string paramName, int param) { FirebaseAnalytics.LogEvent(eventName, paramName, param); }
    public override void Event(string eventName, string paramName, long param) { FirebaseAnalytics.LogEvent(eventName, paramName, param); }
    public override void Event(string eventName, string paramName, double param) { FirebaseAnalytics.LogEvent(eventName, paramName, param); }
    public override void Event(string eventName, string paramName, string param) { FirebaseAnalytics.LogEvent(eventName, paramName, param); }
    public override void Event(string eventName, params FMCAnalyticsParameter[] parameters) { FirebaseAnalytics.LogEvent(eventName, FMCAnalyticsParametersToFirebaseParameters(parameters)); }

#endregion
}
#endif

/**
 * A dummy class to softly disable Analytics.
 **/
public class FMCTestAnalytics : FMCAnalytics
{
    public override void Event(string eventName) { Debug.Log("Analytics: " + eventName); }
    public override void Event(string eventName, string paramName, int param) { Debug.Log("Analytics: " + eventName + " - " + param); }
    public override void Event(string eventName, string paramName, long param) { Debug.Log("Analytics: " + eventName + " - " + param); }
    public override void Event(string eventName, string paramName, double param) { Debug.Log("Analytics: " + eventName + " - " + param); }
    public override void Event(string eventName, string paramName, string param) { Debug.Log("Analytics: " + eventName + " - " + param); }
    public override void Event(string eventName, params FMCAnalyticsParameter[] parameters) { Debug.Log("Analytics: " + eventName + " - " + parameters); }
}

/// <summary>
/// A parameter of an Analytics event.
/// Using this from outside, the implementation is completely independent from the service used
/// </summary>
public class FMCAnalyticsParameter
{
    AnalyticsParameterType type;
    object value;

    enum AnalyticsParameterType { Long, Double, String }

    public string Key { get; protected set; }

    public bool IsLong { get { return type == AnalyticsParameterType.Long; } }
    public bool IsDouble { get { return type == AnalyticsParameterType.Double; } }
    public bool IsString { get { return type == AnalyticsParameterType.String; } }

    public long LongValue { get { return (long)value; } }
    public double DoubleValue { get { return (double)value; } }
    public string StringValue { get { return (string)value; } }

    private FMCAnalyticsParameter(string key, object value)
    {
        Key = key;
        this.value = value;
    }

    public FMCAnalyticsParameter(string key, long value) : this(key, (object)value) { type = AnalyticsParameterType.Long; }
    public FMCAnalyticsParameter(string key, double value) : this(key, (object)value) { type = AnalyticsParameterType.Double; }
    public FMCAnalyticsParameter(string key, string value) : this(key, (object)value) { type = AnalyticsParameterType.String; }
}
