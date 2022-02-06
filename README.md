# CustomFilter
The best [OpenTabletDriver](https://github.com/OpentabletDriver/OpenTabletDriver) filter to ever grace this planet!

[![Total Download Count](https://img.shields.io/github/downloads/adryzz/CustomFilter/total.svg)](https://github.com/adryzz/CustomFilter/releases)

Allows you to use any mathematical expression that can be evaluated to a number as a filtering stage!

Uses [AngouriMath](https://github.com/asc-community/AngouriMath) to automatically compile your expressions into code at runtime, to achieve the best performance possible.

All the math is done on [complex numbers](https://en.wikipedia.org/wiki/Complex_number), but the end result is just the [real](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.complex.real?view=net-6.0) part of it

## Simple mode

![image](https://user-images.githubusercontent.com/46694241/152674287-80f94d11-5271-44d7-a11a-a5a9fabe610a.png)

The Simple Mode is the fastest but is limited in the number of samples you can use.
Here's the supported parameters for both expressions:

- `x` = The X coordinate.
- `y` = The Y coordinate.
- `mx` = The max X coordinate.
- `my` = The max Y coordinate.
- `lx` = The last X coordinate.
- `ly` = The last Y coordinate.

#### Example: EMA smoothing
![image](https://user-images.githubusercontent.com/46694241/152674407-eaccdf71-6fb2-448a-9eb4-6bc1c820bac0.png)

## Multi-Sample mode
![image](https://user-images.githubusercontent.com/46694241/152674423-eaded8d6-6158-4cf9-8e23-ed28ebb846e5.png)

The Multi-Sample mode is slower than the Simple Mode, but it allows for more complex expressions.

It will automatically store the last `n` samples from your tablet, so that you can retrieve them with ease.

(e.g. the last X axis sample will be saved as `x0`, the one before as `x1` and so on, same goes with the Y axis).

**Remember to tell the filter how many samples you are going to use, as storing more than what's needed will slow down execution.**

As always, other than the last samples we've just seen, you can always access these values: 

- `x` = The X coordinate.
- `y` = The Y coordinate.
- `mx` = The max X coordinate.
- `my` = The max Y coordinate.
