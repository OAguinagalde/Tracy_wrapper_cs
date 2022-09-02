using tracy;

public static class TestingProgram {
        
    public static void doSomeWork(int ms) {
        System.Threading.Thread.Sleep(ms);
    }

    public static void Main(string[] args) {
        while (true) {
            doSomeWork(30);
            using var _1 = new Tracy.ProfileScope("testProfiledScope");

            if (true) {
                using var _2 = new Tracy.ProfileScope("testPlots");
                Tracy.PlotValue("myPlot", new System.Random().NextDouble());
                Tracy.SendMessage("finished the if...");
            }

            Tracy.FrameMark("LoopEnd");
        }
    }
}