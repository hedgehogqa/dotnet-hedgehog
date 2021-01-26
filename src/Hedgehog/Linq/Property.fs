namespace Hedgehog.Linq

#if !FABLE_COMPILER

open System
open System.Runtime.CompilerServices
open Hedgehog
open System.Runtime.InteropServices

module internal Build =
    let config testLimit shrinkLimit =
        PropertyConfig.defaultConfig
        |> PropertyConfig.withTests (testLimit |> Option.defaultValue PropertyConfig.defaultConfig.TestLimit)
        |> fun config ->
            match shrinkLimit with
            | Some shrinkLimit -> config |> PropertyConfig.withShrinks shrinkLimit
            | None -> config


[<Extension>]
[<AbstractClass; Sealed>]
type PropertyConfigExtensions private () =

    /// Set the number of times a property is allowed to shrink before the test runner gives up and prints the counterexample.
    [<Extension>]
    static member WithShrinks (config : PropertyConfig, shrinkLimit: int<shrinks>) : PropertyConfig =
        PropertyConfig.withShrinks shrinkLimit config

    /// Restores the default shrinking behavior.
    [<Extension>]
    static member WithoutShrinks (config : PropertyConfig) : PropertyConfig =
        PropertyConfig.withoutShrinks config

    /// Set the number of times a property should be executed before it is considered successful.
    [<Extension>]
    static member WithTests (config : PropertyConfig, testLimit: int<tests>) : PropertyConfig =
        PropertyConfig.withTests testLimit config


type PropertyConfig =

    /// The default configuration for a property test.
    static member Default : Hedgehog.PropertyConfig =
        PropertyConfig.defaultConfig

        
type Property = private Property of Property<unit> with

    static member Failure : Property =
        Property.failure
        |> Property

    static member Discard : Property =
        Property.discard
        |> Property

    static member Success (value : 'T) : Property<'T> =
        Property.success value

    static member FromBool (value : bool) : Property =
        Property.ofBool value
        |> Property

    static member FromGen (gen : Gen<Journal * Outcome<'T>>) : Property<'T> =
        Property.ofGen gen

    static member FromOutcome (result : Outcome<'T>) : Property<'T> =
        Property.ofOutcome result

    static member FromThrowing (throwingFunc : Action<'T>, arg : 'T) : Property =
        Property.ofThrowing throwingFunc.Invoke arg
        |> Property

    static member Delay (f : Func<Property<'T>>) : Property<'T> =
        Property.delay f.Invoke

    static member Using (resource : 'T, action : Func<'T, Property<'TResult>>) : Property<'TResult> =
        Property.using resource action.Invoke

    static member CounterExample (message : Func<string>) : Property =
        Property.counterexample message.Invoke
        |> Property

    static member ForAll (gen : Gen<'T>, k : Func<'T, Property<'TResult>>) : Property<'TResult> =
        Property.forAll k.Invoke gen

    static member ForAll (gen : Gen<'T>) : Property<'T> =
        Property.forAll' gen

[<Extension>]
[<AbstractClass; Sealed>]
type PropertyExtensions private () =

    [<Extension>]
    static member ToGen (property : Property<'T>) : Gen<Journal * Outcome<'T>> =
        Property.toGen property

    [<Extension>]
    static member TryFinally (property : Property<'T>, onFinally : Action) : Property<'T> =
        Property.tryFinally onFinally.Invoke property

    [<Extension>]
    static member TryWith (property : Property<'T>, onError : Func<exn, Property<'T>>) : Property<'T> =
        Property.tryWith onError.Invoke property

    //
    // Runner
    //

    [<Extension>]
    static member Report (property : Property) : Report =
        let (Property property) = property
        Property.report property

    [<Extension>]
    static member Report (property : Property<bool>) : Report =
        Property.reportBool property

    [<Extension>]
    static member Check
        (   property : Property,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : unit =
        let (Property property) = property
        Property.checkWith (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member Check
        (   property : Property<bool>,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : unit =
        Property.checkBoolWith (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member Check (property : Property, config : Hedgehog.PropertyConfig) : unit =
        let (Property property) = property
        Property.checkWith config property

    [<Extension>]
    static member Check (property : Property<bool>, config : Hedgehog.PropertyConfig) : unit =
        Property.checkBoolWith config property

    [<Extension>]
    static member Recheck
        (   property : Property,
            size : Size,
            seed : Seed,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : unit =
        let (Property property) = property
        Property.recheckWith size seed (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member Recheck
        (   property : Property<bool>,
            size : Size,
            seed : Seed,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : unit =
        Property.recheckBoolWith size seed (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member Recheck (property : Property, size : Size, seed : Seed, config : Hedgehog.PropertyConfig) : unit =
        let (Property property) = property
        Property.recheckWith size seed config property

    [<Extension>]
    static member Recheck (property : Property<bool>, size : Size, seed : Seed, config : Hedgehog.PropertyConfig) : unit =
        Property.recheckBoolWith size seed config property

    [<Extension>]
    static member ReportRecheck
        (   property : Property,
            size : Size,
            seed : Seed,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : Report =
        let (Property property) = property
        Property.reportRecheckWith size seed (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member ReportRecheck
        (   property : Property<bool>,
            size : Size,
            seed : Seed,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : Report =
        Property.reportRecheckBoolWith size seed (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member ReportRecheck (property : Property, size : Size, seed : Seed, config : Hedgehog.PropertyConfig) : Report =
        let (Property property) = property
        Property.reportRecheckWith size seed config property

    [<Extension>]
    static member ReportRecheck (property : Property<bool>, size : Size, seed : Seed, config : Hedgehog.PropertyConfig) : Report =
        Property.reportRecheckBoolWith size seed config property

    [<Extension>]
    static member Print
        (   property : Property,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : unit =
        let (Property property) = property
        Property.printWith (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member Print
        (   property : Property<bool>,
            [<Optional; DefaultParameterValue null>] ?testLimit   : int<tests>,
            [<Optional; DefaultParameterValue null>] ?shrinkLimit : int<shrinks>
        ) : unit =
        Property.printBoolWith (Build.config testLimit shrinkLimit) property

    [<Extension>]
    static member Where (property : Property<'T>, filter : Func<'T, bool>) : Property<'T> =
        Property.filter filter.Invoke property

    [<Extension>]
    static member Select (property : Property<'T>, mapper : Func<'T, 'TResult>) : Property<'TResult> =
        Property.map mapper.Invoke property

    [<Extension>]
    static member Select (property : Property<'T>, mapper : Action<'T>) : Property =
        property
        |> Property.bind (Property.ofThrowing mapper.Invoke)
        |> Property

    [<Extension>]
    static member SelectMany (property : Property<'T>, binder : Func<'T, Property<'TCollection>>, projection : Func<'T, 'TCollection, 'TResult>) : Property<'TResult> =
        property |> Property.bind (fun a ->
            binder.Invoke a |> Property.map (fun b -> projection.Invoke (a, b)))

    [<Extension>]
    static member SelectMany (property : Property<'T>, binder : Func<'T, Property<'TCollection>>, projection : Action<'T, 'TCollection>) : Property =
        let result =
            property |> Property.bind (fun a ->
                binder.Invoke a |> Property.bind (fun b ->
                    Property.ofThrowing projection.Invoke (a, b)))
        Property result

#endif
