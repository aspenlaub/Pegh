# Pegh

Summary: a small library that I use as a foundation for my GitHub projects. It allows me to keep objects and algorithms secret, i.e. they are not published on github.

## What does *Pegh* mean?

According to a [Klingon to English Dictionary](http://www.movies-dictionary.org/Klingon-to-English-Dictionary/pegh) the word means *secret*.

A key for me to publish code on github was to turn all 'personal' elements of the code into *secrets* that are not available to anyone.

## Can it be useful to you?

Yes. Have a look at the [Backbend project](https://github.com/aspenlaub/Backbend/) on how to implement and use secrets, that is: secret serializable objects as well as secret algorithms provided in Powershell.

## Honorable mentions

### Interface ```IComponentProvider```

A component provider is an object that provides you with implementations of interfaces. Such an interface could be an XML serializer:

```
    public interface IXmlSerializer {
        string Serialize<TItemType>(TItemType item);
    }
```

If you have a ```componentProvider``` instance, you can use property ```componentProvider.XmlSerializer``` to obtain an implementation.

Component constructors are parameterless, except of course if they use other components itself.

Each constructor or method that expects an ```IComponentProvider``` can be provided with a mock object like e.g. ```componentProviderMock.Object``` to increase testability without heavy dependencies.

### Class ```SecretRepository```

The ```SecretRepository``` is a component, of course. The component exposes a method 

```
public void Set<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new()
```
  
to set a secret and 

```
public TResult Get<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new()
```

to get a secret. Finally it offers to execute a secret algorithm in Powershell for you:

```
public TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class
```
