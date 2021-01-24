# Tocsoft.DateTimeAbstractions

**Tocsoft.DateTimeAbstractions** is a testable alternative to the static `DateTime.Now` and `DateTimeOffset.Now` methods.

[![Build status](https://ci.appveyor.com/api/projects/status/3befgsic0fhiuy5e/branch/master?svg=true)](https://ci.appveyor.com/project/Tocsoft/DateTimeAbstractions/branch/master)
[![codecov](https://codecov.io/gh/Tocsoft/DateTimeAbstractions/branch/master/graph/badge.svg)](https://codecov.io/gh/Tocsoft/DateTimeAbstractions)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/Tocsoft/DateTimeAbstractions/master/LICENSE)

[![GitHub issues](https://img.shields.io/github/issues/Tocsoft/DateTimeAbstractions.svg)](https://github.com/Tocsoft/DateTimeAbstractions/issues)
[![GitHub stars](https://img.shields.io/github/stars/Tocsoft/DateTimeAbstractions.svg)](https://github.com/Tocsoft/DateTimeAbstractions/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/Tocsoft/DateTimeAbstractions.svg)](https://github.com/Tocsoft/DateTimeAbstractions/network)


### Installation

We also have a [MyGet package repository](https://www.myget.org/gallery/tocsoft) - for bleeding-edge / development NuGet releases.

### Usage

Using `Tocsoft.DateTimeAbstractions` is as simple as switching out all usages of `DateTime.Now` with `Clock.Now` and your done your businesslogic thats dependent of on getting the current time is now more testable.

### Using `Tocsoft.DateTimeAbstractions`  from Tests

Now your code is using `Clock.Now` you can now wrap your code under test like so 

```c#
using (Clock.Pin(new DateTime(2018, 02, 02))) 
{  
	// Clock.Now will always return the pinned value until the using block has ended.
	Assert.Equal(new DateTime(2018, 02, 02), Clock.Now);
}
```` 

### Additional APIs 

You probably aren't using DateTime.Now in you code so how can this library help? well we support all the static properties from `DateTime` and `DateTimeOffset` so any of this properties can be switched out and tested.

| System API             | DateTimeAbstractions Replacement |
|------------------------|----------------------------------|
| `DateTime.Now`         | `Clock.Now`                      |
| `DateTime.UtcNow`      | `Clock.UtcNow`                   |
| `DateTime.Today`       | `Clock.Today`                    |
| `DateTimeOffset.Now`   | `ClockOffset.Now`                |
| `DateTimeOffset.UtcNow`| `ClockOffset.UtcNow`             |
> We include warnings and code fixes for all these mappings.

In addition to the `DateTime` set of APIs we also include a testable wrapper around using the `Stopwatch` class in the form of the `ClockTimer` as with the `Clock` APIs we support pinning for testing purposes.

### The Team

Lead
- [Scott Williams](https://github.com/tocsoft)
