// Compile this with...
// 
//     cl /D_USRDLL /D_WINDLL build-tracy-as-dll.cpp /nologo /link /DLL /OUT:.\tracy.dll
// 
// Or use the script Make-TracyDll.ps1

#define TRACY_ENABLE
#define TRACY_EXPORTS
#include "tracy/public/TracyClient.cpp"




// Testing with my own c functions

extern "C" {
struct testStruct {
    const char* name;
    const char* function;
    const char* file;
    uint32_t line;
    uint32_t color;
};

__declspec( dllexport ) struct testStruct MyTest(const struct testStruct* object) {
    printf("From C: ");
    printf("name: %s, ", object->name);
    printf("function: %s, ", object->function);
    printf("file: %s, ", object->file);
    printf("line: %d, ", object->line);
    printf("color: %d\n", object->color);
    return *object;
};

__declspec( dllexport ) struct ___tracy_c_zone_context wrapperStart(const struct ___tracy_source_location_data* loc, int active) {
    
    // printf("C - ");
    // printf("name: %s, ", loc->name);
    // printf("function: %s, ", loc->function);
    // printf("file: %s, ", loc->file);
    // printf("line: %d, ", loc->line);
    // printf("color: %d, ", loc->color);
    // printf("active: %d\n", active);

    struct ___tracy_c_zone_context ctx = ___tracy_emit_zone_begin( loc, active );
    return ctx;
}

__declspec( dllexport ) void wrapperEnd(struct ___tracy_c_zone_context ctx) {
    
    // printf("C - ");
    // printf("active: %d, ", ctx.active);
    // printf("id: %d\n", ctx.id);

    ___tracy_emit_zone_end(ctx);
}

}