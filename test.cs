using tracy;

public static class TestingProgram {
        
    // For now we need to make the profiling locations static to ensure its lifetime...
    // TODO Figure out how to get rid of this in a relatively performant way
    private static SourceLocation loc1;
    private static SourceLocation loc2;
    private static SourceLocation loc3;
    
    public static void doSomeWork(int ms) {
        System.Threading.Thread.Sleep(ms);
    }

    public static void Main(string[] args) {
        while (true) {
            using var _1 = new ProfileScope(ref loc1, "testArea");
            doSomeWork(30);
            if (true) {
                using var _2 = new ProfileScope(ref loc2, "if");
                doSomeWork(10);
                for (int i = 0; i < 20; i++) {
                    using var _3 = new ProfileScope(ref loc3, "for");
                    doSomeWork(10);
                }
            }
        }
    }
}