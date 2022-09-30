// Compile this with...
// 
//     cl /D_USRDLL /D_WINDLL build-tracy-as-dll.cpp /nologo /link /DLL /OUT:.\tracy.dll
// 
// Or use the script Make-TracyDll.ps1

#define TRACY_ENABLE
#define TRACY_EXPORTS
#define TRACY_FIBERS
#include "tracy/public/TracyClient.cpp"
