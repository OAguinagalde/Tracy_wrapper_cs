# Usage

Add a reference to this project, `dotnet add reference Tracy_wrapper_cs/Tracy_wrapper_cs.csproj`.

Use like this.

```cs
// It has 2 functions, `ProfileStart` and `ProfileEnd`
using static tracy.Tracy;

// This is optional. Only required if using Defer
using static utils.Utils;

// 1 location per profile target must be statically defined
using ProfileLocation = tracy.TracyNative.___tracy_source_location_data;
static ProfileLocation loc1;
static ProfileLocation loc2;

void function(bool condition) {
    
    // Defer will call ProfileEnd(ctx) at the end of the scope
    // giving a name is not necessary
    using var _1 = Defer(ctx => ProfileEnd(ctx), ProfileStart(ref loc1, "function-duration"));

    // work

    if (condition) {
        using var _2 = Defer(ctx => ProfileEnd(ctx), ProfileStart(ref loc2, "extra-logic"));

        // work

        for (int i = 0; i < 10; i++) {
            var context = ProfileStart(ref loc2, "extra-logic");

            // work

            ProfilerEnd(context);
        }
    }
    
    // work

}
```