using tracy;

public static class TestingProgram {
        
    public static void doSomeWork(int ms) {
        System.Threading.Thread.Sleep(ms);
    }

    public static void Main(string[] args) {
        
        while (true) {

            Tracy.EnterFiber("MyFiber");
            doSomeAsyncWork_WORKS(10).Wait();
            Tracy.ExitFiber();
            
            doSomeAsyncWork_FAILS(10).Wait();

            var manualProfile = Tracy.ProfileManually("manualProfile");
            doSomeWork(30);
            manualProfile.End();

            using var _1 = Tracy.ProfileScope("testProfiledScope");

            if (true) {
                using var _2 = Tracy.ProfileScope("testPlots");
                Tracy.PlotValue("myPlot", new System.Random().NextDouble());
                Tracy.SendMessage("finished the if...");
            }

            for (int i = 0; i < 5; i++) {

                using var _3 = Tracy.ProfileScope();
                doSomeWork(10);
            }

            Tracy.FrameMark("LoopEnd");
        }
    }

    public static async System.Threading.Tasks.Task doSomeAsyncWork_WORKS(int ms) {
        {
            using var _ = Tracy.ProfileScope();
            await System.Threading.Tasks.Task.Delay(ms);
        }
        
    }

    public static async System.Threading.Tasks.Task doSomeAsyncWork_FAILS(int ms) {
        using var _ = Tracy.ProfileScope();
        await System.Threading.Tasks.Task.Delay(ms);
    }
}