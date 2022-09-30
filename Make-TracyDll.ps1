param (
    [Switch] $DebugSymbols
)
# Defaults to release mode

if (!(test-path tracy)) {
    
    if (!(get-command git -ErrorAction SilentlyContinue)) {
        throw "git not in path!"
    }

    git clone https://github.com/wolfpld/tracy
    if (!(test-path tracy)) {
        throw "couldn't git clone tracy"
    }

}

if (get-command cl -ErrorAction SilentlyContinue) {
    if ($DebugSymbols) {
        cl /DEBUG /Zi /D_USRDLL /D_WINDLL build-tracy-as-dll.cpp /nologo /link /DLL /OUT:.\tracy.dll
    }
    else {
        cl /O2 /D_USRDLL /D_WINDLL build-tracy-as-dll.cpp /nologo /link /DLL /OUT:.\tracy.dll
    }
}
else {
    throw "cl.exe not in path! You can build the dll with 'cl /D_USRDLL /D_WINDLL build-tracy-as-dll.cpp /nologo /link /DLL /OUT:.\tracy.dll'"
}